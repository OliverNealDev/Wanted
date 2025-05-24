using System.Collections.Generic;
using UnityEngine;

public class lawEnforcementManager : MonoBehaviour
{
    [SerializeField] private GameObject policeCarPrefab;
    [SerializeField] private Vector2 policeCarSpawnPoint;
    
    [SerializeField] private GameObject policeHelicopterPrefab;
    [SerializeField] private Vector2 policeHelicopterSpawnPoint;
    
    [SerializeField] private GameObject policeOfficerPrefab;
    
    private List<GameObject> policeOfficers = new List<GameObject>();
    
    void Start()
    {
        InvokeRepeating("SpawnPoliceCar", 1f, 20.4f);
        InvokeRepeating("SpawnPoliceHelicopter", 30f, 45.4f);
    }
    
    void Update()
    {
        
    }

    public void AlertSpottedTransform(Transform transform)
    {
        foreach (GameObject policeOfficer in policeOfficers)
        {
            policeOfficer.GetComponent<policeOfficerController>().SetTarget(transform.position);
        }
    }
    
    void SpawnPoliceCar()
    {
        Instantiate(policeCarPrefab, policeCarSpawnPoint, Quaternion.identity);
    }
    
    public void SpawnPoliceOfficer(Vector3 position)
    {
        GameObject newPoliceOfficer = Instantiate(policeOfficerPrefab, position, Quaternion.identity);
        policeOfficers.Add(newPoliceOfficer);
    }
    
    void SpawnPoliceHelicopter()
    {
        Instantiate(policeHelicopterPrefab, policeHelicopterSpawnPoint, Quaternion.identity);
    }
}
