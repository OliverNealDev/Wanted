using System;
using System.Collections;
using UnityEngine;

public class bushController : MonoBehaviour
{
   [SerializeField] private Sprite emptyBushSprite;
   [SerializeField] private Sprite berryBushSprite;
   private SpriteRenderer SR;

   [SerializeField] private float berryImmediateSpawnChance = 0.2f;
   [SerializeField] private float minBerrySpawnTime = 6f;
   [SerializeField] private float maxBerrySpawnTime = 18f;

   private bool isBerryBush = false;
   private Coroutine activeFadeCoroutine;

   [Tooltip("Duration of the opacity fade in seconds.")]
   public float fadeDuration = 0.25f;

   void Start()
   {
      SR = GetComponent<SpriteRenderer>();
      Color initialColor = SR.material.color;
      SR.material.color = new Color(initialColor.r, initialColor.g, initialColor.b, 1f);

      if (UnityEngine.Random.value < berryImmediateSpawnChance)
      {
         isBerryBush = true;
         SR.sprite = berryBushSprite;
      }
      else
      {
         Invoke("growBerries", UnityEngine.Random.Range(minBerrySpawnTime, maxBerrySpawnTime));
      }
   }

   void growBerries()
   {
      if (!isBerryBush)
      {
         isBerryBush = true;
         SR.sprite = berryBushSprite;
      }

      Invoke("growBerries", UnityEngine.Random.Range(minBerrySpawnTime, maxBerrySpawnTime));
   }

   public void onBerriesEaten()
   {
        if (isBerryBush)
        {
             isBerryBush = false;
             SR.sprite = emptyBushSprite;
             Color currentColor = SR.material.color;
             SR.material.color = new Color(currentColor.r, currentColor.g, currentColor.b, 1f);
             
             CancelInvoke("growBerries");
             Invoke("growBerries", UnityEngine.Random.Range(minBerrySpawnTime, maxBerrySpawnTime));
        }
        else
        {
            Debug.LogWarning("Berries are not on bush");
        }
   }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (activeFadeCoroutine != null)
            {
                StopCoroutine(activeFadeCoroutine);
            }
            activeFadeCoroutine = StartCoroutine(FadeOpacity(0.8f));
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (activeFadeCoroutine != null)
            {
                StopCoroutine(activeFadeCoroutine);
            }
            activeFadeCoroutine = StartCoroutine(FadeOpacity(1.0f));
        }
    }

    private IEnumerator FadeOpacity(float targetAlpha)
    {
        Color currentColor = SR.material.color;
        float startAlpha = currentColor.a;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / fadeDuration);
            SR.material.color = new Color(currentColor.r, currentColor.g, currentColor.b, newAlpha);
            yield return null;
        }

        SR.material.color = new Color(currentColor.r, currentColor.g, currentColor.b, targetAlpha);
        activeFadeCoroutine = null;
    }
}