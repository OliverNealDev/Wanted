using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Slider = UnityEngine.UI.Slider;

public class lawEnforcementManager : MonoBehaviour
{
    [SerializeField] private GameObject policeCarPrefab;
    [SerializeField] private Vector2 policeCarSpawnPoint;
    
    [SerializeField] private GameObject policeHelicopterPrefab;
    [SerializeField] private Vector2 policeHelicopterSpawnPoint;
    
    [SerializeField] private GameObject policeOfficerPrefab;
    
    private List<GameObject> policeOfficers = new List<GameObject>();

    public float detectionPercentage { get; private set; } = 0f;
    [SerializeField] private Slider detectionSlider;
    private float timeSinceLastDetectionIncrease = 0f;
    [SerializeField] private float detectionDecreaseRate = 0.1f;
    private int policeCarsSpawned = 0;
    public int radarsSpawned = 0;
    public float timePassed { get; private set; } = 0f;
    public float timeSinceRadarSpawned = 0f;
    public float RadarCooldown = 0f;

    private GameObject player;
    
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        InvokeRepeating("SpawnPoliceCar", 1f, 30.44f);
        Invoke("SpawnPoliceCar", 3f);
        InvokeRepeating("SpawnPoliceHelicopter", 30f, 25.4f);
        
        detectionSlider.value = detectionPercentage;
    }
    
    void Update()
    {
        timePassed += Time.deltaTime;
        timeSinceRadarSpawned += Time.deltaTime;
        
        timeSinceLastDetectionIncrease += Time.deltaTime;
        if (timeSinceLastDetectionIncrease > 3 && detectionPercentage > 0f)
        {
            detectionPercentage -= detectionDecreaseRate * Time.deltaTime;
            if (detectionPercentage < 0f)
            {
                detectionPercentage = 0;
            }
            
            detectionSlider.value = detectionPercentage;
        }
    }
    
    public void ChangeDetectionPercentage(float amount)
    {
        timeSinceLastDetectionIncrease = 0;
        detectionPercentage += amount;
        
        if (detectionPercentage >= 1f)
        {
            detectionPercentage = 1f;
            detectionSlider.value = detectionPercentage;
            AlertSpottedTransform(new Vector2(player.transform.position.x, player.transform.position.y));
        }
        else
        {
            detectionSlider.value = detectionPercentage;
        }
    }

    public void AlertSpottedTransform(Vector2 targetTransform)
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

            float distanceToTarget = Vector3.Distance(policeOfficer.transform.position, targetTransform);
            officersWithDistances.Add((policeOfficer, distanceToTarget));
        }

        var sortedOfficers = officersWithDistances.OrderBy(o => o.distance).ToList();

        int officersToAlertCount = Mathf.CeilToInt(policeOfficers.Count * 0.1f);
        officersToAlertCount = Mathf.Min(officersToAlertCount, sortedOfficers.Count);

        var closestOfficers = sortedOfficers.Take(officersToAlertCount);

        Debug.Log($"Alerting {closestOfficers.Count()} closest officers.");
        foreach (var officerData in closestOfficers)
        {
            policeOfficerController controller = officerData.officer.GetComponent<policeOfficerController>();
            if (controller != null)
            {
                controller.SetTarget(targetTransform);
            }
            else
            {
                Debug.LogWarning($"Police officer {officerData.officer.name} does not have a policeOfficerController component.");
            }
        }
    }
    
    public void SpawnPoliceCar()
    {
        policeCarsSpawned++;
        Instantiate(policeCarPrefab, policeCarSpawnPoint, Quaternion.identity);
        if (policeCarsSpawned > 5)
        {
            CancelInvoke(nameof(SpawnPoliceCar));
        }
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
