using TMPro;
using UnityEngine;

public class textManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI fpsText;
    void Start()
    {
        InvokeRepeating("fpsUpdate", 0.1f, 0.1f);
    }
    
    void fpsUpdate()
    {
        float fps = 1.0f / Time.deltaTime;
        fpsText.text = "FPS: " + Mathf.RoundToInt(fps).ToString();
    }
}
