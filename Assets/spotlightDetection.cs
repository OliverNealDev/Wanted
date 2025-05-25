using UnityEngine;

public class spotlightDetection : MonoBehaviour
{
    private lawEnforcementManager lawEnforcementManager;

    void Start()
    {
        lawEnforcementManager = FindObjectOfType<lawEnforcementManager>();
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            lawEnforcementManager.ChangeDetectionPercentage(4 * Time.deltaTime);
        }
    }
    
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            lawEnforcementManager.ChangeDetectionPercentage(4 * Time.deltaTime);
        }
    }
}
