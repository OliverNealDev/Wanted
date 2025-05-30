using System.IO.Compression;
using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UIElements;
using Slider = UnityEngine.UI.Slider;

public class radarController : MonoBehaviour
{
    [SerializeField] private GameObject radarBit;
    [SerializeField] private float radarBitRotationSpeed;
    [SerializeField] private float timeToScan;
    [SerializeField] private float timeToDisable;

    [SerializeField] private GameObject pingEffectPrefab;
    //[SerializeField] private GameObject RadarPanel;
    //[SerializeField] private TextMeshProUGUI RadarTimer;
    [SerializeField] private Slider radarSlider;
    [SerializeField] private GameObject radarSliderObject;
    private float timePassed = 0;
    
    private GameObject Player;
    private GameObject worldCanvas;
    private GameObject Canvas;
    [SerializeField] private GameObject radarInteractablePrompt;
    private bool isInteractablePromptActive = false;
    [SerializeField] private float minOuterRadiusForPrompt = 1f;
    private GameObject thisRadarInteractablePrompt;
    
    lawEnforcementManager lawEnforcementManager;

    private AudioSource audioSource;
    private GameObject spawnedRadarSliderObject;
    
    private bool isDisabled = false;

    void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player");
        worldCanvas = GameObject.FindGameObjectWithTag("WorldCanvas");
        Canvas = GameObject.FindGameObjectWithTag("Canvas");
        lawEnforcementManager = FindObjectOfType<lawEnforcementManager>();
        audioSource = GetComponent<AudioSource>();
        
        spawnedRadarSliderObject = Instantiate(radarSliderObject, Canvas.transform);
        radarSlider = spawnedRadarSliderObject.GetComponent<Slider>();
        
        //RadarPanel = GameObject.FindGameObjectWithTag("radarPanel");
        //RadarTimer = GameObject.FindGameObjectWithTag("radarTimer").GetComponent<TextMeshProUGUI>();
        //RadarPanel.transform.position = new Vector2(965.1453f, 153.4f);

        timeToScan -= (lawEnforcementManager.timePassed / 10f) - 12; // The 12 is compensation as radars don't spawn until 2 minutes in (12 seconds worth)
        Debug.Log(timeToScan);
        if (timeToScan < 20f)
        {
            timeToScan = 20f; // Ensure minimum scan time
        }

        lawEnforcementManager.radarsSpawned++;

        /*if (!radarSlider.gameObject.activeInHierarchy)
        {
            radarSlider.gameObject.SetActive(true);
        }*/

        lawEnforcementManager.timeSinceRadarSpawned = 0;
        lawEnforcementManager.RadarCooldown = timeToScan + 4;
        
        //Invoke("Scan", timeToScan);
        InvokeRepeating("sonarPing", 1f, 2f);
    }

    void Update()
    {
        if (isDisabled) return;
        
        timePassed += Time.deltaTime;
        
        radarBit.transform.Rotate(0, 0, radarBitRotationSpeed * Time.deltaTime, Space.Self);

        if (Input.GetKeyDown(KeyCode.E) && isInteractablePromptActive)
        {
            if (Player != null)
            {
                playerController pc = Player.GetComponent<playerController>();
                if (pc != null)
                {
                    isDisabled = true;
                    //StopAllCoroutines();
                    CancelInvoke();
                    Invoke("Despawn", 4f);
                }
                else
                {
                    Debug.LogWarning("Player object does not have a playerController component.", Player);
                }
            }
            else
            {
                Debug.LogWarning("Player object not found for interaction.", this);
            }

            if (thisRadarInteractablePrompt != null)
            {
                Destroy(thisRadarInteractablePrompt);
            }
        }
    }

    private void FixedUpdate()
    {
        if (isDisabled) return;
        
        if (Player == null)
        {
            return;
        }

        float distanceToPlayer = Vector2.Distance(Player.transform.position, transform.position);

        if (!isInteractablePromptActive && distanceToPlayer < minOuterRadiusForPrompt)
        {
            isInteractablePromptActive = true;

            Vector3 worldOffset = new Vector3(0f, 7f, 0f);
            Vector3 targetPromptWorldPosition = transform.position + worldOffset;

            if (radarInteractablePrompt != null && worldCanvas != null)
            {
                GameObject newRadarInteractablePrompt = Instantiate(radarInteractablePrompt, worldCanvas.transform);
                newRadarInteractablePrompt.transform.position = targetPromptWorldPosition;
                thisRadarInteractablePrompt = newRadarInteractablePrompt;
            }
            else
            {
                if (radarInteractablePrompt == null) Debug.LogError("'radarInteractablePrompt' prefab is not assigned in the Inspector on " + gameObject.name, this);
                if (worldCanvas == null) Debug.LogError("'worldCanvas' is not assigned or found (Tag: WorldCanvas) by " + gameObject.name, this);
                isInteractablePromptActive = false;
            }
        }
        else if (isInteractablePromptActive && distanceToPlayer > minOuterRadiusForPrompt)
        {
            isInteractablePromptActive = false;
            if (thisRadarInteractablePrompt != null)
            {
                Destroy(thisRadarInteractablePrompt);
                thisRadarInteractablePrompt = null;
            }
        }
    }

    void sonarPing()
    {
        if (audioSource != null)
        {
            audioSource.Play();
        }

        //RadarTimer.text = timeToScan.ToString();
        radarSlider.value = timePassed / timeToScan;
        if (radarSlider.value == 1)
        {
            Scan();
        }
        
        if (pingEffectPrefab != null)
        {
            GameObject pingInstance = Instantiate(pingEffectPrefab, transform.position, Quaternion.identity);
            StartCoroutine(AnimatePing(pingInstance, 4.0f));
        }
        else
        {
            Debug.LogWarning("PingEffectPrefab not assigned on " + gameObject.name, this);
        }
    }

    IEnumerator AnimatePing(GameObject pingInstance, float duration)
    {
        SpriteRenderer sr = pingInstance.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            Debug.LogError("PingEffectPrefab on " + gameObject.name + " is missing a SpriteRenderer component.", pingInstance);
            Destroy(pingInstance);
            yield break;
        }

        float elapsedTime = 0f;
        Vector3 initialScale = Vector3.zero;
        Vector3 targetScale = Vector3.one * 200f;
        Color pingBaseColor = Color.green;

        pingInstance.transform.localScale = initialScale;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            pingInstance.transform.localScale = Vector3.Lerp(initialScale, targetScale, t);
            
            float currentAlpha = Mathf.Lerp(0.05f, 0f, t); // Starting alpha - 0.4, ending alpha - 0
            sr.color = new Color(pingBaseColor.r, pingBaseColor.g, pingBaseColor.b, currentAlpha);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Destroy(pingInstance);
    }

    void Scan()
    {
        if (lawEnforcementManager != null)
        {
           lawEnforcementManager.ChangeDetectionPercentage(1);
        }
        
        if (thisRadarInteractablePrompt != null)
        {
            Destroy(thisRadarInteractablePrompt);
        }
        
        //RadarTimer.text = "REVEALED";
        radarSlider.value = 1;
        
        isDisabled = true;
        //StopAllCoroutines();
        CancelInvoke();
        Invoke("Despawn", 4f);
    }

    void Despawn()
    {
        //RadarTimer.text = "";
        //RadarPanel.transform.position = new Vector2(965.1453f, 1530.4f);
        Destroy(spawnedRadarSliderObject);
        if (thisRadarInteractablePrompt != null)
        {
            Destroy(thisRadarInteractablePrompt);
        }
        Destroy(gameObject);
    }
}