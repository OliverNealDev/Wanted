using UnityEngine;
using System.Collections;

public class musicManager : MonoBehaviour
{
    private AudioSource audioSource;
    [SerializeField] private AudioClip passiveMusic;
    [SerializeField] private AudioClip chaseMusic;
    [SerializeField] private float fadeDuration = 1.0f;
    [SerializeField] private float minChaseDuration = 10.0f;

    private const float PASSIVE_MUSIC_BASE_VOLUME = 0.45f * 1.5f;
    private const float CHASE_MUSIC_BASE_VOLUME = 0.27f * 1.5f;
    private const string MUSIC_VOLUME_KEY = "MusicVolume";

    private GameObject player;
    private lawEnforcementManager lawEnforcementManager;
    private Coroutine currentFadeCoroutine;
    private AudioClip currentTargetClip;
    private float chaseMusicStartTime;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogWarning("No AudioSource found on musicManager. Adding one.");
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.loop = true;

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

        if (passiveMusic != null)
        {
            audioSource.clip = passiveMusic;
            audioSource.volume = GetFinalVolumeForClip(passiveMusic);
            audioSource.Play();
            currentTargetClip = passiveMusic;
        }
        else
        {
            Debug.LogError("Passive Music AudioClip is not assigned in the Inspector. Music manager may not function correctly.");
        }
        chaseMusicStartTime = -minChaseDuration;
    }

    void FixedUpdate()
    {
        if (lawEnforcementManager == null) return;

        bool isDetectionHigh = lawEnforcementManager.detectionPercentage >= 1.0f;

        if (isDetectionHigh)
        {
            if (currentTargetClip != chaseMusic)
            {
                if (chaseMusic != null)
                {
                    chaseMusicStartTime = Time.time;
                    StartFade(chaseMusic);
                }
                else
                {
                    Debug.LogWarning("Chase Music AudioClip is not assigned. Cannot switch.");
                }
            }
        }
        else
        {
            if (currentTargetClip == chaseMusic)
            {
                if (Time.time - chaseMusicStartTime >= minChaseDuration)
                {
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
            else if (currentTargetClip != passiveMusic)
            {
                if (passiveMusic != null)
                {
                    StartFade(passiveMusic);
                }
                else
                {
                    if (currentTargetClip == null)
                    {
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

        float finalTargetVolume = GetFinalVolumeForClip(newClip);

        if (currentTargetClip == newClip && audioSource.clip == newClip && audioSource.isPlaying && Mathf.Approximately(audioSource.volume, finalTargetVolume))
        {
            return;
        }
        
        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
        }
        currentTargetClip = newClip;
        currentFadeCoroutine = StartCoroutine(FadeMusic(newClip, finalTargetVolume));
    }

    private IEnumerator FadeMusic(AudioClip newClip, float targetVolume)
    {
        float initialVolumeForFadeOut = audioSource.volume;
        float timer = 0f;

        if (audioSource.isPlaying && audioSource.volume > 0.001f)
        {
            if (audioSource.clip != newClip || !Mathf.Approximately(audioSource.volume, targetVolume)) {
                while (timer < fadeDuration)
                {
                    audioSource.volume = Mathf.Lerp(initialVolumeForFadeOut, 0f, timer / fadeDuration);
                    timer += Time.deltaTime;
                    yield return null;
                }
            }
            audioSource.volume = 0f;
            if (audioSource.clip != newClip) {
                audioSource.Stop();
            }
        } else if (!audioSource.isPlaying && newClip != null) {
             initialVolumeForFadeOut = 0f; 
        }

        if (audioSource.clip != newClip) {
            audioSource.clip = newClip;
        }
        if (!audioSource.isPlaying && newClip != null)
        {
            audioSource.Play();
        }
        
        timer = 0f;
        float currentVolumeForFadeIn = audioSource.volume; 

        while (timer < fadeDuration)
        {
            audioSource.volume = Mathf.Lerp(currentVolumeForFadeIn, targetVolume, timer / fadeDuration);
            timer += Time.deltaTime;
            yield return null;
        }
        audioSource.volume = targetVolume;

        currentFadeCoroutine = null;
    }

    private float GetBaseVolumeForClip(AudioClip clip)
    {
        if (clip == passiveMusic)
        {
            return PASSIVE_MUSIC_BASE_VOLUME;
        }
        else if (clip == chaseMusic)
        {
            return CHASE_MUSIC_BASE_VOLUME;
        }
        else
        {
            if (clip != null)
            {
                Debug.LogWarning($"GetBaseVolumeForClip: Clip '{clip.name}' is not specifically passive or chase. Defaulting base volume to 1.0f.");
            }
            return 1.0f;
        }
    }

    private float GetFinalVolumeForClip(AudioClip clip)
    {
        if (clip == null) return 0f;
        float baseVolume = GetBaseVolumeForClip(clip);
        float globalMusicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 1.0f);
        return baseVolume * globalMusicVolume;
    }

    public void UpdateMusicVolume()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            float newTargetVolume = GetFinalVolumeForClip(currentTargetClip);
            if (currentFadeCoroutine != null) {
                StopCoroutine(currentFadeCoroutine); 
            }
            currentFadeCoroutine = StartCoroutine(FadeToNewVolume(newTargetVolume));
        }
         else if (audioSource != null && audioSource.clip != null) { // If not playing but a clip is loaded
            audioSource.volume = GetFinalVolumeForClip(audioSource.clip);
        }
    }
    
    private IEnumerator FadeToNewVolume(float newFinalVolume)
    {
        float currentActualVolume = audioSource.volume;
        float t = 0f;
        float adjustmentFadeDuration = fadeDuration / 2f; 
        if (adjustmentFadeDuration <= 0) adjustmentFadeDuration = 0.1f; // Ensure some duration

        while (t < adjustmentFadeDuration)
        {
            audioSource.volume = Mathf.Lerp(currentActualVolume, newFinalVolume, t / adjustmentFadeDuration);
            t += Time.deltaTime;
            yield return null;
        }
        audioSource.volume = newFinalVolume;
        currentFadeCoroutine = null; 
    }

    public void PlayPassiveMusicInstantly()
    {
        if (audioSource == null || passiveMusic == null) return;
        if (currentFadeCoroutine != null) StopCoroutine(currentFadeCoroutine);
        currentFadeCoroutine = null;
        audioSource.clip = passiveMusic;
        audioSource.volume = GetFinalVolumeForClip(passiveMusic);
        audioSource.Play();
        currentTargetClip = passiveMusic;
    }

    public void PlayChaseMusicInstantly()
    {
        if (audioSource == null || chaseMusic == null) return;
        if (currentFadeCoroutine != null) StopCoroutine(currentFadeCoroutine);
        currentFadeCoroutine = null;
        chaseMusicStartTime = Time.time;
        audioSource.clip = chaseMusic;
        audioSource.volume = GetFinalVolumeForClip(chaseMusic);
        audioSource.Play();
        currentTargetClip = chaseMusic;
    }
}