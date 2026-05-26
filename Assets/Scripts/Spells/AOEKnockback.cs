using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows.Speech;

public class AOEKnockback : MonoBehaviour
{
    [Header("Cooldown")]
    [SerializeField] private float cooldownDuration = 1.5f;

    public float CooldownDuration => cooldownDuration;
    public float CooldownRemaining { get; private set; }
    public bool IsOnCooldown => CooldownRemaining > 0f;

    [Header("References")]
    [SerializeField] private EyeTargeting eyeTargeting;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Animator characterAnimator;

    [Header("Targeting")]
    [SerializeField] private float radius = 5f;

    [Header("Knockback")]
    [SerializeField] private float knockbackDistance = 6f;
    [SerializeField] private float upwardHeight = 3f;
    [SerializeField] private float knockbackDuration = 0.6f;
    [SerializeField] private float groundRaycastHeight = 20f;
    [SerializeField] private float groundRaycastDistance = 60f;
    [SerializeField] private float landingHeightOffset = 0.05f;

    private KeywordRecognizer keywordRecognizer;

    private void Start()
    {
        keywordRecognizer = new KeywordRecognizer(new string[] { "knock" });
        keywordRecognizer.OnPhraseRecognized += OnPhraseRecognized;
        keywordRecognizer.Start();

        Debug.Log("AOE Knockback ready. Press G or say Knock.");
    }

    private void Update()
    {
        if (CooldownRemaining > 0f)
        {
            CooldownRemaining -= Time.deltaTime;
        }

        if (Keyboard.current != null && Keyboard.current.gKey.wasPressedThisFrame)
        {
            CastKnockback();
        }
    }

    private void CastKnockback()
    {
        if (IsOnCooldown)
        {
            Debug.Log("Knockback is on cooldown.");
            return;
        }

        if (playerTransform == null)
        {
            Debug.LogWarning("AOEKnockback is missing Player Transform reference.");
            return;
        }

        bool castFromPlayer =
            Keyboard.current != null &&
            (Keyboard.current.leftAltKey.isPressed || Keyboard.current.rightAltKey.isPressed);

        Vector3 origin;
        EyeTargetable ignoredTarget = null;

        if (castFromPlayer)
        {
            origin = playerTransform.position;

            if (characterAnimator != null)
            {
                characterAnimator.SetTrigger("CastKnock");
            }
        }
        else
        {
            if (eyeTargeting == null)
            {
                Debug.LogWarning("AOEKnockback is missing EyeTargeting reference.");
                return;
            }

            EyeTargetable currentTarget = eyeTargeting.CurrentTarget;

            if (currentTarget == null)
            {
                Debug.Log("No target selected for knockback.");
                return;
            }

            origin = currentTarget.GetTargetPoint();
            ignoredTarget = currentTarget;
        }

        Collider[] hits = Physics.OverlapSphere(origin, radius);

        foreach (Collider hit in hits)
        {
            if (hit.GetComponentInParent<PlayerMovement>() != null)
            {
                continue;
            }

            EyeTargetable targetable = hit.GetComponentInParent<EyeTargetable>();

            if (targetable == null) continue;
            if (!targetable.CanBeTargeted) continue;
            if (targetable == ignoredTarget) continue;

            Levitatable levitatable = targetable.GetComponentInParent<Levitatable>();

            if (levitatable == null) continue;

            Vector3 direction = targetable.transform.position - origin;
            direction.y = 0f;

            if (direction == Vector3.zero)
            {
                direction = playerTransform.forward;
            }

            direction.Normalize();

            Vector3 endPosition =
                targetable.transform.position + direction * knockbackDistance;

            endPosition = SnapPositionToGround(endPosition);

            levitatable.ArcMoveTo(
                endPosition,
                upwardHeight,
                knockbackDuration
            );
        }

        CooldownRemaining = cooldownDuration;

        Debug.Log("AOE knockback cast.");
    }

    private Vector3 SnapPositionToGround(Vector3 position)
    {
        Vector3 rayStart = position + Vector3.up * groundRaycastHeight;

        if (Physics.Raycast(
            rayStart,
            Vector3.down,
            out RaycastHit hit,
            groundRaycastDistance
        ))
        {
            position.y = hit.point.y + landingHeightOffset;
        }

        return position;
    }

    private void OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        if (args.text.ToLower() == "knock")
        {
            CastKnockback();
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