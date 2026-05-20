using UnityEngine;

public class EyeTargetable : MonoBehaviour
{
    [SerializeField] private Color highlightedColor = Color.red;

    private Renderer objectRenderer;
    private Color originalColor;

    private void Awake()
    {
        objectRenderer = GetComponentInChildren<Renderer>();

        if (objectRenderer != null)
        {
            originalColor = objectRenderer.material.color;
        }
    }

    public void SetHighlighted(bool highlighted)
    {
        if (objectRenderer == null) return;

        objectRenderer.material.color = highlighted
            ? highlightedColor
            : originalColor;
    }

    public Vector3 GetTargetPoint()
    {
        Collider col = GetComponentInChildren<Collider>();

        if (col != null)
        {
            return col.bounds.center;
        }

        return transform.position;
    }
}