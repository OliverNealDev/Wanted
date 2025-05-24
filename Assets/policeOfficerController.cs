using NavMeshPlus.Components;
using UnityEngine;
using UnityEngine.AI;

public class policeOfficerController : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform target;

    [Header("Movement & Rotation")]
    [SerializeField] private float rotationSpeed = 5f;

    [Header("Flashlight Sweep Behavior")]
    [SerializeField] private float minSweepInterval = 3f;
    [SerializeField] private float maxSweepInterval = 5f;
    [SerializeField] private float sweepDuration = 2.5f;
    [SerializeField] private float maxSweepAngle = 20f;
    [SerializeField] private float sweepOscillationSpeed = 1.5f;

    NavMeshAgent agent;

    private float nextSweepTimer;
    private bool isCurrentlySweeping = false;
    private float currentSweepActiveTime;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        target = searchNodes.instance.searchNodesList[Random.Range(0, searchNodes.instance.searchNodesList.Count)];

        SetNewSweepInterval();
    }

    void Update()
    {
        if (target == null) return;

        if (Vector2.Distance(transform.position, target.position) > 1f)
        {
            agent.SetDestination(target.position);
        }
        else
        {
            target = searchNodes.instance.searchNodesList[Random.Range(0, searchNodes.instance.searchNodesList.Count)];
            agent.SetDestination(target.position);
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
        Vector3 directionToTarget = target.position - transform.position;
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
}