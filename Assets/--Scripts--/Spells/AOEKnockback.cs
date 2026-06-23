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
    [SerializeField] private TargetingSystem targetingSystem;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Animator characterAnimator;
    [SerializeField] private LayerMask groundLayerMask;
    // [SerializeField] private GameObject crackDecalPrefab;

    [Header("VFX")]
    [SerializeField] private GameObject knockbackVfxPrefab;
    [SerializeField] private float vfxLifetime = 5f;

    [Header("Targeting")]
    [SerializeField] private float radius = 5f;

    [Header("Knockback")]
    [SerializeField] private float knockbackDistance = 6f;
    [SerializeField] private float upwardHeight = 3f;
    [SerializeField] private float knockbackDuration = 0.6f;
    [SerializeField] private float groundRaycastHeight = 20f;
    [SerializeField] private float groundRaycastDistance = 60f;
    [SerializeField] private float landingHeightOffset = 0.05f;
    [SerializeField] private float vfxGroundOffset = 0.05f;

    // [SerializeField] private float decalLifetime = 6f;

    private KeywordRecognizer keywordRecognizer;

    private void Start()
    {
        keywordRecognizer = new KeywordRecognizer(new string[] { "knock" });
        keywordRecognizer.OnPhraseRecognized += OnPhraseRecognized;
        keywordRecognizer.Start();

        Debug.Log("AOE Knockback ready. Press G, Alt+G, or say Knock.");
    }

    private void Update()
    {
        if (CooldownRemaining > 0f)
        {
            CooldownRemaining -= Time.deltaTime;
        }

        if (Keyboard.current != null && Keyboard.current.gKey.wasPressedThisFrame)
        {
            bool castFromPlayer =
                Keyboard.current.leftAltKey.isPressed ||
                Keyboard.current.rightAltKey.isPressed;

            CastKnockback(castFromPlayer);
        }
    }

    private void CastKnockback(bool castFromPlayer)
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
            if (targetingSystem == null)
            {
                Debug.LogWarning("AOEKnockback is missing TargetingSystem reference.");
                return;
            }

            EyeTargetable currentTarget = targetingSystem.CurrentTarget;

            if (currentTarget == null)
            {
                Debug.Log("No target selected for knockback.");
                return;
            }

            origin = currentTarget.GetTargetPoint();
            ignoredTarget = currentTarget;
        }

        SpawnKnockbackVfx(origin);

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

    private void SpawnKnockbackVfx(Vector3 origin)
    {
        if (knockbackVfxPrefab == null) return;

        Vector3 spawnPosition = SnapPositionToGround(origin);
        spawnPosition.y += vfxGroundOffset;

        GameObject vfx = Instantiate(
            knockbackVfxPrefab,
            spawnPosition,
            Quaternion.identity
        );

        /* if (crackDecalPrefab != null)
        {
            RaycastHit hit;

            Vector3 rayStart = transform.position + Vector3.up * 5f;

            if (Physics.Raycast(rayStart, Vector3.down, out hit, 20f))
            {
                GameObject decal = Instantiate(
                    crackDecalPrefab,
                    hit.point + Vector3.up * 0.02f,
                    Quaternion.Euler(90f, Random.Range(0f, 360f), 0f)
                );

                Destroy(decal, decalLifetime);
            }
        }*/

        Destroy(vfx, vfxLifetime);
    }

    private Vector3 SnapPositionToGround(Vector3 position)
    {
        Vector3 rayStart = position + Vector3.up * groundRaycastHeight;

        if (Physics.Raycast(
             rayStart,
             Vector3.down,
             out RaycastHit hit,
             groundRaycastDistance,
             groundLayerMask
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
            CastKnockback(true);
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