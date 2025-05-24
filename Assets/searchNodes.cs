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
    }
}
