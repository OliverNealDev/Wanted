using System.Collections.Generic;
using System.Linq;
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

    public void AlertSpottedTransform(Transform targetTransform)
    {
        if (policeOfficers == null || policeOfficers.Count == 0)
        {
            Debug.LogWarning("No police officers available to alert.");
            return;
        }

        var officersWithDistances = new List<(GameObject officer, float distance)>();
        foreach (GameObject policeOfficer in policeOfficers)
        {
            if (policeOfficer == null) continue;

            float distanceToTarget = Vector3.Distance(policeOfficer.transform.position, targetTransform.position);
            officersWithDistances.Add((policeOfficer, distanceToTarget));
        }

        var sortedOfficers = officersWithDistances.OrderBy(o => o.distance).ToList();

        int officersToAlertCount = Mathf.CeilToInt(policeOfficers.Count * 0.25f);
        officersToAlertCount = Mathf.Min(officersToAlertCount, sortedOfficers.Count);

        var closestOfficers = sortedOfficers.Take(officersToAlertCount);

        Debug.Log($"Alerting {closestOfficers.Count()} closest officers.");
        foreach (var officerData in closestOfficers)
        {
            policeOfficerController controller = officerData.officer.GetComponent<policeOfficerController>();
            if (controller != null)
            {
                controller.SetTarget(targetTransform.position);
            }
            else
            {
                Debug.LogWarning($"Police officer {officerData.officer.name} does not have a policeOfficerController component.");
            }
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
