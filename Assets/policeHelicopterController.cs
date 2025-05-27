using UnityEngine;

public class policeHelicopterController : MonoBehaviour
{
    private float radius = 3f;
    private Vector3 targetPosition; 
    private Vector3 startPosition;
    private Vector3 flyThroughPoint;

    public float speed = 10f;
    public Transform spotlightChild;
    public float spotlightSmoothTime = 0.3f;
    public float spotlightTargetPosChangeInterval = 1.5f;
    public float spotlightMoveRadius = 0.5f;

    private Vector3 currentSpotlightTargetLocalPosition;
    private float timeToNextSpotlightTargetPosChange;
    private float initialSpotlightLocalZ;
    private Vector3 spotlightVelocity = Vector3.zero;

    void Start()
    {
        Vector3 centerPointForStart = new Vector3(10, 3, 0);
        Vector2 randomNormalizedDirection2D = Random.insideUnitCircle.normalized;
        Vector3 scaledDirection = new Vector3(randomNormalizedDirection2D.x * radius, randomNormalizedDirection2D.y * radius, 0f);
        startPosition = centerPointForStart + scaledDirection * 50f;

        transform.position = startPosition;
        transform.position = new Vector3(transform.position.x, transform.position.y, 0f);

        float randomFlyThroughX = Random.Range(-56f, 56f);
        float randomFlyThroughY = Random.Range(-40f, 45f);
        flyThroughPoint = new Vector3(randomFlyThroughX, randomFlyThroughY, 0f);

        Vector3 vectorToFlyThrough = flyThroughPoint - startPosition;
        targetPosition = flyThroughPoint + vectorToFlyThrough;

        RotateTowardsFinalTarget();

        if (spotlightChild != null)
        {
            initialSpotlightLocalZ = spotlightChild.localPosition.z;
            SetNewSpotlightTargetPosition();
        }
        
        Invoke("DespawnTimeout", 60f);
    }

    void SetNewSpotlightTargetPosition()
    {
        Vector2 randomOffsetXY = Random.insideUnitCircle * spotlightMoveRadius;
        currentSpotlightTargetLocalPosition = new Vector3(randomOffsetXY.x, randomOffsetXY.y, initialSpotlightLocalZ);

        float intervalVariation = spotlightTargetPosChangeInterval * 0.2f;
        timeToNextSpotlightTargetPosChange = spotlightTargetPosChangeInterval + Random.Range(-intervalVariation, intervalVariation);
    }

    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        RotateTowardsFinalTarget();

        transform.position = new Vector3(transform.position.x, transform.position.y, 0f);

        if (Vector2.Distance(new Vector2(transform.position.x, transform.position.y), new Vector2(targetPosition.x, targetPosition.y)) < 1f)
        {
            Destroy(gameObject);
            return;
        }

        if (spotlightChild != null)
        {
            timeToNextSpotlightTargetPosChange -= Time.deltaTime;
            if (timeToNextSpotlightTargetPosChange <= 0f)
            {
                SetNewSpotlightTargetPosition();
            }
            spotlightChild.localPosition = Vector3.SmoothDamp(spotlightChild.localPosition, currentSpotlightTargetLocalPosition, ref spotlightVelocity, spotlightSmoothTime, Mathf.Infinity, Time.deltaTime);
        }
    }

    void DespawnTimeout()
    {
        Destroy(gameObject);
    }

    void RotateTowardsFinalTarget()
    {
        if (targetPosition != transform.position)
        {
            Vector3 directionToTarget = targetPosition - transform.position;
            float angle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle + 90f);
        }
    }
}