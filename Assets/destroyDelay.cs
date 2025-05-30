using UnityEngine;

public class destroyDelay : MonoBehaviour
{
    [SerializeField] private float delay = 2f;

    void Start()
    {
        Destroy(gameObject, delay);
    }
}
