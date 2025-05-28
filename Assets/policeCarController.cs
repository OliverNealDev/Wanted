using UnityEngine;
using UnityEngine.Rendering.Universal;

public class policeCarController : MonoBehaviour
{
    private enum CarState { Approaching, PullingOver, Parked }
    private CarState currentState = CarState.Approaching;

    private Vector2 initialTargetPosition;
    private Vector2 pullOverTargetPosition;
    private float initialY;
    private float pullOverYOffset = 6f;

    [SerializeField] private GameObject policeOfficerPrefab;

    [SerializeField] private float maxSpeed = 10f;
    [SerializeField] private float smoothTime = 0.3f;

    private bool disbandedOfficers = false;
    private Vector2 currentVelocity = Vector2.zero;

    [SerializeField] private lawEnforcementManager lawEnforcementManager;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip siren1Clip;
    [SerializeField] private AudioClip siren2Clip;
    [SerializeField] private AudioClip parkingClip;

    private float timeAlive = 0f;
    private const float selfDestructTimeout = 30f;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        lawEnforcementManager = FindObjectOfType<lawEnforcementManager>();

        bool randomEntrance = Random.Range(0, 2) == 0;
        float randomTargetX = Random.Range(-56f, 56f);

        if (randomEntrance)
        {
            transform.position = new Vector3(-110f, 4, 0f);
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            initialY = 4f;
            initialTargetPosition = new Vector2(randomTargetX, initialY);
            pullOverTargetPosition = new Vector2(randomTargetX, initialY + pullOverYOffset);
        }
        else
        {
            transform.position = new Vector3(125f, 1, 0f);
            transform.rotation = Quaternion.Euler(0f, 0f, 180f);
            initialY = 1f;
            initialTargetPosition = new Vector2(randomTargetX, initialY);
            pullOverTargetPosition = new Vector2(randomTargetX, initialY - pullOverYOffset);
        }

        if (Random.value > 0.5f)
        {
            audioSource.clip = siren1Clip;
        }
        else
        {
            audioSource.clip = siren2Clip;
        }

        audioSource.loop = true;
        audioSource.Play();
    }

    void Update()
    {
        if (currentState != CarState.Parked)
        {
            timeAlive += Time.deltaTime;
            if (timeAlive > selfDestructTimeout)
            {
                lawEnforcementManager.SpawnPoliceCar();
                Destroy(gameObject);
                return;
            }
        }

        switch (currentState)
        {
            case CarState.Approaching:
                HandleApproachingState();
                break;
            case CarState.PullingOver:
                HandlePullingOverState();
                break;
            case CarState.Parked:
                HandleParkedState();
                break;
        }
    }

    void HandleApproachingState()
    {
        transform.position = Vector2.SmoothDamp(
            (Vector2)transform.position,
            initialTargetPosition,
            ref currentVelocity,
            smoothTime,
            maxSpeed,
            Time.deltaTime
        );

        UpdateRotationBasedOnXVelocity();

        if (Vector2.Distance((Vector2)transform.position, initialTargetPosition) < 0.5f)
        {
            currentState = CarState.PullingOver;
        }
    }

    void HandlePullingOverState()
    {
        transform.position = Vector2.SmoothDamp(
            (Vector2)transform.position,
            pullOverTargetPosition,
            ref currentVelocity,
            smoothTime,
            maxSpeed / 2f,
            Time.deltaTime
        );

        Vector2 directionToTarget = (pullOverTargetPosition - (Vector2)transform.position).normalized;
        if (directionToTarget.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        if (Vector2.Distance((Vector2)transform.position, pullOverTargetPosition) < 0.1f)
        {
            currentVelocity = Vector2.zero;
            transform.position = pullOverTargetPosition;
            //transform.rotation = Quaternion.Euler(0f, 0f, initialY > 0 ? 90f : -90f);

            currentState = CarState.Parked;
            if (autoBake.instance != null)
            {
                autoBake.instance.Bake();
            }
        }
    }

    void HandleParkedState()
    {
        if (!disbandedOfficers)
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
            audioSource.clip = parkingClip;
            audioSource.loop = false;
            audioSource.Play();

            if (transform.childCount > 2)
            {
                Transform thirdChild = transform.GetChild(2);
                if (thirdChild != null)
                {
                    Light2D lightComponent = thirdChild.GetComponent<Light2D>();
                    if (lightComponent != null)
                    {
                        lightComponent.enabled = false;
                    }
                }
            }

            disbandedOfficers = true;

            float sideSpawnOffset = 1.5f;
            float longitudinalSpawnOffset = 0.5f;

            lawEnforcementManager.SpawnPoliceOfficer(transform.position + new Vector3(sideSpawnOffset, longitudinalSpawnOffset, 0));
            lawEnforcementManager.SpawnPoliceOfficer(transform.position + new Vector3(-sideSpawnOffset, longitudinalSpawnOffset, 0));
            lawEnforcementManager.SpawnPoliceOfficer(transform.position + new Vector3(sideSpawnOffset, -longitudinalSpawnOffset, 0));
            lawEnforcementManager.SpawnPoliceOfficer(transform.position + new Vector3(-sideSpawnOffset, -longitudinalSpawnOffset, 0));
        }
    }

    void UpdateRotationBasedOnXVelocity()
    {
        if (Mathf.Abs(currentVelocity.x) > 0.01f)
        {
            if (currentVelocity.x > 0.01f)
            {
                transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            }
            else if (currentVelocity.x < -0.01f)
            {
                transform.rotation = Quaternion.Euler(0f, 0f, 180f);
            }
        }
    }
}