using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class PoliceLightFlasher : MonoBehaviour
{
    public Light2D redLight;
    public Light2D blueLight;
    public float flashInterval = 0.5f;

    private Coroutine flashingCoroutine;
    private GameObject player;
    private bool isCulled = true;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        
        if (redLight == null || blueLight == null)
        {
            Debug.LogError("PoliceLightFlasher: Red Light or Blue Light not assigned!");
            if (this.redLight != null) this.redLight.enabled = false;
            if (this.blueLight != null) this.blueLight.enabled = false;
            return;
        }
        
        redLight.enabled = false;
        blueLight.enabled = false;
    }

    void OnEnable()
    {
        if (player == null)
        {
           player = GameObject.FindGameObjectWithTag("Player");
        }

        if (redLight != null && blueLight != null && flashingCoroutine == null)
        {
        }
    }

    private void FixedUpdate()
    {
        if (player == null)
        {
            if (flashingCoroutine != null)
            {
                StopCoroutine(flashingCoroutine);
                flashingCoroutine = null;
            }
            if (redLight != null && redLight.enabled)
            {
                redLight.enabled = false;
            }
            if (blueLight != null && blueLight.enabled)
            {
                blueLight.enabled = false;
            }
            isCulled = true; 
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);

        if (distanceToPlayer < 22f) 
        {
            if (isCulled) 
            {
                if (redLight != null && blueLight != null)
                {
                    isCulled = false; 
                    if (flashingCoroutine == null)
                    {
                        flashingCoroutine = StartCoroutine(FlashLights());
                    }
                }
            }
        }
        else 
        {
            if (!isCulled) 
            {
                isCulled = true; 
                if (flashingCoroutine != null)
                {
                    StopCoroutine(flashingCoroutine);
                    flashingCoroutine = null;
                }
                if (redLight != null)
                {
                    redLight.enabled = false;
                }
                if (blueLight != null)
                {
                    blueLight.enabled = false;
                }
            }
        }
    }

    void OnDisable()
    {
        if (flashingCoroutine != null)
        {
            StopCoroutine(flashingCoroutine);
            flashingCoroutine = null;
        }
        if (redLight != null)
        {
            redLight.enabled = false;
        }
        if (blueLight != null)
        {
            blueLight.enabled = false;
        }
    }

    IEnumerator FlashLights()
    {
        bool redIsOn = true;
        if (redLight != null) redLight.enabled = true;
        if (blueLight != null) blueLight.enabled = false;

        while (true)
        {
            yield return new WaitForSeconds(flashInterval);
            redIsOn = !redIsOn;
            if (redLight != null) redLight.enabled = redIsOn;
            if (blueLight != null) blueLight.enabled = !redIsOn;
        }
    }
}