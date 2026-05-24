using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows.Speech;
using System.Collections;

public class EyeTargetingFrostboltCaster : MonoBehaviour
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
    [SerializeField] private Transform spellCastPoint;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private BeamAimProvider aimProvider;
    [SerializeField] private Animator characterAnimator;

    [Header("Targeting")]
    [SerializeField] private float castDelay = 0.35f;
    [SerializeField] private float maxScreenDistance = 180f;
    [SerializeField] private float projectileSpeed = 18f;

    private EyeTargetable[] targets;
    private EyeTargetable currentTarget;

    private KeywordRecognizer keywordRecognizer;

    private void Start()
    {
        targets = FindObjectsByType<EyeTargetable>(FindObjectsSortMode.None);

        keywordRecognizer = new KeywordRecognizer(new string[] { "frost" });
        keywordRecognizer.OnPhraseRecognized += OnPhraseRecognized;
        keywordRecognizer.Start();

        Debug.Log("Eye targeting frostbolt caster started. Press 2 or say: frost");
    }

    private void Update()
    {
        if (CooldownRemaining > 0f)
        {
            CooldownRemaining -= Time.deltaTime;
        }

        UpdateCurrentTarget();

        if (Keyboard.current != null && Keyboard.current[castKey].wasPressedThisFrame)
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

            Vector3 targetScreenPosition =
                playerCamera.WorldToScreenPoint(target.GetTargetPoint());

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

    private void CastAtCurrentTarget()
    {
        if (IsOnCooldown)
        {
            Debug.Log("Frostbolt is on cooldown.");
            return;
        }

        if (currentTarget == null)
        {
            Debug.Log("No eye target selected for frostbolt.");
            return;
        }

        if (characterAnimator != null)
        {
            characterAnimator.SetTrigger("CastSpell");
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

        GameObject frostbolt = Instantiate(
            frostboltPrefab,
            spellCastPoint.position,
            spellCastPoint.rotation
        );

        SimpleFireballProjectile projectile =
            frostbolt.GetComponent<SimpleFireballProjectile>();

        if (projectile == null)
        {
            projectile = frostbolt.AddComponent<SimpleFireballProjectile>();
        }

        projectile.LaunchAt(targetTransform, projectileSpeed);

        Debug.Log("Frostbolt fired at target: " + targetTransform.name);
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