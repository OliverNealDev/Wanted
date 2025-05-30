using System;
using System.Collections.Generic;
using NavMeshPlus.Components;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class policeOfficerController : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Vector2 target;

    [Header("Movement & Rotation")]
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseSpeed = 3f;

    [Header("Flashlight Sweep Behavior")]
    [SerializeField] private float minSweepInterval = 3f;
    [SerializeField] private float maxSweepInterval = 5f;
    [SerializeField] private float sweepDuration = 2.5f;
    [SerializeField] private float maxSweepAngle = 20f;
    [SerializeField] private float sweepOscillationSpeed = 1.5f;

    [Header("Flashlight Alert Settings")]
    [SerializeField] private Color chaseFlashlightColor = Color.red;
    [SerializeField] private float chaseFlashlightIntensityMultiplier = 2f;

    [Header("Equipment Settings")]
    [SerializeField] private GameObject proximityMinePrefab;
    [SerializeField] private int proximityMineCount = 1;
    [SerializeField] private float proximityMineMinimumCooldownTime = 30f;
    [SerializeField] private float proximityMineMaximumCooldownTime = 90f;
    
    [SerializeField] private GameObject radarPrefab;
    [SerializeField] private float radarMinimumCooldownTime = 90f;
    [SerializeField] private float radarMaximumCooldownTime = 180f;

    [Header("Raycast Settings")]
    [SerializeField] private LayerMask visionObstructingLayers;
    [SerializeField] private Vector2 raycastOriginOffset = new Vector2(0, 0.5f);
    [SerializeField] private float lkpSampleRadius = 1.0f;

    [Header("Capture Settings")]
    [SerializeField] private float playerTouchDistance = 0.75f;

    private Light2D officerFlashlight;
    NavMeshAgent agent;

    private float nextSweepTimer;
    private bool isCurrentlySweeping = false;
    private float currentSweepActiveTime;

    private lawEnforcementManager lawEnforcementManager;
    private GameObject player;

    private enum OfficerState
    {
        Patrolling,
        Chasing,
        Searching
    }

    private OfficerState currentState;
    private Vector2 lastKnownPlayerPosition;
    private bool canSeePlayerCurrentFrame;
    private bool hasReachedLKPInCurrentSearch = false;
    
    private static bool isTimeFrozen = false;

    private Color originalFlashlightColor;
    private float originalFlashlightIntensity;


    void Start()
    {
        Time.timeScale = 1f;
        isTimeFrozen = false;

        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Player GameObject not found. Make sure it's tagged 'Player'.");
        }

        lawEnforcementManager = FindObjectOfType<lawEnforcementManager>();
        if (lawEnforcementManager == null)
        {
            Debug.LogError("lawEnforcementManager not found in the scene.");
        }

        if (transform.childCount > 0)
        {
            Transform firstChild = transform.GetChild(0);
            officerFlashlight = firstChild.GetComponent<Light2D>();
            if (officerFlashlight == null)
            {
                Debug.LogError("No Light2D component found on the first child of '" + gameObject.name + "'.");
            }
            else
            {
                originalFlashlightColor = officerFlashlight.color;
                originalFlashlightIntensity = officerFlashlight.intensity;
                officerFlashlight.enabled = false;
            }
        }
        else
        {
            Debug.LogError("Police officer '" + gameObject.name + "' has no children; cannot find Light2D.");
        }

        float randomSpeedPercentageModifier = Random.Range(0.9f, 1.1f);
        patrolSpeed *= randomSpeedPercentageModifier;
        chaseSpeed *= randomSpeedPercentageModifier;

        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("NavMeshAgent component not found on this GameObject.");
            if (officerFlashlight != null) officerFlashlight.enabled = false;
            return;
        }

        agent.updateRotation = false;
        agent.updateUpAxis = false;

        currentState = OfficerState.Patrolling;
        canSeePlayerCurrentFrame = false;

        if (searchNodes.instance != null && searchNodes.instance.searchNodesList.Count > 0)
        {
            target = searchNodes.instance.searchNodesList[Random.Range(0, searchNodes.instance.searchNodesList.Count)].position;
            if (agent.isOnNavMesh)
            {
                agent.SetDestination(target);
                agent.speed = patrolSpeed;
            }
            else
            {
                Debug.LogWarning("Agent is not on NavMesh at Start. Cannot set initial destination or speed for " + gameObject.name);
                if (officerFlashlight != null) officerFlashlight.enabled = false;
            }
        }
        else
        {
            target = transform.position;
            Debug.LogWarning("SearchNodes instance not found or nodeList is empty. Officer " + gameObject.name + " may not have a patrol target and will remain stationary.");
            if (agent.isOnNavMesh)
            {
                agent.speed = patrolSpeed;
            }
        }

        SetNewSweepInterval();
        if (proximityMineCount > 0 && proximityMinePrefab != null)
        {
            Invoke("placeProximityMine", Random.Range(proximityMineMinimumCooldownTime, proximityMineMaximumCooldownTime));
        }
        if (radarPrefab != null)
        {
            Invoke("placeRadar", Random.Range(radarMinimumCooldownTime, radarMaximumCooldownTime));
        }
        else if (proximityMinePrefab == null && proximityMineCount > 0)
        {
            Debug.LogWarning("ProximityMinePrefab is not assigned for " + gameObject.name + ", but proximityMineCount is > 0. Officer cannot place mines.");
        }
    }

    void Update()
    {
        if (isTimeFrozen)
        {
            if (agent != null && agent.isOnNavMesh && agent.isActiveAndEnabled && !agent.isStopped)
            {
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
            }
            UpdateFlashlightStatus();
            return;
        }
        else if (agent != null && agent.isOnNavMesh && agent.isActiveAndEnabled && agent.isStopped)
        {
             agent.isStopped = false;
        }


        if (player == null || agent == null || !agent.isOnNavMesh)
        {
            if (officerFlashlight != null && (player == null || agent == null || !agent.isOnNavMesh))
            {
                officerFlashlight.enabled = false;
            }
            canSeePlayerCurrentFrame = false;
            UpdateFlashlightStatus();
            return;
        }
        
        CheckForPlayerTouch();
        if (isTimeFrozen)
        {
            UpdateFlashlightStatus();
            return;
        }

        HandleStateTransitions();
        ExecuteCurrentStateBehavior();
        HandleSweepingState();
        RotateTowardsTargetWithSweep();
        UpdateFlashlightStatus();
    }

    void CheckForPlayerTouch()
    {
        if (isTimeFrozen || player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
        if (distanceToPlayer < playerTouchDistance)
        {
            Debug.Log("PLAYER TOUCHED by " + gameObject.name + "! Time Freezing.");
            if (lawEnforcementManager.timePassed > PlayerPrefs.GetFloat("Highscore"))
            {
                PlayerPrefs.SetFloat("Highscore", lawEnforcementManager.timePassed);
            }
            Time.timeScale = 0.01f;
            isTimeFrozen = true;
            Invoke("restartGame", 0.04f);

            if (agent != null && agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
            }
        }
    }

    void restartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }


    void HandleStateTransitions()
    {
        if (canSeePlayerCurrentFrame)
        {
            if (currentState != OfficerState.Chasing)
            {
                currentState = OfficerState.Chasing;
                hasReachedLKPInCurrentSearch = false; 
                if (player != null) lastKnownPlayerPosition = player.transform.position;
            }
        }
        else
        {
            if (currentState == OfficerState.Chasing)
            {
                currentState = OfficerState.Searching;
                hasReachedLKPInCurrentSearch = false; 
            }
        }
    }

    void ExecuteCurrentStateBehavior()
    {
        switch (currentState)
        {
            case OfficerState.Patrolling:
                agent.speed = patrolSpeed;
                if (Vector2.Distance(transform.position, target) <= agent.stoppingDistance + 0.5f || (!agent.hasPath && !agent.pathPending) || agent.pathStatus == NavMeshPathStatus.PathInvalid)
                {
                    if (searchNodes.instance != null && searchNodes.instance.searchNodesList.Count > 0)
                    {
                        target = searchNodes.instance.searchNodesList[Random.Range(0, searchNodes.instance.searchNodesList.Count)].position;
                    }
                    else
                    {
                        target = transform.position;
                    }
                    if (agent.isOnNavMesh) agent.SetDestination(target);
                }
                else if (agent.destination != (Vector3)target && agent.isOnNavMesh)
                {
                    agent.SetDestination(target);
                }
                break;

            case OfficerState.Chasing:
                agent.speed = chaseSpeed;
                if (player != null)
                {
                    lastKnownPlayerPosition = player.transform.position;
                    if (agent.isOnNavMesh) agent.SetDestination(lastKnownPlayerPosition);
                    target = lastKnownPlayerPosition;
                }
                else
                {
                    currentState = OfficerState.Searching;
                    hasReachedLKPInCurrentSearch = false; 
                    if (agent.isOnNavMesh) agent.SetDestination(lastKnownPlayerPosition);
                }
                break;

            case OfficerState.Searching:
                agent.speed = patrolSpeed;

                if (!hasReachedLKPInCurrentSearch) 
                {
                    if (Vector2.Distance(transform.position, lastKnownPlayerPosition) <= agent.stoppingDistance + 0.2f)
                    {
                        hasReachedLKPInCurrentSearch = true; 
                        if (searchNodes.instance != null && searchNodes.instance.searchNodesList.Count > 0)
                        {
                            Vector2 nextSearchTargetNode = searchNodes.instance.searchNodesList[Random.Range(0, searchNodes.instance.searchNodesList.Count)].position;
                            if (agent.isOnNavMesh) agent.SetDestination(nextSearchTargetNode);
                            target = nextSearchTargetNode; 
                        }
                        else
                        {
                            currentState = OfficerState.Patrolling; 
                        }
                    }
                    else 
                    {
                        if (agent.isOnNavMesh)
                        {
                            NavMeshHit hit;
                            if (NavMesh.SamplePosition(lastKnownPlayerPosition, out hit, lkpSampleRadius, NavMesh.AllAreas))
                            {
                                agent.SetDestination(hit.position);
                                target = hit.position; 
                            }
                            else
                            {
                                Debug.LogWarning($"Officer {gameObject.name} could not find valid NavMesh position near LKP {lastKnownPlayerPosition}. Switching to Patrolling.");
                                currentState = OfficerState.Patrolling; 
                            }
                        }
                        else { currentState = OfficerState.Patrolling; } 
                    }
                }
                else 
                {
                    if (Vector2.Distance(transform.position, target) <= agent.stoppingDistance + 0.5f || (!agent.hasPath && !agent.pathPending) || agent.pathStatus == NavMeshPathStatus.PathInvalid)
                    {
                        currentState = OfficerState.Patrolling; 
                    }
                    else if (agent.destination != (Vector3)target && agent.isOnNavMesh) 
                    {
                         agent.SetDestination(target);
                    }
                    else if(!agent.isOnNavMesh) { currentState = OfficerState.Patrolling; } 
                }
                break;
        }
    }

    void SetNewSweepInterval()
    {
        nextSweepTimer = Random.Range(minSweepInterval, maxSweepInterval);
    }

    void HandleSweepingState()
    {
        if (isCurrentlySweeping)
        {
            currentSweepActiveTime += Time.deltaTime;
            if (currentSweepActiveTime >= sweepDuration)
            {
                isCurrentlySweeping = false;
            }
        }
        else
        {
            nextSweepTimer -= Time.deltaTime;
            if (nextSweepTimer <= 0)
            {
                isCurrentlySweeping = true;
                currentSweepActiveTime = 0f;
                SetNewSweepInterval();
            }
        }
    }

    void RotateTowardsTargetWithSweep()
    {
        if (agent == null) return;
        Vector3 primaryLookDirection = Vector3.zero;

        if (currentState == OfficerState.Chasing && player != null)
        {
            primaryLookDirection = player.transform.position - transform.position;
        }
        else if (agent.hasPath && agent.velocity.sqrMagnitude > 0.05f)
        {
            primaryLookDirection = agent.velocity.normalized;
        }
        else if (agent.hasPath && Vector2.Distance(transform.position, agent.destination) > agent.stoppingDistance)
        {
             primaryLookDirection = agent.destination - transform.position;
        }
        else if (Vector2.Distance(transform.position, target) > agent.stoppingDistance + 0.1f) 
        {
            primaryLookDirection = (Vector3)target - transform.position;
        }

        primaryLookDirection.z = 0; 
        float baseAngle;

        if (primaryLookDirection.sqrMagnitude > 0.001f) 
        {
            baseAngle = Mathf.Atan2(primaryLookDirection.y, primaryLookDirection.x) * Mathf.Rad2Deg - 90f; 
        }
        else
        {
            baseAngle = transform.eulerAngles.z; 
        }

        float finalAngle = baseAngle;

        if (isCurrentlySweeping && currentState != OfficerState.Chasing)
        {
            float sweepOscillation = Mathf.Sin(Time.time * sweepOscillationSpeed);
            float sweepAngleOffset = sweepOscillation * maxSweepAngle;
            
            if (agent.hasPath && agent.velocity.sqrMagnitude > 0.05f) 
            {
                finalAngle += sweepAngleOffset * 0.6f;
            }
            else 
            {
                finalAngle += sweepAngleOffset;
            }
        }
        Quaternion targetRotation = Quaternion.Euler(new Vector3(0, 0, finalAngle));
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private void placeProximityMine()
    {
        if (isTimeFrozen) return;

        if (proximityMinePrefab != null && proximityMineCount > 0)
        {
            Vector2 minePosition = (Vector2)transform.position + Random.insideUnitCircle * 0.5f;
            Instantiate(proximityMinePrefab, minePosition, Quaternion.identity);
            proximityMineCount--;
        }

        if (proximityMineCount > 0 && proximityMinePrefab != null) 
        {
            float cooldown = Random.Range(proximityMineMinimumCooldownTime, proximityMineMaximumCooldownTime);
            Invoke("placeProximityMine", cooldown);
        }
    }

    void placeRadar()
    {
        if (isTimeFrozen) return;

        if (radarPrefab != null && GameObject.FindGameObjectWithTag("radar") == null && lawEnforcementManager.timeSinceRadarSpawned >= lawEnforcementManager.RadarCooldown && lawEnforcementManager.timePassed >= 60)
        {
            Vector2 radarPosition = (Vector2)transform.position + Random.insideUnitCircle * 1f;
            Instantiate(radarPrefab, radarPosition, Quaternion.identity);
        }

        if (radarPrefab != null) 
        {
            float cooldown = Random.Range(radarMinimumCooldownTime, radarMaximumCooldownTime);
            Invoke("placeRadar", cooldown);
        }
    }

    public void SetTarget(Vector2 newTarget)
    {
        if (isTimeFrozen) return;

        if (currentState == OfficerState.Patrolling)
        {
            this.target = newTarget;
            if (agent != null && agent.isOnNavMesh)
            {
                agent.SetDestination(this.target);
            }
            else if (agent == null)
            {
                Debug.LogError("SetTarget called, but NavMeshAgent is null on " + gameObject.name);
            }
            else if (!agent.isOnNavMesh)
            {
                Debug.LogWarning("SetTarget called, but agent is not on NavMesh on " + gameObject.name);
            }
        }
    }

    void UpdateFlashlightStatus()
    {
        if (officerFlashlight == null) return;

        if (isTimeFrozen)
        {
            officerFlashlight.enabled = false;
            return;
        }

        if (player == null)
        {
            officerFlashlight.enabled = false;
            return;
        }

        if (currentState == OfficerState.Chasing)
        {
            officerFlashlight.enabled = true;
            officerFlashlight.color = chaseFlashlightColor;
            officerFlashlight.intensity = originalFlashlightIntensity * chaseFlashlightIntensityMultiplier;
        }
        else
        {
            officerFlashlight.color = originalFlashlightColor;
            officerFlashlight.intensity = originalFlashlightIntensity;

            float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
            bool shouldBeEnabled = distanceToPlayer <= 30f || currentState == OfficerState.Searching;
            officerFlashlight.enabled = shouldBeEnabled;
        }
    }
    
    private void OnTriggerStay2D(Collider2D other)
    {
        if (isTimeFrozen || player == null || lawEnforcementManager == null || !other.CompareTag("Player"))
        {
            return;
        }

        Vector2 currentOfficerPos = (Vector2)transform.position;
        Vector2 rayOrigin = currentOfficerPos + raycastOriginOffset;
        Vector2 playerCenterPos = (Vector2)other.bounds.center;
        Vector2 directionToPlayer = (playerCenterPos - rayOrigin).normalized;
        float distanceToPlayerForRay = Vector2.Distance(rayOrigin, playerCenterPos);

        if (distanceToPlayerForRay <= 0.05f)
        {
        }

        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, directionToPlayer, distanceToPlayerForRay, visionObstructingLayers);

        if (hit.collider == null)
        {
            Debug.DrawRay(rayOrigin, directionToPlayer * distanceToPlayerForRay, Color.green); 

            playerController pc = other.GetComponent<playerController>();
            if (pc != null)
            {
                if (pc.foliageUnder.Count == 0)
                {
                    lawEnforcementManager.ChangeDetectionPercentage(1 * Time.deltaTime);
                }
                else
                {
                    lawEnforcementManager.ChangeDetectionPercentage(0.25f * Time.deltaTime);
                }
            }
            if (lawEnforcementManager.detectionPercentage >= 1f)
            {
                canSeePlayerCurrentFrame = true;
            }
        }
        else
        {
            Debug.DrawRay(rayOrigin, directionToPlayer * hit.distance, Color.red); 
            canSeePlayerCurrentFrame = false; 
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            canSeePlayerCurrentFrame = false; 
        }
    }

    public static void ResetTime()
    {
        Time.timeScale = 1f;
        isTimeFrozen = false;
    }

    void OnDestroy()
    {
        CancelInvoke("placeProximityMine");
        CancelInvoke("placeRadar");
    }

     void OnEnable()
    {
        if (agent != null && agent.isOnNavMesh)
        {
            if (isTimeFrozen) {
                agent.isStopped = true;
            } else {
                agent.isStopped = false;
            }
        }
         if (officerFlashlight != null && !isTimeFrozen)
        {
           UpdateFlashlightStatus(); // Re-evaluate flashlight state on enable
        }
    }
}
