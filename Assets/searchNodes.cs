using System;
using System.Collections.Generic;
using UnityEngine;

public class searchNodes : MonoBehaviour
{
    public static searchNodes instance;
    
    public List<Transform> searchNodesList = new List<Transform>();

    private void Start()
    {
        instance = this;
        searchNodesList.Clear();
        foreach (Transform child in transform.GetChild(0).transform)
        {
            searchNodesList.Add(child);
        }
        foreach (Transform child in transform.GetChild(1).transform)
        {
            searchNodesList.Add(child);
        }
        foreach (Transform child in transform.GetChild(2).transform)
        {
            searchNodesList.Add(child);
        }
        foreach (Transform child in transform.GetChild(3).transform)
        {
            searchNodesList.Add(child);
        }
    }
}
