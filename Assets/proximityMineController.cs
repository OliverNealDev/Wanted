using UnityEngine;
using UnityEngine.Rendering.Universal;

public class proximityMineController : MonoBehaviour
{
    [SerializeField] private Sprite pMineOnSprite;
    [SerializeField] private Sprite pMineOffSprite;
    private SpriteRenderer SR;

    private Light2D light;
    private AudioSource audio;
    
    private bool isLit = false;

    void Start()
    {
        SR = GetComponent<SpriteRenderer>();
        light = GetComponent<Light2D>();
        audio = GetComponent<AudioSource>();
        
        SR.sprite = pMineOffSprite;
        light.enabled = false;
        
        InvokeRepeating("ToggleLight", 0.5f, 0.5f);
    }
    
    void ToggleLight()
    {
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
        SR.sprite = isLit ? pMineOnSprite : pMineOffSprite;

        if (!SR.enabled)
        {
            light.enabled = false;
        }
        else
        {
            light.enabled = isLit;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            SR.enabled = true;
            if (isLit)
            {
                light.enabled = true;
            }
            else
            {
                light.enabled = false;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            SR.enabled = false;
            light.enabled = false;
        }
    }
}
