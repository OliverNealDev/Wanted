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
            if (other.GetComponent<playerController>().foliageUnder.Count == 0)
            {
                lawEnforcementManager.ChangeDetectionPercentage(1 * Time.deltaTime);
            }
        }
    }
    
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.GetComponent<playerController>().foliageUnder.Count == 0)
            {
                lawEnforcementManager.ChangeDetectionPercentage(1 * Time.deltaTime);
            }
        }
    }
}
