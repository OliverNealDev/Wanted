using UnityEngine;

public class policeCarController : MonoBehaviour
{
    private bool randomEntrance;
    private Vector2 targetPosition;

    [SerializeField] private GameObject policeOfficerPrefab;
    
    [SerializeField] private float maxSpeed = 10f;
    [SerializeField] private float smoothTime = 0.3f;
    // [SerializeField] private float breakingForce = 5f; // Kept as it was in the source, though noted as unused

    private bool isDriving = true;
    private bool disbandedOfficers = false;
    private Vector2 currentVelocity = Vector2.zero;

    void Start()
    {
        targetPosition = new Vector2(Random.Range(-30f, 30f), transform.position.y);
        randomEntrance = Random.Range(0, 2) == 0;

        if (randomEntrance)
        {
            transform.position = new Vector3(-75f, 0.75f, 0f);
            transform.rotation = Quaternion.Euler(0f, 0f, 180f);
        }
        else
        {
            transform.position = new Vector3(75f, -0.75f, 0f);
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }
    }

    void Update()
    {
        if (isDriving)
        {
            if (Vector2.Distance((Vector2)transform.position, targetPosition) < 0.1f && isDriving)
            {
                isDriving = false;
                autoBake.instance.Bake();
            }
            else
            {
                transform.position = Vector2.SmoothDamp(
                    (Vector2)transform.position,
                    targetPosition,
                    ref currentVelocity,
                    smoothTime,
                    maxSpeed,
                    Time.deltaTime
                );

                if (currentVelocity.x > 0.01f)
                {
                    transform.rotation = Quaternion.Euler(0f, 0f, 180f);
                }
                else if (currentVelocity.x < -0.01f)
                {
                    transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                }
            }
        }
        else if (!disbandedOfficers)
        {
            disbandedOfficers = true;
            for (int i = 0; i < 4; i++)
            {
                Instantiate(policeOfficerPrefab, transform.position + new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0), Quaternion.identity);
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        isDriving = false;
        autoBake.instance.Bake();
    }
}