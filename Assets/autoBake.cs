using NavMeshPlus.Components;
using UnityEngine;

public class autoBake : MonoBehaviour
{
    void Start()
    {
        //InvokeRepeating("Bake", 0, 10f);
    }
    
    void Bake()
    {
        GetComponent<NavMeshSurface>().BuildNavMesh();
    }
}
