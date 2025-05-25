using System.Collections.Generic;
using NavMeshPlus.Components;
using UnityEngine;

public class proceduralMap : MonoBehaviour
{
    [SerializeField] private GameObject treePrefab;
    private List<GameObject> treeObjects = new List<GameObject>();
    
    [SerializeField] private GameObject bushPrefab;
    private List<GameObject> bushObjects = new List<GameObject>();
    
    private bool treesBaked = false;
    private Vector2 position;
    
    void Update()
    {
        if (treeObjects.Count < 500)
        {
            GenerateTree();
        }
        else if (treesBaked == false)
        {
            treesBaked = true;
            autoBake.instance.Bake();
        }
        
        if (bushObjects.Count < 50)
        {
            GenerateBush();
        }
    }

    void GenerateTree()
    {
        bool randomBool = Random.Range(0f, 1f) < 0.5f;
        if (randomBool)
        {
            position = new Vector2(Random.Range(-80f, 80f), Random.Range(12f, 80f));
        }
        else
        {
            position = new Vector2(Random.Range(-80f, 80f), Random.Range(-12f, -80f));
        }
        
        for (int i = 0; i < bushObjects.Count; i++)
        {
            if (Vector2.Distance(bushObjects[i].transform.position, position) < 2.5f)
            {
                return; // Too close to another bush, don't spawn
            }
        }
        for (int i = 0; i < treeObjects.Count; i++)
        {
            if (Vector2.Distance(treeObjects[i].transform.position, position) < 2.5f)
            {
                return; // Too close to another tree, don't spawn
            }
        }
        
        GameObject tree = Instantiate(treePrefab, position, Quaternion.identity);
        treeObjects.Add(tree);
    }

    void GenerateBush()
    {
        bool randomBool = Random.Range(0f, 1f) < 0.5f;
        if (randomBool)
        {
            position = new Vector2(Random.Range(-80f, 80f), Random.Range(6f, 80f));
        }
        else
        {
            position = new Vector2(Random.Range(-80f, 80f), Random.Range(-6f, -80f));
        }
        
        for (int i = 0; i < bushObjects.Count; i++)
        {
            if (Vector2.Distance(bushObjects[i].transform.position, position) < 2.5f)
            {
                return; // Too close to another bush, don't spawn
            }
        }
        for (int i = 0; i < treeObjects.Count; i++)
        {
            if (Vector2.Distance(treeObjects[i].transform.position, position) < 2.5f)
            {
                return; // Too close to another bush, don't spawn
            }
        }
        
        GameObject bush = Instantiate(bushPrefab, position, Quaternion.identity);
        bushObjects.Add(bush);
    }
}
