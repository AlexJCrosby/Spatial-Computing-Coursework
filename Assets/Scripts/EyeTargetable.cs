using UnityEngine;

public class EyeTargetable : MonoBehaviour
{
    private Outline outline;

    private void Awake()
    {
        outline = GetComponentInChildren<Outline>();

        if (outline != null)
        {
            outline.enabled = false;
        }
    }

    public void SetHighlighted(bool highlighted)
    {
        if (outline != null)
        {
            outline.enabled = highlighted;
        }
    }

    public Vector3 GetTargetPoint()
    {
        Collider col = GetComponentInChildren<Collider>();

        if (col != null)
        {
            return col.bounds.center;
        }

        return transform.position + Vector3.up * 1.2f;
    }
}