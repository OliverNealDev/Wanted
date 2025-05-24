using UnityEngine;

public class invisStart : MonoBehaviour
{
    void Start()
    {
        //GetComponent<SpriteRenderer>().material.color = new Color(0f, 0f, 0f, 0.5f);
        GetComponent<SpriteRenderer>().material.color = Color.clear;
    }
}
