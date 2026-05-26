using UnityEngine;
using UnityEngine.InputSystem;

public class EyeTargeting : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private BeamAimProvider aimProvider;

    [Header("Targeting")]
    [SerializeField] private float preciseTargetScreenDistance = 180f;
    [SerializeField] private bool snapToClosestVisibleTarget = true;
    [SerializeField] private bool highlightCurrentTarget = true;

    public EyeTargetable CurrentTarget { get; private set; }

    private EyeTargetable[] targets;

    private void Awake()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
    }

    private void Update()
    {
        UpdateCurrentTarget();
    }

    private void UpdateCurrentTarget()
    {
        targets = FindObjectsByType<EyeTargetable>(FindObjectsSortMode.None);

        Vector2 gazeScreenPosition = GetAimScreenPosition();

        EyeTargetable preciseTarget = null;
        float preciseDistance = preciseTargetScreenDistance;

        EyeTargetable closestVisibleTarget = null;
        float closestVisibleDistance = float.MaxValue;

        foreach (EyeTargetable target in targets)
        {
            if (target == null) continue;
            if (!target.CanBeTargeted) continue;

            Vector3 targetScreenPosition =
                playerCamera.WorldToScreenPoint(target.GetTargetPoint());

            if (!IsVisibleOnScreen(targetScreenPosition)) continue;

            float distance = Vector2.Distance(
                gazeScreenPosition,
                new Vector2(targetScreenPosition.x, targetScreenPosition.y)
            );

            if (distance < preciseDistance)
            {
                preciseDistance = distance;
                preciseTarget = target;
            }

            if (distance < closestVisibleDistance)
            {
                closestVisibleDistance = distance;
                closestVisibleTarget = target;
            }
        }

        EyeTargetable selectedTarget = preciseTarget;

        if (selectedTarget == null && snapToClosestVisibleTarget)
        {
            selectedTarget = closestVisibleTarget;
        }

        SetCurrentTarget(selectedTarget);
    }

    private bool IsVisibleOnScreen(Vector3 screenPosition)
    {
        if (screenPosition.z < 0f) return false;

        if (screenPosition.x < 0f || screenPosition.x > Screen.width) return false;
        if (screenPosition.y < 0f || screenPosition.y > Screen.height) return false;

        return true;
    }

    private void SetCurrentTarget(EyeTargetable newTarget)
    {
        if (CurrentTarget == newTarget) return;

        if (highlightCurrentTarget && CurrentTarget != null)
        {
            CurrentTarget.SetHighlighted(false);
        }

        CurrentTarget = newTarget;

        if (highlightCurrentTarget && CurrentTarget != null)
        {
            CurrentTarget.SetHighlighted(true);
        }
    }

    public Vector2 GetAimScreenPosition()
    {
        if (aimProvider != null)
        {
            return aimProvider.GetAimScreenPosition();
        }

        if (Mouse.current != null)
        {
            return Mouse.current.position.ReadValue();
        }

        return new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
    }
}