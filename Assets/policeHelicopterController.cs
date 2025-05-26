using UnityEngine;
using Random = UnityEngine.Random;

public class policeHelicopterController : MonoBehaviour
{
    private Vector3 centerPoint;
    private float radius = 3f;
    private Vector3 targetPosition;
    public float speed = 10f;
    private Vector3 startPosition;

    public Transform spotlightChild;
    public float spotlightSmoothTime = 0.3f; 
    public float spotlightTargetPosChangeInterval = 1.5f;
    public float spotlightMoveRadius = 0.5f;

    private Vector3 currentSpotlightTargetLocalPosition;
    private float timeToNextSpotlightTargetPosChange;
    private float initialSpotlightLocalZ;
    private Vector3 spotlightVelocity = Vector3.zero;
    
    private lawEnforcementManager lawEnforcementManager;

    void Start()
    {
        //Time.timeScale = 10f;
        lawEnforcementManager = FindObjectOfType<lawEnforcementManager>();
        centerPoint = new Vector3(10, 3, 0);
        
        Vector2 randomNormalizedDirection2D = Random.insideUnitCircle.normalized;
        Vector3 scaledDirection = new Vector3(randomNormalizedDirection2D.x * radius, randomNormalizedDirection2D.y * radius, 0f);

        startPosition = centerPoint + scaledDirection * 50f;
        transform.position = startPosition;

        targetPosition = centerPoint - scaledDirection * 100f;

        if (spotlightChild != null)
        {
            initialSpotlightLocalZ = spotlightChild.localPosition.z;
            SetNewSpotlightTargetPosition();
        }
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
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
        //transform.LookAt(targetPosition);
        //transform.position = new Vector3(transform.position.x, transform.position.y, 0);
        if (Vector2.Distance(transform.position, targetPosition) < 1f)
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
}