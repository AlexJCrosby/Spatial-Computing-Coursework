using UnityEngine;

public class EyeTargetable : MonoBehaviour
{
    [SerializeField] private Color normalHighlightColor = Color.yellow;
    [SerializeField] private Color lockedHighlightColor = Color.white;

    private Outline outline;

    private void Awake()
    {
        outline = GetComponentInChildren<Outline>();

        if (outline != null)
        {
            outline.enabled = false;
            outline.OutlineColor = normalHighlightColor;
        }
    }

    public void SetHighlighted(bool highlighted)
    {
        SetHighlighted(highlighted, false);
    }

    public void SetHighlighted(bool highlighted, bool locked)
    {
        if (outline == null) return;

        outline.enabled = highlighted;
        outline.OutlineColor = locked ? lockedHighlightColor : normalHighlightColor;
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