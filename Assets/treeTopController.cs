using UnityEngine;

public class treeTopController : MonoBehaviour
{
    void Start()
    {
        GetComponent<SpriteRenderer>().color = new Color(Random.Range(0.5f, 0.6f), Random.Range(0.5f, 0.6f), Random.Range(0.5f, 0.6f), 1);
        int randomIndex = Random.Range(0, 4);
        switch (randomIndex)
        {
            case 0:
                transform.rotation = Quaternion.Euler(0, 0, 90);
                break;
            case 1:
                transform.rotation = Quaternion.Euler(0, 0, 180);
                break;
            case 2:
                transform.rotation = Quaternion.Euler(0, 0, 270);
                break;
            case 3:
                transform.rotation = Quaternion.Euler(0, 0, 0);
                break;
        }
    }
}
