using System;
using System.Collections;
using UnityEngine;

public class treeController : MonoBehaviour
{
    private GameObject treeTop;
    private GameObject treeStump;
    private SpriteRenderer treeTopRenderer;
    private SpriteRenderer treeStumpRenderer;
    private Coroutine activeFadeCoroutine;

    [Tooltip("Duration of the opacity fade in seconds.")]
    public float fadeDuration = 0.25f;

    private void Start()
    {
        //transform.GetChild(0).GetComponent<SpriteRenderer>().sortingOrder = Mathf.RoundToInt((transform.position.y) * -100f);
        //transform.GetChild(1).GetComponent<SpriteRenderer>().sortingOrder = Mathf.RoundToInt((transform.position.y) * -100f) + 1;
        //transform.GetChild(2).GetComponent<SpriteRenderer>().sortingOrder = Mathf.RoundToInt((transform.position.y) * -100f);
        
        treeTop = transform.GetChild(1).gameObject;
        treeTopRenderer = treeTop.GetComponent<SpriteRenderer>();
            
        treeStump = transform.GetChild(0).gameObject;
        treeStumpRenderer = treeStump.GetComponent<SpriteRenderer>();

        float randomScale = UnityEngine.Random.Range(0.8f, 1.2f);
        transform.localScale = new Vector3(randomScale, randomScale, 1);

        Color initialTopColor = treeTopRenderer.material.color;
        treeTopRenderer.material.color = new Color(initialTopColor.r, initialTopColor.g, initialTopColor.b, 1f);
        Color initialStumpColor = treeStumpRenderer.material.color;
        treeStumpRenderer.material.color = new Color(initialStumpColor.r, initialStumpColor.g, initialStumpColor.b, 1f);
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
            if (gameObject.activeInHierarchy) 
            {
                activeFadeCoroutine = StartCoroutine(FadeOpacity(1.0f));
            }
        }
    }

    private IEnumerator FadeOpacity(float targetAlpha)
    {
        Color currentTopColor = treeTopRenderer.material.color;
        Color currentStumpColor = treeStumpRenderer.material.color;
        float startTopAlpha = currentTopColor.a;
        float startStumpAlpha = currentStumpColor.a;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float newTopAlpha = Mathf.Lerp(startTopAlpha, targetAlpha, elapsedTime / fadeDuration);
            float newStumpAlpha = Mathf.Lerp(startStumpAlpha, targetAlpha, elapsedTime / fadeDuration);
            treeTopRenderer.material.color = new Color(currentTopColor.r, currentTopColor.g, currentTopColor.b, newTopAlpha);
            treeStumpRenderer.material.color = new Color(currentStumpColor.r, currentStumpColor.g, currentStumpColor.b, newStumpAlpha);
            yield return null;
        }

        treeTopRenderer.material.color = new Color(currentTopColor.r, currentTopColor.g, currentTopColor.b, targetAlpha);
        treeStumpRenderer.material.color = new Color(currentStumpColor.r, currentStumpColor.g, currentStumpColor.b, targetAlpha);
        activeFadeCoroutine = null;
    }
}
