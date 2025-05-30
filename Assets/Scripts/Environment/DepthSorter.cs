using UnityEngine;

public class DepthSorter : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    public float yOffset = 0f;

    /*void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer not found on " + gameObject.name + " or its children for depth sorting.", this);
            enabled = false;
            return;
        }
    }

    void LateUpdate()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = Mathf.RoundToInt((transform.position.y + yOffset) * -100f);
        }
    }*/
}