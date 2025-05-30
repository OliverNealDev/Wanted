using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization; // Required for Light2D

public class cherryController : MonoBehaviour
{
    [Header("Light Settings")]
    [SerializeField] private Light2D spotLight; // Assign your Light2D component here
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
    
    private proceduralMap _proceduralMap;
    
    private AudioSource _audioSource;
    [SerializeField] private GameObject cherrySoundObject;
    [SerializeField] private AudioClip promptSound;
    
    // Removed: private GameObject lightHolderObject; 
    // We will now directly control the spotLight component's enabled state.

    void Start()
    {
        _proceduralMap =  GameObject.FindGameObjectWithTag("proceduralMap").GetComponent<proceduralMap>();
        player = GameObject.FindGameObjectWithTag("Player");
        worldCanvas = GameObject.FindGameObjectWithTag("WorldCanvas");
        _audioSource = GetComponent<AudioSource>();
        
        if (spotLight == null)
        {
            Debug.LogError("CherryController: 'spotLight' has not been assigned in the Inspector on " + gameObject.name + ". Light functionality will be disabled.", this);
        }
        else
        {
            // Check if the Light2D is on the same GameObject as this script
            if (spotLight.gameObject == this.gameObject)
            {
                // Updated warning: The script now handles this case by toggling the component, not the GameObject.
                Debug.LogWarning("CherryController: 'spotLight' is attached to the same GameObject as this script (" + gameObject.name + "). " +
                                 "The script will toggle the Light2D component's 'enabled' state directly to prevent disabling itself. " +
                                 "For cleaner organization, or if other components on a dedicated light object also need toggling with distance, " +
                                 "consider placing the Light2D on a child GameObject and assigning that Light2D component to the 'spotLight' field.", this);
            }
            // No specific warning needed if it's on a different GameObject (e.g., a child), 
            // as toggling spotLight.enabled will work correctly for that setup too.
        }
    }

    void Update()
    {
        // --- Light Component Activation/Deactivation based on player distance ---
        if (player != null && spotLight != null) // Ensure player and Light2D component exist
        {
            float distanceToPlayer = Vector2.Distance(player.transform.position, transform.position);
            bool shouldLightBeEnabled = distanceToPlayer <= lightActivationDistance;

            // Only change enabled state if it's different from the current state to avoid unnecessary calls
            if (spotLight.enabled != shouldLightBeEnabled)
            {
                spotLight.enabled = shouldLightBeEnabled;
            }

            // --- Light Pulsing Effect ---
            // Only attempt to pulse if the spotLight component is assigned AND it is currently enabled
            // The spotLight != null check is implicitly covered by the outer 'if' condition.
            if (spotLight.enabled) 
            {
                pulseTimer += Time.deltaTime * pulseSpeed;

                // Mathf.Sin returns values between -1 and 1. We map this to 0-1.
                float oscillationFactor = (Mathf.Sin(pulseTimer) + 1f) / 2f; 

                float currentOuterRadius = Mathf.Lerp(minOuterRadius, maxOuterRadius, oscillationFactor);
                spotLight.pointLightOuterRadius = currentOuterRadius;

                float currentIntensity = Mathf.Lerp(minIntensity, maxIntensity, oscillationFactor);
                spotLight.intensity = currentIntensity;
            }
        }
        // If player or spotLight is null, or if spotLight is disabled by distance, light logic (including pulsing) won't execute.

        // --- Interaction Logic ---
        // This part remains unchanged as it's not directly related to the light activation bug.
        if (Input.GetKeyDown(KeyCode.E) && isInteractablePromptActive)
        {
            if (player != null)
            {
                playerController pc = player.GetComponent<playerController>();
                if (pc != null)
                {
                    Instantiate(cherrySoundObject, transform.position, Quaternion.identity);
                    pc.cherryConsumed();
                    if (_proceduralMap != null) // Good practice to check if _proceduralMap is assigned
                    {
                        _proceduralMap.GenerateBush();
                    }
                    else
                    {
                        Debug.LogWarning("CherryController: '_proceduralMap' is not assigned. Cannot generate bush.", this);
                    }
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
        // This logic remains unchanged.
        if (player == null) 
        {
            return; 
        }

        float distanceToPlayer = Vector2.Distance(player.transform.position, transform.position);

        if (!isInteractablePromptActive && distanceToPlayer < minOuterRadiusForPrompt)
        {
            _audioSource.clip = promptSound;
            _audioSource.Play();
            isInteractablePromptActive = true;

            Vector3 worldOffset = new Vector3(0f, 7f, 0f); 
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
