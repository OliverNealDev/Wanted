using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class proximityMineController : MonoBehaviour
{
    [SerializeField] private Sprite pMineOnSprite;
    [SerializeField] private Sprite pMineOffSprite;
    private SpriteRenderer SR;
    
    [SerializeField] private float proximityRadius = 1f;
    private GameObject player;

    private Light2D light;
    private AudioSource audio;
    
    private bool isLit = false;
    private bool isAlerting = false;

    [SerializeField] private AudioClip alertSound;
    [SerializeField] private AudioClip beepSound;
    
    private lawEnforcementManager lawEnforcementManager;

    private Coroutine activeFadeCoroutine;

    void Start()
    {
        lawEnforcementManager = FindObjectOfType<lawEnforcementManager>();
        player = GameObject.FindGameObjectWithTag("Player");
        SR = GetComponent<SpriteRenderer>();
        light = GetComponent<Light2D>();
        audio = GetComponent<AudioSource>();
        
        SR.sprite = pMineOffSprite;
        light.enabled = false;
        
        InvokeRepeating("ToggleLight", 0.5f, 0.5f);
        InvokeRepeating("checkProximityToPlayer", 0.5f, 0.5f);
    }
    
    void ToggleLight()
    {
        if (isAlerting) return;
        
        isLit = !isLit;
        
        if (isLit)
        {
            audio.Play();
        }
        else
        {
            audio.Stop();
        }
        
        Invoke("litVisualDelay", 0.2f);
    }
    
    private void litVisualDelay()
    {
        if (isAlerting) return;
        
        SR.sprite = isLit ? pMineOnSprite : pMineOffSprite;

        if (SR.enabled) 
        {
            light.enabled = isLit;
        }
        else
        {
            light.enabled = false;
        }
    }
    
    private void checkProximityToPlayer()
    {
        if (player == null) return;

        if (Vector2.Distance(transform.position, player.transform.position) <= proximityRadius && isAlerting == false)
        {
            if (activeFadeCoroutine != null)
            {
                StopCoroutine(activeFadeCoroutine);
                activeFadeCoroutine = null;
            }

            isAlerting = true;
            SR.enabled = true;
            SR.sprite = pMineOnSprite;
            light.enabled = true;
            light.intensity *= 10f;
            light.pointLightOuterRadius *= 3f;
            SR.color = new Color(SR.color.r, SR.color.g, SR.color.b, 1f);
            
            audio.clip = alertSound;
            audio.Play();
            lawEnforcementManager.AlertSpottedTransform(new Vector2(transform.position.x, transform.position.y));
            Invoke("Reset", 5f);
        }
    }
    
    private void Reset()
    {
        isAlerting = false;
        audio.clip = beepSound;
        light.intensity /= 10f;
        light.pointLightOuterRadius /= 3f;
        SR.color = new Color(SR.color.r, SR.color.g, SR.color.b, 0.75f); 

        if (player != null && SR.enabled) {
             light.enabled = isLit;
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
        
        Color currentSpriteColor = SR.color;
        float startSpriteAlpha = SR.color.a;
        float targetSpriteAlpha = fadeIn ? 1f : 0f;

        float startLightIntensity = light.intensity;
        float targetLightIntensity = 0f;
        bool finalLightEnabledState = false;

        if (fadeIn)
        {
            if (isAlerting)
            {
                finalLightEnabledState = true;
                targetLightIntensity = light.intensity; 
            }
            else if (isLit)
            {
                finalLightEnabledState = true;
                targetLightIntensity = light.intensity;
            }

            if (finalLightEnabledState && !light.enabled) {
                light.enabled = true;
            }
        }
        else
        {
            targetLightIntensity = 0f;
            finalLightEnabledState = false;
        }
        
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / duration);

            SR.color = new Color(currentSpriteColor.r, currentSpriteColor.g, currentSpriteColor.b, Mathf.Lerp(startSpriteAlpha, targetSpriteAlpha, progress));
            
            if (light.enabled || (fadeIn && finalLightEnabledState))
            {
                light.intensity = Mathf.Lerp(startLightIntensity, targetLightIntensity, progress);
            }
            yield return null;
        }

        SR.color = new Color(currentSpriteColor.r, currentSpriteColor.g, currentSpriteColor.b, targetSpriteAlpha);
        light.intensity = targetLightIntensity;
        light.enabled = finalLightEnabledState;

        if (!fadeIn)
        {
            SR.enabled = false;
        }
        activeFadeCoroutine = null;
    }
}
