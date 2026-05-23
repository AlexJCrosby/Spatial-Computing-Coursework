using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows.Speech;

public class AOEKnockbackSpellCaster : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private BeamAimProvider aimProvider;
    [SerializeField] private Transform playerTransform;

    [Header("Targeting")]
    [SerializeField] private float maxScreenDistance = 180f;
    [SerializeField] private float radius = 5f;

    [Header("Knockback")]
    [SerializeField] private float knockbackDistance = 6f;
    [SerializeField] private float upwardHeight = 3f;
    [SerializeField] private float knockbackDuration = 0.6f;

    private EyeTargetable[] targets;
    private EyeTargetable currentTarget;
    private KeywordRecognizer keywordRecognizer;

    private void Start()
    {
        targets = FindObjectsByType<EyeTargetable>(FindObjectsSortMode.None);

        keywordRecognizer = new KeywordRecognizer(new string[] { "knock" });
        keywordRecognizer.OnPhraseRecognized += OnPhraseRecognized;
        keywordRecognizer.Start();

        Debug.Log("AOE Knockback ready. Press G or say Knock.");
    }

    private void Update()
    {
        UpdateCurrentTarget();

        if (Keyboard.current.gKey.wasPressedThisFrame)
        {
            CastKnockback();
        }
    }

    private void CastKnockback()
    {
        bool castFromPlayer = Keyboard.current.leftAltKey.isPressed || Keyboard.current.rightAltKey.isPressed;

        Vector3 origin;

        EyeTargetable ignoredTarget = null;

        if (castFromPlayer)
        {
            origin = playerTransform.position;
        }
        else
        {
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

            if (targetable == null)
            {
                continue;
            }

            if (targetable == ignoredTarget)
            {
                continue;
            }

            Levitatable levitatable = targetable.GetComponentInParent<Levitatable>();

            if (levitatable == null)
            {
                continue;
            }

            Vector3 direction = targetable.transform.position - origin;
            direction.y = 0f;

            if (direction == Vector3.zero)
            {
                direction = playerTransform.forward;
            }

            direction.Normalize();

            Vector3 endPosition = targetable.transform.position + direction * knockbackDistance;
            endPosition.y = targetable.transform.position.y;

            levitatable.ArcMoveTo(endPosition, upwardHeight, knockbackDuration);
        }

        Debug.Log("AOE knockback cast.");
    }

    private void UpdateCurrentTarget()
    {
        Vector2 gazeScreenPosition = GetGazeScreenPosition();

        EyeTargetable closestTarget = null;
        float closestDistance = maxScreenDistance;

        foreach (EyeTargetable target in targets)
        {
            if (target == null) continue;

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

        currentTarget = closestTarget;
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