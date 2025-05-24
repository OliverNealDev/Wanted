using UnityEngine;

public class lawEnforcementManager : MonoBehaviour
{
    [SerializeField] private GameObject policeCarPrefab;
    [SerializeField] private Vector2 policeCarSpawnPoint;
    
    [SerializeField] private GameObject policeOfficerPrefab;
    
    void Start()
    {
        InvokeRepeating("SpawnPoliceCar", 1f, 20f);
    }
    
    void Update()
    {
        
    }
    
    void SpawnPoliceCar()
    {
        Instantiate(policeCarPrefab, policeCarSpawnPoint, Quaternion.identity);
    }
    
    void SpawnPoliceOfficer(Vector3 position)
    {
        Instantiate(policeOfficerPrefab, position, Quaternion.identity);
    }
}
