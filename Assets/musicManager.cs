using UnityEngine;
using System.Collections;

public class musicManager : MonoBehaviour
{
    private AudioSource audioSource;
    [SerializeField] private AudioClip passiveMusic;
    [SerializeField] private AudioClip chaseMusic;
    [SerializeField] private float fadeDuration = 1.0f; // Duration of the music fade in seconds
    [SerializeField] private float minChaseDuration = 10.0f; // Minimum duration chase music should play

    // Define target volumes
    private const float PASSIVE_MUSIC_VOLUME = 0.05f;
    private const float CHASE_MUSIC_VOLUME = 0.03f;

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
            audioSource.volume = PASSIVE_MUSIC_VOLUME; // Use defined passive music volume
            audioSource.Play();
            currentTargetClip = passiveMusic;
        }
        else
        {
            Debug.LogError("Passive Music AudioClip is not assigned in the Inspector. Music manager may not function correctly.");
        }
        // Initialize chaseMusicStartTime
        chaseMusicStartTime = -minChaseDuration;
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
            }
            else if (currentTargetClip != passiveMusic) // If not targeting chase, and not targeting passive
            {
                // Fallback to passive if detection is low and we're not already on it or committed to chase.
                if (passiveMusic != null)
                {
                    StartFade(passiveMusic);
                }
                else
                {
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

        if (currentTargetClip == newClip)
        {
            // If we are already targeting this clip, and it's playing at the correct volume, do nothing.
            // Check if the current volume matches the target volume for this clip.
            float expectedVolume = 0f;
            if (newClip == passiveMusic) expectedVolume = PASSIVE_MUSIC_VOLUME;
            else if (newClip == chaseMusic) expectedVolume = CHASE_MUSIC_VOLUME;
            else expectedVolume = 1; // Default for other clips

            // If already playing this clip and volume is close enough to target, don't restart fade.
            if (audioSource.clip == newClip && audioSource.isPlaying && Mathf.Approximately(audioSource.volume, expectedVolume))
            {
                return;
            }
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
        float initialVolumeForFadeOut = audioSource.volume; 
        float timer = 0f;

        // Fade out current music
        if (audioSource.isPlaying && audioSource.volume > 0.001f) // Use a very small threshold
        {
            if (audioSource.clip != newClip || !Mathf.Approximately(audioSource.volume, GetTargetVolumeForClip(newClip)))
            {
                while (timer < fadeDuration)
                {
                    audioSource.volume = Mathf.Lerp(initialVolumeForFadeOut, 0f, timer / fadeDuration);
                    timer += Time.deltaTime;
                    yield return null;
                }
            }
            audioSource.volume = 0f;
            // Don't stop if it's the same clip we're fading to (e.g., adjusting volume of current clip)
            if (audioSource.clip != newClip) {
                audioSource.Stop();
            }
        } else if (!audioSource.isPlaying && newClip != null) {
             initialVolumeForFadeOut = 0f; 
        }


        audioSource.clip = newClip;
        if (!audioSource.isPlaying && newClip != null) // Ensure it plays if it was stopped or not playing
        {
            audioSource.Play();
        }

        // Determine target volume for the new clip
        float targetFadeInVolume = GetTargetVolumeForClip(newClip);
        
        // Fade in the new music
        timer = 0f; // Reset timer for fade in
        float currentVolumeForFadeIn = audioSource.volume; 

        while (timer < fadeDuration)
        {
            audioSource.volume = Mathf.Lerp(currentVolumeForFadeIn, targetFadeInVolume, timer / fadeDuration);
            timer += Time.deltaTime;
            yield return null;
        }
        audioSource.volume = targetFadeInVolume; 

        currentFadeCoroutine = null; 
    }

    private float GetTargetVolumeForClip(AudioClip clip)
    {
        if (clip == passiveMusic)
        {
            return PASSIVE_MUSIC_VOLUME;
        }
        else if (clip == chaseMusic)
        {
            return CHASE_MUSIC_VOLUME;
        }
        else
        {
            // Default volume for any other clip
            if (clip != null)
            {
                Debug.LogWarning($"GetTargetVolumeForClip: Clip '{clip.name}' is not specifically passive or chase. Defaulting target volume to 1.0f.");
            }
            return 1.0f; 
        }
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
        audioSource.volume = PASSIVE_MUSIC_VOLUME; // Use defined passive music volume
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
        chaseMusicStartTime = Time.time; 
        audioSource.clip = chaseMusic;
        audioSource.volume = CHASE_MUSIC_VOLUME; // Use defined chase music volume
        audioSource.Play();
        currentTargetClip = chaseMusic;
    }
}
