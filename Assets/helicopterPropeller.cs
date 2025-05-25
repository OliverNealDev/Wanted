using UnityEngine;

public class helicopterPropeller : MonoBehaviour
{
    void Update()
    {
        transform.Rotate(0, 0, 1440 * Time.deltaTime, Space.Self);
    }
}
