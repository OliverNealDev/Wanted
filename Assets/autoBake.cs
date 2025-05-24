using NavMeshPlus.Components;
using UnityEngine;

public class autoBake : MonoBehaviour
{
    public static autoBake instance;
    
    void Start()
    {
        instance = this;
    }
    
    public void Bake()
    {
        GetComponent<NavMeshSurface>().BuildNavMesh();
    }
}
