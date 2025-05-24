using System.Collections.Generic;
using NavMeshPlus.Components;
using UnityEngine;

public class proceduralMap : MonoBehaviour
{
    [SerializeField] private GameObject treePrefab;
    private List<GameObject> treeObjects = new List<GameObject>();
    
    private bool treesBaked = false;
    
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
    }

    void GenerateTree()
    {
        Vector2 position = new Vector2(Random.Range(-60f, 60f), Random.Range(3f, 100f));
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
}
