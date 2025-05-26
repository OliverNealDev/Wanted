using System;
using NavMeshPlus.Components;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.Universal;
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

    [Header("Equipment Settings")]
    [SerializeField] private GameObject proximityMinePrefab;
    [SerializeField] private int proximityMineCount = 1;
    [SerializeField] private float minimumCooldownTime = 30f;
    [SerializeField] private float maximumCooldownTime = 90f;

    private Light2D officerFlashlight;
    NavMeshAgent agent;

    private float nextSweepTimer;
    private bool isCurrentlySweeping = false;
    private float currentSweepActiveTime;

    private lawEnforcementManager lawEnforcementManager;
    private GameObject player;

    void Start()
    {
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
        }
        else
        {
            Debug.LogError("Police officer '" + gameObject.name + "' has no children; cannot find Light2D.");
        }

        float randomSpeedPercentageModifier = Random.Range(0.8f, 1.2f);
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
                Debug.LogWarning("Agent is not on NavMesh at Start. Cannot set initial destination or speed.");
                if (officerFlashlight != null) officerFlashlight.enabled = false;
            }
        }
        else
        {
            target = transform.position;
            Debug.LogWarning("SearchNodes instance not found or nodeList is empty. Officer may not have a patrol target and will remain stationary.");
            if (agent.isOnNavMesh)
            {
                agent.speed = patrolSpeed;
            }
        }

        SetNewSweepInterval();
        if (proximityMineCount > 0 && proximityMinePrefab != null)
        {
            Invoke("placeProximityMine", Random.Range(minimumCooldownTime, maximumCooldownTime));
        }
        else if (proximityMinePrefab == null && proximityMineCount > 0)
        {
            Debug.LogWarning("ProximityMinePrefab is not assigned, but proximityMineCount is greater than 0. Officer cannot place mines.");
        }

        if (officerFlashlight != null)
        {
            officerFlashlight.enabled = false;
        }
    }

    void Update()
    {
        if (player == null || lawEnforcementManager == null || agent == null || !agent.isOnNavMesh)
        {
            if (officerFlashlight != null)
            {
                officerFlashlight.enabled = false;
            }
            return;
        }

        bool fullDetection = Mathf.Approximately(lawEnforcementManager.detectionPercentage, 1f);

        if (fullDetection)
        {
            target = new Vector2(player.transform.position.x, player.transform.position.y);
            agent.SetDestination(target);
            agent.speed = chaseSpeed;
        }
        else
        {
            agent.speed = patrolSpeed;

            if (Vector2.Distance(transform.position, target) <= agent.stoppingDistance + 0.5f || !agent.hasPath || agent.pathStatus == NavMeshPathStatus.PathInvalid)
            {
                if (searchNodes.instance != null && searchNodes.instance.searchNodesList.Count > 0)
                {
                    target = searchNodes.instance.searchNodesList[Random.Range(0, searchNodes.instance.searchNodesList.Count)].position;
                }
                else
                {
                    target = transform.position;
                }
                agent.SetDestination(target);
            }
            else if (agent.destination != (Vector3)target)
            {
                agent.SetDestination(target);
            }
        }

        HandleSweepingState();
        RotateTowardsTargetWithSweep();

        if (officerFlashlight != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
            if (distanceToPlayer > 30f)
            {
                officerFlashlight.enabled = false;
            }
            else
            {
                officerFlashlight.enabled = true;
            }
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
        bool isActivelyChasing = lawEnforcementManager != null && Mathf.Approximately(lawEnforcementManager.detectionPercentage, 1f) && player != null;

        if (isActivelyChasing)
        {
            primaryLookDirection = player.transform.position - transform.position;
        }
        else if (agent.hasPath && agent.velocity.sqrMagnitude > 0.05f)
        {
            primaryLookDirection = agent.velocity.normalized;
        }
        else if (!isActivelyChasing && Vector2.Distance(transform.position, target) > agent.stoppingDistance)
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

        if (isCurrentlySweeping)
        {
            float sweepOscillation = Mathf.Sin(Time.time * sweepOscillationSpeed);
            float sweepAngleOffset = sweepOscillation * maxSweepAngle;

            if (isActivelyChasing)
            {
                finalAngle += sweepAngleOffset * 0.3f;
            }
            else if (agent.hasPath && agent.velocity.sqrMagnitude > 0.05f)
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
        if (proximityMinePrefab != null && proximityMineCount > 0)
        {
            Vector2 minePosition = (Vector2)transform.position + Random.insideUnitCircle * 0.5f;
            Instantiate(proximityMinePrefab, minePosition, Quaternion.identity);
            proximityMineCount--;
            Debug.Log($"Proximity mine placed. {proximityMineCount} remaining.");
        }

        if (proximityMineCount > 0 && proximityMinePrefab != null)
        {
            float cooldown = Random.Range(minimumCooldownTime, maximumCooldownTime);
            Invoke("placeProximityMine", cooldown);
        }
    }

    public void SetTarget(Vector2 newTarget)
    {
        if (lawEnforcementManager != null && Mathf.Approximately(lawEnforcementManager.detectionPercentage, 1f))
        {
            return;
        }

        target = newTarget;
        if (agent != null && agent.isOnNavMesh)
        {
            agent.SetDestination(target);
            agent.speed = patrolSpeed;
        }
        else if (agent == null)
        {
            Debug.LogError("SetTarget called, but NavMeshAgent is null.");
        }
        else if (!agent.isOnNavMesh)
        {
            Debug.LogWarning("SetTarget called, but agent is not on NavMesh.");
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player") && lawEnforcementManager != null)
        {
            lawEnforcementManager.ChangeDetectionPercentage(1f * Time.deltaTime);
        }
    }
}