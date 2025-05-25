using System;
using NavMeshPlus.Components;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class policeOfficerController : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Vector2 target;

    [Header("Movement & Rotation")]
    [SerializeField] private float rotationSpeed = 5f;

    [Header("Flashlight Sweep Behavior")]
    [SerializeField] private float minSweepInterval = 3f;
    [SerializeField] private float maxSweepInterval = 5f;
    [SerializeField] private float sweepDuration = 2.5f;
    [SerializeField] private float maxSweepAngle = 20f;
    [SerializeField] private float sweepOscillationSpeed = 1.5f;

    [Header("Equipment Settings")] [SerializeField]
    private GameObject proximityMinePrefab;
    [SerializeField] private int proximityMineCount = 1;
    [SerializeField] private float minimumCooldownTime = 30f;
    [SerializeField] private float maximumCooldownTime = 90f;

    NavMeshAgent agent;

    private float nextSweepTimer;
    private bool isCurrentlySweeping = false;
    private float currentSweepActiveTime;
    
    private lawEnforcementManager lawEnforcementManager;

    private SpriteRenderer SR;

    private GameObject player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        lawEnforcementManager = FindObjectOfType<lawEnforcementManager>();
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        target = searchNodes.instance.searchNodesList[Random.Range(0, searchNodes.instance.searchNodesList.Count)].position;

        SR = GetComponent<SpriteRenderer>();
        //SR.enabled = false;
        
        SetNewSweepInterval();
        Invoke("placeProximityMine", Random.Range(minimumCooldownTime * 1f, maximumCooldownTime * 1f));
    }

    void Update()
    {
        if (target == null) return;

        if (Mathf.Approximately(lawEnforcementManager.detectionPercentage, 1) && Vector2.Distance(player.transform.position, transform.position) <= 20f)
        {
            target = new Vector2(player.transform.position.x, player.transform.position.y);
        }
        
        if (Vector2.Distance(transform.position, target) > 3f && lawEnforcementManager.detectionPercentage < 1)
        {
            agent.SetDestination(target);
        }
        else if (Mathf.Approximately(lawEnforcementManager.detectionPercentage, 1))
        {
            agent.SetDestination(target);
        }
        else
        {
            target = searchNodes.instance.searchNodesList[Random.Range(0, searchNodes.instance.searchNodesList.Count)].position;
            agent.SetDestination(target);
        }

        HandleSweepingState();
        RotateTowardsTargetWithSweep();
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
        Vector3 directionToTarget = target - new Vector2(transform.position.x, transform.position.z);
        directionToTarget.z = 0;

        float finalAngle;

        if (directionToTarget != Vector3.zero)
        {
            float baseAngleToTarget = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg - 90f;
            finalAngle = baseAngleToTarget;

            if (isCurrentlySweeping)
            {
                float sweepRadianOffset = Mathf.Sin(currentSweepActiveTime * sweepOscillationSpeed * (2 * Mathf.PI / sweepDuration) );
                float sweepAngleOffset = sweepRadianOffset * maxSweepAngle;
                finalAngle += sweepAngleOffset;
            }
        }
        else
        {
            finalAngle = transform.eulerAngles.z;
        }

        Quaternion targetRotation = Quaternion.Euler(new Vector3(0, 0, finalAngle));
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }
    
    private void placeProximityMine()
    {
        if (proximityMinePrefab != null && proximityMineCount > 0)
        {
            Vector2 minePosition = transform.position + new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0);
            Instantiate(proximityMinePrefab, minePosition, Quaternion.identity);
            proximityMineCount--;
        }
        if (proximityMineCount > 0)
        {
            Invoke("placeProximityMine", Random.Range(minimumCooldownTime * 1f, maximumCooldownTime * 1f));
        }
    }
    
    public void SetTarget(Vector2 newTarget)
    {
        target = newTarget;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            lawEnforcementManager.ChangeDetectionPercentage(1f * Time.deltaTime);
        }
    }
    
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            lawEnforcementManager.ChangeDetectionPercentage(1f * Time.deltaTime);
        }
    }
}