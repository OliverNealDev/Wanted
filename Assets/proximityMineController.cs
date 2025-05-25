using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class proximityMineController : MonoBehaviour
{
    [Header("Sprite Settings")]
    [SerializeField] private Sprite pMineOnSprite;
    [SerializeField] private Sprite pMineOffSprite;
    private SpriteRenderer SR;

    [Header("Proximity Settings")]
    [SerializeField] private float proximityRadius = 1f;
    private GameObject player;

    [Header("Light Settings")]
    private Light2D mineLight;
    private float baseLightIntensity;
    private float baseLightOuterRadius;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip alertSound;
    [SerializeField] private AudioClip beepSound;
    private AudioSource audioSource;

    [Header("State & Behavior")]
    [Tooltip("Alpha the mine resets to if visible after an alert sequence.")]
    [SerializeField] private float defaultAlphaAfterReset = 0.75f; 
    private bool isLit = false;
    private bool isAlerting = false;
    
    private lawEnforcementManager lawEnforcementManager;
    private Coroutine activeFadeCoroutine;

    void Start()
    {
        lawEnforcementManager = FindObjectOfType<lawEnforcementManager>();
        player = GameObject.FindGameObjectWithTag("Player");
        
        SR = GetComponent<SpriteRenderer>();
        mineLight = GetComponent<Light2D>();
        audioSource = GetComponent<AudioSource>();

        baseLightIntensity = mineLight.intensity;
        baseLightOuterRadius = mineLight.pointLightOuterRadius;
        
        SR.sprite = pMineOffSprite;
        SR.color = new Color(SR.color.r, SR.color.g, SR.color.b, 0f); 
        SR.enabled = false; 

        mineLight.intensity = 0f;
        mineLight.enabled = false;
        
        InvokeRepeating("ToggleBeepingState", 0.5f, 0.5f); 
        InvokeRepeating("CheckProximityToPlayer", 0.5f, 0.5f);
    }
    
    void ToggleBeepingState() 
    {
        if (isAlerting) return; 
        
        isLit = !isLit;
        
        if (isLit)
        {
            audioSource.clip = beepSound;
            audioSource.Play();
        }
        else
        {
            audioSource.Stop();
        }
        
        Invoke("UpdateBlinkingVisuals", 0.2f);
    }
    
    private void UpdateBlinkingVisuals()
    {
        if (isAlerting || activeFadeCoroutine != null || !SR.enabled) return;
        
        SR.sprite = isLit ? pMineOnSprite : pMineOffSprite;
        mineLight.enabled = isLit;

        if (isLit)
        {
            mineLight.intensity = baseLightIntensity;
            mineLight.pointLightOuterRadius = baseLightOuterRadius;
        }
        else
        {
            mineLight.intensity = 0f; 
        }
    }
    
    private void CheckProximityToPlayer()
    {
        if (player == null || isAlerting) return;

        if (Vector2.Distance(transform.position, player.transform.position) <= proximityRadius)
        {
            if (activeFadeCoroutine != null)
            {
                StopCoroutine(activeFadeCoroutine);
                activeFadeCoroutine = null; 
            }

            isAlerting = true;
            SR.sprite = pMineOnSprite; 
            SR.color = new Color(SR.color.r, SR.color.g, SR.color.b, 1f); 
            
            if(!SR.enabled) SR.enabled = true;

            mineLight.enabled = true;
            mineLight.intensity = baseLightIntensity * 3f;
            mineLight.pointLightOuterRadius = baseLightOuterRadius * 3f;
            
            audioSource.Stop(); 
            audioSource.clip = alertSound;
            audioSource.Play();

            if (lawEnforcementManager != null)
            {
                lawEnforcementManager.AlertSpottedTransform(new Vector2(transform.position.x, transform.position.y));
            }
            Invoke("ResetMineState", 5f);
        }
    }
    
    private void ResetMineState()
    {
        isAlerting = false;
        audioSource.Stop(); 
        audioSource.clip = beepSound; 
        
        SR.sprite = isLit ? pMineOnSprite : pMineOffSprite;
        mineLight.enabled = isLit;
        if (isLit)
        {
            mineLight.intensity = baseLightIntensity;
            mineLight.pointLightOuterRadius = baseLightOuterRadius;
        }
        else
        {
            mineLight.intensity = 0f;
            mineLight.pointLightOuterRadius = baseLightOuterRadius; 
        }

        if (SR.enabled) 
        {
            SR.color = new Color(SR.color.r, SR.color.g, SR.color.b, defaultAlphaAfterReset); 
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (activeFadeCoroutine != null)
            {
                StopCoroutine(activeFadeCoroutine);
            }
            SR.enabled = true; 
            activeFadeCoroutine = StartCoroutine(FadeMineVisuals(true, 0.25f));
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (!isAlerting) 
            {
                if (activeFadeCoroutine != null)
                {
                    StopCoroutine(activeFadeCoroutine);
                }
                activeFadeCoroutine = StartCoroutine(FadeMineVisuals(false, 0.25f));
            }
        }
    }

    private IEnumerator FadeMineVisuals(bool fadeIn, float duration)
    {
        float timer = 0f;
        Color initialSpriteColorForLerp = SR.color; 
        float startSpriteAlpha = SR.color.a;       
        float targetSpriteAlpha;

        float lightFadeOut_StartIntensity = 0f;
        float lightFadeOut_StartRadius = 0f;
        bool lightWasEnabledForFadeOut = false;

        if (fadeIn)
        {
            targetSpriteAlpha = 1f;

            if (isAlerting)
            {
                mineLight.enabled = true;
                mineLight.intensity = baseLightIntensity * 3f;
                mineLight.pointLightOuterRadius = baseLightOuterRadius * 3f;
            }
            else if (isLit) 
            {
                mineLight.enabled = true;
                mineLight.intensity = baseLightIntensity;
                mineLight.pointLightOuterRadius = baseLightOuterRadius;
            }
            else 
            {
                mineLight.enabled = false;
                mineLight.intensity = 0f;
            }
        }
        else 
        {
            targetSpriteAlpha = 0f;

            lightWasEnabledForFadeOut = mineLight.enabled;
            if (lightWasEnabledForFadeOut)
            {
                lightFadeOut_StartIntensity = mineLight.intensity;
                lightFadeOut_StartRadius = mineLight.pointLightOuterRadius;
            }
        }

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / duration);

            SR.color = new Color(initialSpriteColorForLerp.r, initialSpriteColorForLerp.g, initialSpriteColorForLerp.b, Mathf.Lerp(startSpriteAlpha, targetSpriteAlpha, progress));

            if (!fadeIn && lightWasEnabledForFadeOut) 
            {
                mineLight.intensity = Mathf.Lerp(lightFadeOut_StartIntensity, 0f, progress);
                mineLight.pointLightOuterRadius = Mathf.Lerp(lightFadeOut_StartRadius, 0f, progress);
            }
            yield return null;
        }

        SR.color = new Color(initialSpriteColorForLerp.r, initialSpriteColorForLerp.g, initialSpriteColorForLerp.b, targetSpriteAlpha);

        if (!fadeIn) 
        {
            mineLight.intensity = 0f;
            mineLight.pointLightOuterRadius = 0f; 
            mineLight.enabled = false;
            SR.enabled = false;
        }
        
        activeFadeCoroutine = null;
    }
}