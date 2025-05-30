using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ShadowCuller : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("The distance at which shadows will be disabled.")]
    public float cullDistance = 15f;

    [Header("References (Optional - will attempt to find if not set)")]
    [Tooltip("Drag the player GameObject here. If not set, it will be found by tag 'Player'.")]
    public Transform playerTransform;

    private ShadowCaster2D shadowCaster;
    private CompositeShadowCaster2D compositeShadowCaster;

    private bool shadowsEnabled = true;

    void Awake()
    {
        shadowCaster = GetComponent<ShadowCaster2D>();
        compositeShadowCaster = GetComponent<CompositeShadowCaster2D>();

        if (playerTransform == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                playerTransform = playerObject.transform;
            }
            else
            {
                Debug.LogError("ShadowCuller: Player not found! Make sure your player GameObject has the 'Player' tag.", this);
                enabled = false;
                return;
            }
        }

        if (playerTransform != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer > cullDistance)
            {
                SetShadowsEnabled(false);
            }
            else
            {
                SetShadowsEnabled(true);
            }
        }
    }

    void Update()
    {
        if (playerTransform == null)
        {
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer > cullDistance)
        {
            if (shadowsEnabled)
            {
                SetShadowsEnabled(false);
            }
        }
        else
        {
            if (!shadowsEnabled)
            {
                SetShadowsEnabled(true);
            }
        }
    }

    void SetShadowsEnabled(bool enable)
    {
        if (shadowCaster != null && shadowCaster.enabled != enable)
        {
            shadowCaster.enabled = enable;
        }

        if (compositeShadowCaster != null && compositeShadowCaster.enabled != enable)
        {
            compositeShadowCaster.enabled = enable;
        }
        shadowsEnabled = enable;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, cullDistance);
    }
}