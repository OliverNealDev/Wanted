using UnityEngine;

public class cameraController : MonoBehaviour
{
    public GameObject target;
    [SerializeField] private float smoothSpeed = 0.125f;
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10f);
    private Vector3 _cameraVelocity = Vector3.zero;

    void Start()
    {
        if (target == null)
        {
            Debug.LogError("CameraController: Target not set. Please assign a target (player) in the Inspector.");
        }
    }

    void LateUpdate()
    {
        if (target == null)
        {
            return; 
        }

        //Rigidbody2D targetRB = target.GetComponent<Rigidbody2D>();
        
        Vector3 desiredPosition = new Vector3(target.transform.position.x + offset.x, target.transform.position.y + offset.y, offset.z);
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;
    }
}