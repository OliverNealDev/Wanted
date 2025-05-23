using System.Collections.Generic;
using UnityEngine;

public class proceduralMap : MonoBehaviour
{
    [SerializeField] private GameObject treePrefab;
    private List<GameObject> treeObjects = new List<GameObject>();
    
    void Update()
    {
        if (treeObjects.Count < 500)
        {
            GenerateTree();
            Debug.Log(treeObjects.Count);
        }
    }

    void GenerateTree()
    {
        Vector2 position = new Vector2(Random.Range(-60f, 60f), Random.Range(-4f, 100f));
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
