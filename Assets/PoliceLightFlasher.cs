using UnityEngine;
using UnityEngine.Rendering.Universal; // Required for Light2D
using System.Collections;

public class PoliceLightFlasher : MonoBehaviour
{
    public Light2D redLight;
    public Light2D blueLight;
    public float flashInterval = 0.5f;

    private Coroutine flashingCoroutine;

    void Start()
    {
        if (redLight == null || blueLight == null)
        {
            Debug.LogError("PoliceLightFlasher: Red Light or Blue Light not assigned!");
            return;
        }
        flashingCoroutine = StartCoroutine(FlashLights());
    }

    void OnEnable()
    {
        if (redLight != null && blueLight != null && flashingCoroutine == null)
        {
            flashingCoroutine = StartCoroutine(FlashLights());
        }
    }

    void OnDisable()
    {
        if (flashingCoroutine != null)
        {
            StopCoroutine(flashingCoroutine);
            flashingCoroutine = null;
            if (redLight != null) redLight.enabled = false;
            if (blueLight != null) blueLight.enabled = false;
        }
    }

    IEnumerator FlashLights()
    {
        bool redIsOn = true;
        redLight.enabled = true;
        blueLight.enabled = false;

        while (true)
        {
            yield return new WaitForSeconds(flashInterval);
            redIsOn = !redIsOn;
            redLight.enabled = redIsOn;
            blueLight.enabled = !redIsOn;
        }
    }
}