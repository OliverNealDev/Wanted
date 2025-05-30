using TMPro;
using UnityEngine;

public class devVisibility : MonoBehaviour
{
    private TextMeshProUGUI textMesh;
    void Start()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
        textMesh.enabled = false;
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F8))
        {
            textMesh.enabled = !textMesh.enabled;
        }
    }
}
