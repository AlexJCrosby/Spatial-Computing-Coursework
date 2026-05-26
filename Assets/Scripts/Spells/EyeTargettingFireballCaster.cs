using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows.Speech;
using System.Collections;

public class EyeTargetingFireballCaster : MonoBehaviour
{
    [Header("Cooldown")]
    [SerializeField] private float cooldownDuration = 2f;

    public float CooldownDuration => cooldownDuration;
    public float CooldownRemaining { get; private set; }
    public bool IsOnCooldown => CooldownRemaining > 0f;

    [Header("References")]
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private Transform spellCastPoint;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private BeamAimProvider aimProvider;
    [SerializeField] private Animator characterAnimator;
    [SerializeField] private SpellArmIK armIK;

    [Header("Targeting")]
    [SerializeField] private float castDelay = 0.35f;
    [SerializeField] private float maxScreenDistance = 180f;
    [SerializeField] private float projectileSpeed = 14f;

    private EyeTargetable[] targets;
    private EyeTargetable currentTarget;

    private KeywordRecognizer keywordRecognizer;

    private void Start()
    {
        targets = FindObjectsByType<EyeTargetable>(FindObjectsSortMode.None);

        keywordRecognizer = new KeywordRecognizer(new string[] { "fire" });
        keywordRecognizer.OnPhraseRecognized += OnPhraseRecognized;
        keywordRecognizer.Start();

        Debug.Log("Eye targeting fireball caster started. Press R or say: fire");
    }

    private void Update()
    {
        UpdateCurrentTarget();
        if (CooldownRemaining > 0f)
        {
            CooldownRemaining -= Time.deltaTime;
        }
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            CastAtCurrentTarget();
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

    private void CastAtCurrentTarget()
    {
        if (IsOnCooldown)
        {
            Debug.Log("Fireball is on cooldown.");
            return;
        }

        if (currentTarget == null)
        {
            Debug.Log("No eye target selected.");
            return;
        }

        if (characterAnimator != null)
        {
            characterAnimator.SetTrigger("CastSpell");
        }
        Vector3 target = currentTarget.GetTargetPoint();
        Vector3 handPoint = spellCastPoint.position;
        Vector3 aimPoint = handPoint + (target - handPoint).normalized * 2f;

        armIK.AimAtLimited(
            currentTarget.GetTargetPoint(),
            AvatarIKGoal.RightHand,
            transform,
            spellCastPoint
        );

        CooldownRemaining = cooldownDuration;

        StartCoroutine(SpawnFireballAfterDelay(currentTarget.transform));
    }

    private IEnumerator SpawnFireballAfterDelay(Transform targetTransform)
    {
        yield return new WaitForSeconds(castDelay);

        if (targetTransform == null)
        {
            yield break;
        }

        GameObject fireball = Instantiate(
            fireballPrefab,
            spellCastPoint.position,
            spellCastPoint.rotation
        );

        SimpleFireballProjectile projectile =
            fireball.GetComponent<SimpleFireballProjectile>();

        if (projectile == null)
        {
            projectile = fireball.AddComponent<SimpleFireballProjectile>();
        }

        projectile.LaunchAt(targetTransform, projectileSpeed);
        StartCoroutine(StopAimAfterDelay());
        Debug.Log("Fired at target: " + targetTransform.name);
    }

    private void OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        if (args.text.ToLower() == "fire")
        {
            CastAtCurrentTarget();
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

    private IEnumerator StopAimAfterDelay()
    {
        yield return new WaitForSeconds(0.4f);

        if (armIK != null)
        {
            armIK.StopAiming();
        }
    }
}