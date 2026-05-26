using UnityEngine;
using UnityEngine.InputSystem;

public class TargetingSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private BeamAimProvider aimProvider;

    [Header("Eye Targeting")]
    [SerializeField] private float preciseTargetScreenDistance = 180f;
    [SerializeField] private bool snapToClosestVisibleTarget = true;

    [Header("Selected Targeting")]
    [SerializeField] private Key tabTargetKey = Key.Tab;
    [SerializeField] private bool allowLeftClickSelection = true;

    public EyeTargetable CurrentTarget => EyeTarget;
    public EyeTargetable EyeTarget { get; private set; }
    public EyeTargetable SelectedTarget { get; private set; }

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
        RefreshTargets();
        UpdateEyeTarget();
        HandleSelectedTargetInput();
    }

    private void RefreshTargets()
    {
        targets = FindObjectsByType<EyeTargetable>(FindObjectsSortMode.None);
    }

    private void UpdateEyeTarget()
    {
        Vector2 gazeScreenPosition = GetAimScreenPosition();

        EyeTargetable preciseTarget = null;
        float preciseDistance = preciseTargetScreenDistance;

        EyeTargetable closestVisibleTarget = null;
        float closestVisibleDistance = float.MaxValue;

        foreach (EyeTargetable target in targets)
        {
            if (!IsValidVisibleTarget(target)) continue;

            Vector3 screenPosition =
                playerCamera.WorldToScreenPoint(target.GetTargetPoint());

            float distance = Vector2.Distance(
                gazeScreenPosition,
                new Vector2(screenPosition.x, screenPosition.y)
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

        EyeTargetable newEyeTarget = preciseTarget;

        if (newEyeTarget == null && snapToClosestVisibleTarget)
        {
            newEyeTarget = closestVisibleTarget;
        }

        SetEyeTarget(newEyeTarget);
    }

    private void HandleSelectedTargetInput()
    {
        if (Keyboard.current != null && Keyboard.current[tabTargetKey].wasPressedThisFrame)
        {
            SelectNextVisibleTarget();
        }

        if (
            allowLeftClickSelection &&
            Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame
        )
        {
            SelectTargetUnderMouse();
        }

        if (SelectedTarget != null && !SelectedTarget.CanBeTargeted)
        {
            SetSelectedTarget(null);
        }
    }

    private void SelectNextVisibleTarget()
    {
        EyeTargetable nextTarget = GetNextVisibleTargetAfter(SelectedTarget);

        if (nextTarget != null)
        {
            SetSelectedTarget(nextTarget);
        }
    }

    private EyeTargetable GetNextVisibleTargetAfter(EyeTargetable current)
    {
        if (targets == null || targets.Length == 0) return null;

        int startIndex = 0;

        if (current != null)
        {
            for (int i = 0; i < targets.Length; i++)
            {
                if (targets[i] == current)
                {
                    startIndex = i + 1;
                    break;
                }
            }
        }

        for (int offset = 0; offset < targets.Length; offset++)
        {
            int index = (startIndex + offset) % targets.Length;
            EyeTargetable candidate = targets[index];

            if (IsValidVisibleTarget(candidate))
            {
                return candidate;
            }
        }

        return null;
    }

    private void SelectTargetUnderMouse()
    {
        Ray ray = playerCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, 500f))
        {
            EyeTargetable target =
                hit.collider.GetComponentInParent<EyeTargetable>();

            if (target != null && target.CanBeTargeted)
            {
                SetSelectedTarget(target);
            }
        }
    }

    private bool IsValidVisibleTarget(EyeTargetable target)
    {
        if (target == null) return false;
        if (!target.CanBeTargeted) return false;
        if (playerCamera == null) return false;

        Vector3 screenPosition =
            playerCamera.WorldToScreenPoint(target.GetTargetPoint());

        if (screenPosition.z < 0f) return false;
        if (screenPosition.x < 0f || screenPosition.x > Screen.width) return false;
        if (screenPosition.y < 0f || screenPosition.y > Screen.height) return false;

        return true;
    }

    private void SetEyeTarget(EyeTargetable newTarget)
    {
        if (EyeTarget == newTarget) return;

        UpdateHighlight(EyeTarget);
        EyeTarget = newTarget;
        UpdateHighlight(EyeTarget);
    }

    private void SetSelectedTarget(EyeTargetable newTarget)
    {
        if (SelectedTarget == newTarget) return;

        UpdateHighlight(SelectedTarget);
        SelectedTarget = newTarget;
        UpdateHighlight(SelectedTarget);
    }

    private void UpdateHighlight(EyeTargetable target)
    {
        if (target == null) return;

        bool isEyeTarget = target == EyeTarget;
        bool isSelectedTarget = target == SelectedTarget;

        target.SetHighlighted(
            isEyeTarget || isSelectedTarget,
            isSelectedTarget
        );
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