using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows.Speech;

public class TelekinesisSpellCaster : MonoBehaviour
{
    [Header("Cooldown")]
    [SerializeField] private float cooldownDuration = 1.5f;

    public float CooldownDuration => cooldownDuration;
    public float CooldownRemaining { get; private set; }
    public bool IsOnCooldown => CooldownRemaining > 0f;

    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private BeamAimProvider aimProvider;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private EyeTargetingFireballCaster fireballCaster;
    [SerializeField] private Animator characterAnimator;

    [Header("Targeting")]
    [SerializeField] private float maxScreenDistance = 180f;

    [Header("Levitation Control")]
    [SerializeField] private float horizontalAngleRange = 80f;
    [SerializeField] private float verticalMoveRange = 8;
    [SerializeField] private float minimumHeight = 1.5f;

    private EyeTargetable[] targets;
    private EyeTargetable currentTarget;
    private EyeTargetable lockedTarget;
    private Levitatable levitatedObject;

    private Vector2 levitateStartGaze;
    private Vector3 levitateStartDirection;
    private float levitateRadius;
    private float levitateStartHeight;

    private KeywordRecognizer keywordRecognizer;

    private void Start()
    {
        targets = FindObjectsByType<EyeTargetable>(FindObjectsSortMode.None);

        keywordRecognizer = new KeywordRecognizer(new string[]
        {
            "levitate",
            "pull",
            "throw"
        });

        keywordRecognizer.OnPhraseRecognized += OnPhraseRecognized;
        keywordRecognizer.Start();

        Debug.Log("Telekinesis ready. Press T or say Levitate.");
    }

    private void Update()
    {
        if (CooldownRemaining > 0f)
        {
            CooldownRemaining -= Time.deltaTime;
        }
        if (levitatedObject == null)
        {
            UpdateCurrentTarget();
        }
        else
        {
            UpdateLevitationPosition();
        }

        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            ToggleLevitate();
        }

        if (Keyboard.current.digit3Key.wasPressedThisFrame)
        {
            Pull();
        }

        if (Keyboard.current.digit4Key.wasPressedThisFrame)
        {
            Throw();
        }
    }

    private void UpdateCurrentTarget()
    {
        targets = FindObjectsByType<EyeTargetable>(FindObjectsSortMode.None);

        Vector2 gazeScreenPosition = GetGazeScreenPosition();

        EyeTargetable closestTarget = null;
        float closestDistance = maxScreenDistance;

        foreach (EyeTargetable target in targets)
        {
            if (target == null) continue;
            if (!target.CanBeTargeted) continue;

            Vector3 targetScreenPosition = playerCamera.WorldToScreenPoint(target.GetTargetPoint());

            if (targetScreenPosition.z < 0) continue;

            float distance = Vector2.Distance(
                gazeScreenPosition,
                new Vector2(targetScreenPosition.x, targetScreenPosition.y)
            );

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = target;
            }
        }

        if (currentTarget != closestTarget)
        {
            if (currentTarget != null)
            {
                currentTarget.SetHighlighted(false);
            }

            currentTarget = closestTarget;

            if (currentTarget != null)
            {
                currentTarget.SetHighlighted(true);
            }
        }
    }

    private void ToggleLevitate()
    {
        if (levitatedObject != null)
        {
            CancelLevitate();
            return;
        }

        if (IsOnCooldown)
        {
            Debug.Log("Telekinesis is on cooldown.");
            return;
        }

        if (currentTarget == null) return;

        Levitatable levitatable = currentTarget.GetComponent<Levitatable>();

        if (levitatable == null)
        {
            levitatable = currentTarget.GetComponentInParent<Levitatable>();
        }

        if (levitatable == null)
        {
            Debug.Log("Target cannot be levitated.");
            return;
        }

        lockedTarget = currentTarget;
        lockedTarget.SetHighlighted(true);

        levitatedObject = levitatable;

        BeginLevitationLock();

        if (characterAnimator != null)
        {
            characterAnimator.SetTrigger("CastLevitate");
        }
        levitatedObject.BeginLevitate(GetLockedLevitateWorldPosition());

        CooldownRemaining = cooldownDuration;
    }

    private void BeginLevitationLock()
    {
        levitateStartGaze = GetGazeScreenPosition();

        Vector3 offset = levitatedObject.transform.position - playerTransform.position;
        offset.y = 0f;

        levitateRadius = offset.magnitude;
        levitateStartDirection = offset.normalized;

        if (levitateStartDirection == Vector3.zero)
        {
            levitateStartDirection = playerTransform.forward;
            levitateRadius = 4f;
        }

        levitateStartHeight = levitatedObject.transform.position.y;
    }

    private void UpdateLevitationPosition()
    {
        levitatedObject.SetLevitateTarget(GetLockedLevitateWorldPosition());
    }

    private Vector3 GetLockedLevitateWorldPosition()
    {
        Vector2 gaze = GetGazeScreenPosition();

        float gazeDeltaX = (gaze.x - levitateStartGaze.x) / Screen.width;
        float gazeDeltaY = (gaze.y - levitateStartGaze.y) / Screen.height;

        float angleOffset = gazeDeltaX * horizontalAngleRange;
        float heightOffset = gazeDeltaY * verticalMoveRange;

        Vector3 direction = Quaternion.AngleAxis(angleOffset, Vector3.up) * levitateStartDirection;

        Vector3 targetPosition = playerTransform.position + direction * levitateRadius;
        targetPosition.y = Mathf.Max(levitateStartHeight + heightOffset, minimumHeight);

        return targetPosition;
    }

    private void Pull()
    {
        if (levitatedObject == null) return;

        levitatedObject.PullTowards(playerTransform.position);
        ClearLockedTarget();
    }

    private void Throw()
    {
        if (levitatedObject == null) return;

        levitatedObject.ThrowAwayFrom(playerTransform.position);
        ClearLockedTarget();
    }

    private void CancelLevitate()
    {
        if (levitatedObject != null)
        {
            levitatedObject.EndLevitate();
        }

        ClearLockedTarget();
    }

    private void ClearLockedTarget()
    {
        if (lockedTarget != null)
        {
            lockedTarget.SetHighlighted(false);
        }

        lockedTarget = null;
        levitatedObject = null;
        currentTarget = null;
    }

    private Vector2 GetGazeScreenPosition()
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

    private void OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        string command = args.text.ToLower();

        if (command == "levitate")
        {
            ToggleLevitate();
        }
        else if (command == "pull")
        {
            Pull();
        }
        else if (command == "throw")
        {
            Throw();
        }
    }

    private void OnDestroy()
    {
        if (keywordRecognizer == null) return;

        if (keywordRecognizer.IsRunning)
        {
            keywordRecognizer.Stop();
        }

        keywordRecognizer.Dispose();
    }
}