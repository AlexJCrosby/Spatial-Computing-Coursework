using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows.Speech;
using System.Collections;

public class Fireball : MonoBehaviour
{
    [Header("Cooldown")]
    [SerializeField] private float cooldownDuration = 2f;

    public float CooldownDuration => cooldownDuration;
    public float CooldownRemaining { get; private set; }
    public bool IsOnCooldown => CooldownRemaining > 0f;

    [Header("References")]
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private Transform spellCastPoint;
    [SerializeField] private EyeTargeting eyeTargeting;
    [SerializeField] private Animator characterAnimator;
    [SerializeField] private SpellArmIK armIK;

    [Header("Casting")]
    [SerializeField] private float castDelay = 0.35f;
    [SerializeField] private float projectileSpeed = 14f;

    private KeywordRecognizer keywordRecognizer;

    private void Start()
    {
        keywordRecognizer = new KeywordRecognizer(new string[] { "fire" });
        keywordRecognizer.OnPhraseRecognized += OnPhraseRecognized;
        keywordRecognizer.Start();

        Debug.Log("Fireball ready. Press R or say: fire");
    }

    private void Update()
    {
        if (CooldownRemaining > 0f)
        {
            CooldownRemaining -= Time.deltaTime;
        }

        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            CastAtCurrentTarget();
        }
    }

    private void CastAtCurrentTarget()
    {
        if (IsOnCooldown)
        {
            Debug.Log("Fireball is on cooldown.");
            return;
        }

        if (eyeTargeting == null)
        {
            Debug.LogWarning("Fireball is missing EyeTargeting reference.");
            return;
        }

        EyeTargetable currentTarget = eyeTargeting.CurrentTarget;

        if (currentTarget == null)
        {
            Debug.Log("No target selected for fireball.");
            return;
        }

        if (characterAnimator != null)
        {
            characterAnimator.SetTrigger("CastSpell");
        }

        if (armIK != null && spellCastPoint != null)
        {
            armIK.AimAtLimited(
                currentTarget.GetTargetPoint(),
                AvatarIKGoal.RightHand,
                transform,
                spellCastPoint
            );
        }

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

        if (fireballPrefab == null || spellCastPoint == null)
        {
            Debug.LogWarning("Fireball is missing prefab or cast point.");
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

        Debug.Log("Fireball fired at target: " + targetTransform.name);
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
}