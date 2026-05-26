using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows.Speech;
using System.Collections;

public class Frostbolt : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private Key castKey = Key.Y;

    [Header("Cooldown")]
    [SerializeField] private float cooldownDuration = 2f;

    public float CooldownDuration => cooldownDuration;
    public float CooldownRemaining { get; private set; }
    public bool IsOnCooldown => CooldownRemaining > 0f;

    [Header("References")]
    [SerializeField] private GameObject frostboltPrefab;
    [SerializeField] private Transform leftCastPoint;
    [SerializeField] private EyeTargeting eyeTargeting;
    [SerializeField] private Animator characterAnimator;
    [SerializeField] private SpellArmIK armIK;

    [Header("Casting")]
    [SerializeField] private float castDelay = 0.35f;
    [SerializeField] private float projectileSpeed = 18f;

    private KeywordRecognizer keywordRecognizer;

    private void Start()
    {
        keywordRecognizer = new KeywordRecognizer(new string[] { "frost" });
        keywordRecognizer.OnPhraseRecognized += OnPhraseRecognized;
        keywordRecognizer.Start();

        Debug.Log("Frostbolt ready. Press Y or say: frost");
    }

    private void Update()
    {
        if (CooldownRemaining > 0f)
        {
            CooldownRemaining -= Time.deltaTime;
        }

        if (Keyboard.current != null && Keyboard.current[castKey].wasPressedThisFrame)
        {
            CastAtCurrentTarget();
        }
    }

    private void CastAtCurrentTarget()
    {
        if (IsOnCooldown)
        {
            Debug.Log("Frostbolt is on cooldown.");
            return;
        }

        if (eyeTargeting == null)
        {
            Debug.LogWarning("Frostbolt is missing EyeTargeting reference.");
            return;
        }

        EyeTargetable currentTarget = eyeTargeting.CurrentTarget;

        if (currentTarget == null)
        {
            Debug.Log("No target selected for frostbolt.");
            return;
        }

        if (characterAnimator != null)
        {
            characterAnimator.SetTrigger("CastFrost");
        }

        if (armIK != null && leftCastPoint != null)
        {
            armIK.AimAtLimited(
                currentTarget.GetTargetPoint(),
                AvatarIKGoal.LeftHand,
                transform,
                leftCastPoint
            );
        }

        CooldownRemaining = cooldownDuration;

        StartCoroutine(SpawnFrostboltAfterDelay(currentTarget.transform));
    }

    private IEnumerator SpawnFrostboltAfterDelay(Transform targetTransform)
    {
        yield return new WaitForSeconds(castDelay);

        if (targetTransform == null)
        {
            yield break;
        }

        if (frostboltPrefab == null || leftCastPoint == null)
        {
            Debug.LogWarning("Frostbolt is missing prefab or cast point.");
            yield break;
        }

        GameObject frostbolt = Instantiate(
            frostboltPrefab,
            leftCastPoint.position,
            leftCastPoint.rotation
        );

        SimpleFireballProjectile projectile =
            frostbolt.GetComponent<SimpleFireballProjectile>();

        if (projectile == null)
        {
            projectile = frostbolt.AddComponent<SimpleFireballProjectile>();
        }

        projectile.LaunchAt(targetTransform, projectileSpeed);

        StartCoroutine(StopAimAfterDelay());

        Debug.Log("Frostbolt fired at target: " + targetTransform.name);
    }

    private IEnumerator StopAimAfterDelay()
    {
        yield return new WaitForSeconds(0.4f);

        if (armIK != null)
        {
            armIK.StopAiming();
        }
    }

    private void OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        if (args.text.ToLower() == "frost")
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
}