using System;
using UnityEngine;
using UnityEngine.Rendering.Universal; // Required for Light2D

public class cherryController : MonoBehaviour
{
    [Header("Light Settings")]
    [SerializeField] private Light2D spotLight; // Assign your Light2D component (expected on a child GameObject) here in the Inspector
    [SerializeField] private float lightActivationDistance = 22f; // Distance to turn light on/off

    [Header("Pulse Parameters")]
    [SerializeField] private float minOuterRadius = 2f;
    [SerializeField] private float maxOuterRadius = 3f;
    [SerializeField] private float minIntensity = 0.08f;
    [SerializeField] private float maxIntensity = 0.12f;
    [SerializeField] private float pulseSpeed = 1f; // Controls how fast the pulse occurs

    private float pulseTimer = 0f;

    private GameObject player;
    private GameObject worldCanvas;
    [SerializeField] private GameObject cherryInteractablePrompt;
    private bool isInteractablePromptActive = false;
    [SerializeField] private float minOuterRadiusForPrompt = 1f; // Minimum radius to show the prompt
    private GameObject thisCherryInteractablePrompt;

    // This will hold the GameObject that the spotLight is attached to.
    // This is the object that will be turned on/off.
    private GameObject lightHolderObject; 

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        worldCanvas = GameObject.FindGameObjectWithTag("WorldCanvas");
        
        if (spotLight == null)
        {
            Debug.LogError("CherryController: 'spotLight' has not been assigned in the Inspector on " + gameObject.name + ". Light functionality will be disabled.", this);
            // lightHolderObject will remain null.
        }
        else
        {
            lightHolderObject = spotLight.gameObject; 
            if (lightHolderObject == this.gameObject)
            {
                Debug.LogWarning("CherryController: 'spotLight' is attached to the same GameObject as this script (" + gameObject.name + "). " +
                                 "Turning it off based on distance will also disable this script. " +
                                 "If you intend to turn off only a light visual, ensure the Light2D is on a child GameObject and assign that Light2D component to the 'spotLight' field.", this);
            }
        }
    }

    void Update()
    {
        // --- Light GameObject Activation/Deactivation based on player distance ---
        if (player != null && lightHolderObject != null) // Ensure player exists and light object is set
        {
            float distanceToPlayer = Vector2.Distance(player.transform.position, transform.position);
            bool shouldLightBeActive = distanceToPlayer <= lightActivationDistance;

            // Only change active state if it's different from the current state to avoid unnecessary calls
            if (lightHolderObject.activeSelf != shouldLightBeActive)
            {
                lightHolderObject.SetActive(shouldLightBeActive);
            }
        }

        // --- Light Pulsing Effect ---
        // Only attempt to pulse if the spotLight component is assigned AND its GameObject is currently active
        if (spotLight != null && lightHolderObject != null && lightHolderObject.activeSelf)
        {
            pulseTimer += Time.deltaTime * pulseSpeed;

            float oscillationFactor = (Mathf.Sin(pulseTimer) + 1f) / 2f;

            float currentOuterRadius = Mathf.Lerp(minOuterRadius, maxOuterRadius, oscillationFactor);
            spotLight.pointLightOuterRadius = currentOuterRadius;

            float currentIntensity = Mathf.Lerp(minIntensity, maxIntensity, oscillationFactor);
            spotLight.intensity = currentIntensity;
        }

        // --- Interaction Logic ---
        if (Input.GetKeyDown(KeyCode.E) && isInteractablePromptActive)
        {
            if (player != null)
            {
                playerController pc = player.GetComponent<playerController>();
                if (pc != null)
                {
                    pc.cherryConsumed();
                }
                else
                {
                    Debug.LogWarning("Player object does not have a playerController component.", player);
                }
            }
            else
            {
                Debug.LogWarning("Player object not found for interaction.", this);
            }

            if (thisCherryInteractablePrompt != null)
            {
                Destroy(thisCherryInteractablePrompt);
            }
            Destroy(gameObject); // Destroys the cherry object itself
        }
    }

    private void FixedUpdate()
    {
        if (player == null) 
        {
            // Optionally, try to find player again if it was not found in Start, or just return.
            // player = GameObject.FindGameObjectWithTag("Player"); 
            // if (player == null) return;
            return; 
        }

        float distanceToPlayer = Vector2.Distance(player.transform.position, transform.position);

        if (!isInteractablePromptActive && distanceToPlayer < minOuterRadiusForPrompt)
        {
            isInteractablePromptActive = true;

            Vector3 worldOffset = new Vector3(0f, 0.5f, 0f); 
            Vector3 targetPromptWorldPosition = transform.position + worldOffset;

            if (cherryInteractablePrompt != null && worldCanvas != null)
            {
                GameObject newCherryInteractablePrompt = Instantiate(cherryInteractablePrompt, worldCanvas.transform);
                newCherryInteractablePrompt.transform.position = targetPromptWorldPosition;
                thisCherryInteractablePrompt = newCherryInteractablePrompt;
            }
            else
            {
                if (cherryInteractablePrompt == null) Debug.LogError("'cherryInteractablePrompt' prefab is not assigned in the Inspector on " + gameObject.name, this);
                if (worldCanvas == null) Debug.LogError("'worldCanvas' is not assigned or found (Tag: WorldCanvas) by " + gameObject.name, this);
                isInteractablePromptActive = false; // Revert state as prompt couldn't be created
            }
        }
        else if (isInteractablePromptActive && distanceToPlayer > minOuterRadiusForPrompt)
        {
            isInteractablePromptActive = false;
            if (thisCherryInteractablePrompt != null)
            {
                Destroy(thisCherryInteractablePrompt);
                thisCherryInteractablePrompt = null; // Good practice to nullify after destroying
            }
        }
    }
}