using System;
using System.Collections;
using UnityEngine;

public class treeController : MonoBehaviour
{
    private GameObject treeTop;
    private SpriteRenderer treeTopRenderer;
    private Coroutine activeFadeCoroutine;

    [Tooltip("Duration of the opacity fade in seconds.")]
    public float fadeDuration = 0.25f;

    private void Start()
    {
        if (transform.childCount > 1)
        {
            treeTop = transform.GetChild(1).gameObject;
            treeTopRenderer = treeTop.GetComponent<SpriteRenderer>();

            if (treeTopRenderer == null)
            {
                Debug.LogError("SpriteRenderer not found on treeTop object!", this);
                enabled = false;
                return;
            }
        }
        else
        {
            Debug.LogError("TreeTop child object not found or not enough children!", this);
            enabled = false;
            return;
        }

        float randomScale = UnityEngine.Random.Range(0.9f, 1.1f);
        treeTop.transform.localScale = new Vector3(randomScale, randomScale, 1);

        Color initialColor = treeTopRenderer.material.color;
        treeTopRenderer.material.color = new Color(initialColor.r, initialColor.g, initialColor.b, 1f);
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
        if (treeTopRenderer == null) yield break;

        Color currentColor = treeTopRenderer.material.color;
        float startAlpha = currentColor.a;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / fadeDuration);
            treeTopRenderer.material.color = new Color(currentColor.r, currentColor.g, currentColor.b, newAlpha);
            yield return null;
        }

        treeTopRenderer.material.color = new Color(currentColor.r, currentColor.g, currentColor.b, targetAlpha);
        activeFadeCoroutine = null;
    }
}
