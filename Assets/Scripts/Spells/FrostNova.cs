using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows.Speech;

public class FrostNova : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private Key keybind = Key.R;

    [Header("Frost Nova")]
    [SerializeField] private float duration = 3f;
    [SerializeField] private float radius = 5f;
    [SerializeField] private float cooldownDuration = 8f;

    public float CooldownDuration => cooldownDuration;
    public float CooldownRemaining { get; private set; }
    public bool IsOnCooldown => CooldownRemaining > 0f;

    [Header("References")]
    [SerializeField] private TargetingSystem targetingSystem;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Animator characterAnimator;
    [SerializeField] private LayerMask groundLayerMask;

    [Header("VFX")]
    [SerializeField] private GameObject aoeVfxPrefab;
    [SerializeField] private float aoeVfxLifetime = 5f;
    [SerializeField] private GameObject rootVfxPrefab;
    [SerializeField] private Vector3 rootVfxLocalOffset = Vector3.zero;

    [Header("Grounding")]
    [SerializeField] private float groundRaycastHeight = 20f;
    [SerializeField] private float groundRaycastDistance = 60f;
    [SerializeField] private float groundOffset = 0.05f;

    [Header("Animation")]
    [SerializeField] private string frostNovaTrigger = "CastFrost";

    private KeywordRecognizer keywordRecognizer;

    private readonly Dictionary<GoblinAI, Coroutine> activeFreezes = new();
    private readonly Dictionary<GoblinAI, GameObject> activeRootVfx = new();

    private void Start()
    {
        keywordRecognizer = new KeywordRecognizer(new string[] { "freeze" });
        keywordRecognizer.OnPhraseRecognized += OnPhraseRecognized;
        keywordRecognizer.Start();

        Debug.Log("Frost Nova ready. Press R / Alt+R or say Freeze.");
    }

    private void Update()
    {
        if (CooldownRemaining > 0f)
        {
            CooldownRemaining -= Time.deltaTime;
        }

        if (Keyboard.current == null) return;

        if (keybind != Key.None && Keyboard.current[keybind].wasPressedThisFrame)
        {
            bool castFromPlayer =
                Keyboard.current.leftAltKey.isPressed ||
                Keyboard.current.rightAltKey.isPressed;

            CastFrostNova(castFromPlayer);
        }
    }

    private void CastFrostNova(bool castFromPlayer)
    {
        if (IsOnCooldown)
        {
            Debug.Log("Frost Nova is on cooldown.");
            return;
        }

        if (playerTransform == null)
        {
            Debug.LogWarning("FrostNova is missing Player Transform reference.");
            return;
        }

        Vector3 origin;

        if (castFromPlayer)
        {
            origin = playerTransform.position;
        }
        else
        {
            if (targetingSystem == null)
            {
                Debug.LogWarning("FrostNova is missing TargetingSystem reference.");
                return;
            }

            EyeTargetable currentTarget = targetingSystem.CurrentTarget;

            if (currentTarget == null)
            {
                Debug.Log("No target selected for Frost Nova.");
                return;
            }

            origin = currentTarget.GetTargetPoint();
        }

        if (characterAnimator != null && !string.IsNullOrWhiteSpace(frostNovaTrigger))
        {
            characterAnimator.SetTrigger(frostNovaTrigger);
        }

        CooldownRemaining = cooldownDuration;

        SpawnAoeVfx(origin);
        FreezeTargets(origin);

        Debug.Log("Frost Nova cast.");
    }

    private void FreezeTargets(Vector3 origin)
    {
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

            GoblinAI goblinAI = targetable.GetComponentInParent<GoblinAI>();

            if (goblinAI == null) continue;

            if (activeFreezes.TryGetValue(goblinAI, out Coroutine existingFreeze))
            {
                StopCoroutine(existingFreeze);
                activeFreezes.Remove(goblinAI);
            }

            if (activeRootVfx.TryGetValue(goblinAI, out GameObject existingVfx))
            {
                if (existingVfx != null)
                {
                    Destroy(existingVfx);
                }

                activeRootVfx.Remove(goblinAI);
            }

            activeFreezes[goblinAI] = StartCoroutine(
                FreezeRoutine(goblinAI, targetable.transform)
            );
        }
    }

    private IEnumerator FreezeRoutine(GoblinAI goblinAI, Transform targetRoot)
    {
        if (goblinAI == null) yield break;

        Animator targetAnimator = goblinAI.GetComponentInChildren<Animator>();
        Rigidbody rb = goblinAI.GetComponent<Rigidbody>();
        CharacterController controller = goblinAI.GetComponent<CharacterController>();
        Levitatable levitatable = goblinAI.GetComponent<Levitatable>();

        if (levitatable != null)
        {
            levitatable.StopAllCoroutines();
        }

        goblinAI.IsFrozen = true;
        goblinAI.enabled = false;

        if (controller != null)
        {
            controller.enabled = false;
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (targetAnimator != null)
        {
            targetAnimator.SetFloat("Speed", 0f);
            targetAnimator.SetInteger("AttackIndex", 0);
        }

        GameObject rootVfx = null;

        if (rootVfxPrefab != null && targetRoot != null)
        {
            rootVfx = Instantiate(rootVfxPrefab, targetRoot);
            rootVfx.transform.localPosition = rootVfxLocalOffset;
            rootVfx.transform.localRotation = Quaternion.identity;

            activeRootVfx[goblinAI] = rootVfx;
        }

        yield return new WaitForSeconds(duration);

        if (rootVfx != null)
        {
            Destroy(rootVfx);
        }

        activeRootVfx.Remove(goblinAI);

        if (goblinAI != null)
        {
            CharacterController currentController = goblinAI.GetComponent<CharacterController>();

            if (currentController != null)
            {
                currentController.enabled = true;
            }

            goblinAI.IsFrozen = false;
            goblinAI.enabled = true;

            activeFreezes.Remove(goblinAI);
        }
    }

    private void SpawnAoeVfx(Vector3 origin)
    {
        if (aoeVfxPrefab == null) return;

        Vector3 spawnPosition = SnapPositionToGround(origin);
        spawnPosition.y += groundOffset;

        GameObject vfx = Instantiate(
            aoeVfxPrefab,
            spawnPosition,
            Quaternion.identity
        );

        Destroy(vfx, aoeVfxLifetime);
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
            position.y = hit.point.y;
        }

        return position;
    }

    private void OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        if (args.text.ToLower() == "freeze")
        {
            CastFrostNova(false);
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