using UnityEngine;
using System.Collections;

public class musicManager : MonoBehaviour
{
    private AudioSource audioSource;
    [SerializeField] private AudioClip passiveMusic;
    [SerializeField] private AudioClip chaseMusic;
    [SerializeField] private float fadeDuration = 1.0f; // Duration of the music fade in seconds
    [SerializeField] private float minChaseDuration = 10.0f; // Minimum duration chase music should play

    private GameObject player;
    private lawEnforcementManager lawEnforcementManager;
    private Coroutine currentFadeCoroutine;
    private AudioClip currentTargetClip; // To keep track of the clip we are fading to
    private float chaseMusicStartTime; // Time when chase music was initiated

    void Start()
    {
        // Ensure there's an AudioSource component attached, add one if not.
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogWarning("No AudioSource found on musicManager. Adding one.");
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.loop = true; // Music should always loop

        lawEnforcementManager = FindObjectOfType<lawEnforcementManager>();
        if (lawEnforcementManager == null)
        {
            Debug.LogError("lawEnforcementManager not found in the scene. Music transitions might not work.");
        }

        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Player not found in the scene.");
        }

        // Start with passive music, no fade initially
        if (passiveMusic != null)
        {
            audioSource.clip = passiveMusic;
            audioSource.volume = 1f; // Start at full volume
            audioSource.Play();
            currentTargetClip = passiveMusic;
        }
        else
        {
            Debug.LogError("Passive Music AudioClip is not assigned in the Inspector. Music manager may not function correctly.");
        }
        // Initialize chaseMusicStartTime to a value that won't prematurely block switching back from chase if it somehow started as chase.
        // Effectively, Time.time will be greater than 0, so Time.time - 0 >= minChaseDuration will be true if minChaseDuration is not excessively large.
        // This is mainly relevant if the game starts directly in a chase state without a proper transition.
        chaseMusicStartTime = -minChaseDuration; // Ensures that if somehow currentTargetClip is chaseMusic initially, it can switch if conditions met.
    }

    void FixedUpdate()
    {
        if (lawEnforcementManager == null) return; // Don't proceed if lawEnforcementManager is missing

        bool isDetectionHigh = lawEnforcementManager.detectionPercentage >= 1.0f;

        if (isDetectionHigh)
        {
            // If not already targeting chase music, switch to it
            if (currentTargetClip != chaseMusic)
            {
                if (chaseMusic != null)
                {
                    // Record the time when the intent to play chase music starts
                    chaseMusicStartTime = Time.time;
                    StartFade(chaseMusic);
                }
                else
                {
                    Debug.LogWarning("Chase Music AudioClip is not assigned. Cannot switch.");
                }
            }
            // If already targeting/playing chase music and detection is still high, do nothing.
            // The minChaseDuration timer is running from its initial start.
        }
        else // Detection is low
        {
            // If currently targeting/playing chase music
            if (currentTargetClip == chaseMusic)
            {
                // Check if minimum duration has passed since chase music was initiated
                if (Time.time - chaseMusicStartTime >= minChaseDuration)
                {
                    // Min duration passed, and detection is low, so switch to passive
                    if (passiveMusic != null)
                    {
                        StartFade(passiveMusic);
                    }
                    else
                    {
                        Debug.LogWarning("Passive Music AudioClip is not assigned. Cannot switch back.");
                    }
                }
                // Else, min duration NOT passed, so continue chase music even though detection is low.
            }
            else if (currentTargetClip != passiveMusic) // If not targeting chase, and not targeting passive (e.g. initial state or error)
            {
                // Fallback to passive if detection is low and we're not already on it or committed to chase.
                if (passiveMusic != null)
                {
                    StartFade(passiveMusic);
                }
                else
                {
                    // Cannot switch to passive if not assigned.
                    // Consider stopping music or logging error if currentTargetClip is also null.
                    if(currentTargetClip == null) {
                         Debug.LogWarning("Passive Music AudioClip is not assigned, and no current music target. Music will be silent or unchanged.");
                    }
                }
            }
        }
    }

    public void StartFade(AudioClip newClip)
    {
        if (newClip == null)
        {
            Debug.LogWarning("Cannot fade to a null AudioClip.");
            return;
        }

        // If we're already targeting this clip and it's playing (or about to play via fade), do nothing
        if (currentTargetClip == newClip)
        {
            // If it's the target, and the audio source is already playing this clip and at full volume (or fading in to it),
            // we might not need to restart the fade.
            // However, simply checking currentTargetClip == newClip handles preventing re-triggering the same fade.
            return;
        }
        
        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
        }
        currentTargetClip = newClip; // Set the new target immediately
        currentFadeCoroutine = StartCoroutine(FadeMusic(newClip));
    }

    private IEnumerator FadeMusic(AudioClip newClip)
    {
        float initialVolumeForFadeOut = audioSource.volume; // Volume to fade from
        float timer = 0f;

        // Fade out current music (if any is playing and volume is > 0)
        if (audioSource.isPlaying && audioSource.volume > 0.01f) // Use a small threshold for volume
        {
            // Only fade out if the clip is actually changing
            if (audioSource.clip != newClip) {
                while (timer < fadeDuration)
                {
                    audioSource.volume = Mathf.Lerp(initialVolumeForFadeOut, 0f, timer / fadeDuration);
                    timer += Time.deltaTime;
                    yield return null;
                }
                audioSource.volume = 0f;
                audioSource.Stop(); // Stop after fade out
            } else {
                 // If it's the same clip, but we are here, it might be to ensure it's playing and at target volume
                 // This case should ideally be handled by the StartFade checks, but as a safeguard:
                 initialVolumeForFadeOut = audioSource.volume; // re-evaluate current volume for fade-in
            }
        } else if (!audioSource.isPlaying && newClip != null) {
             initialVolumeForFadeOut = 0f; // If not playing, we're fading in from 0
        }


        audioSource.clip = newClip;
        if (!audioSource.isPlaying) // Ensure it plays if it was stopped or not playing
        {
            audioSource.Play();
        }

        // Fade in the new music
        timer = 0f; // Reset timer for fade in
        float targetFadeInVolume = 1.0f; // Default target volume for new clip is full volume

        // If the previous clip was playing at a certain volume, we might want to fade in to that,
        // but typically, new music comes in at full volume. For simplicity, always fade to 1.0f.
        // If specific start volumes are needed, this logic would be more complex.

        float currentVolumeForFadeIn = audioSource.volume; // Current volume before fade-in starts (could be 0 or partial from previous fade)

        while (timer < fadeDuration)
        {
            audioSource.volume = Mathf.Lerp(currentVolumeForFadeIn, targetFadeInVolume, timer / fadeDuration);
            timer += Time.deltaTime;
            yield return null;
        }
        audioSource.volume = targetFadeInVolume; // Ensure it reaches the target volume

        currentFadeCoroutine = null; // Mark fade as complete
    }

    public void PlayPassiveMusicInstantly()
    {
        if (audioSource == null || passiveMusic == null)
        {
            Debug.LogError("AudioSource or Passive Music not set for instant play.");
            return;
        }
        if (currentFadeCoroutine != null) StopCoroutine(currentFadeCoroutine);
        currentFadeCoroutine = null;
        audioSource.clip = passiveMusic;
        audioSource.volume = 1f;
        audioSource.Play();
        currentTargetClip = passiveMusic;
    }

    public void PlayChaseMusicInstantly()
    {
        if (audioSource == null || chaseMusic == null)
        {
            Debug.LogError("AudioSource or Chase Music not set for instant play.");
            return;
        }
        if (currentFadeCoroutine != null) StopCoroutine(currentFadeCoroutine);
        currentFadeCoroutine = null;
        chaseMusicStartTime = Time.time; // Update start time even for instant play
        audioSource.clip = chaseMusic;
        audioSource.volume = 1f;
        audioSource.Play();
        currentTargetClip = chaseMusic;
    }
}
