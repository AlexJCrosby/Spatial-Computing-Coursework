using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows.Speech;

public class VolcanicBomb : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private Key keybind = Key.None;
    [SerializeField] private string voiceCommand = "bomb";

    [Header("Volcanic Bomb")]
    [SerializeField] private float delayBeforeExplosion = 3f;
    [SerializeField] private int damage = 3;
    [SerializeField] private float explosionRadius = 3f;
    [SerializeField] private float cooldownDuration = 8f;

    public float CooldownDuration => cooldownDuration;
    public float CooldownRemaining { get; private set; }
    public bool IsOnCooldown => CooldownRemaining > 0f;

    [Header("References")]
    [SerializeField] private TargetingSystem targetingSystem;
    [SerializeField] private Animator characterAnimator;

    [Header("Animation")]
    [SerializeField] private string castTrigger = "CastBomb";
    [SerializeField] private string targetExplosionTrigger = "BombExplode";

    [Header("VFX")]
    [SerializeField] private GameObject warningVfxPrefab;
    [SerializeField] private GameObject explosionVfxPrefab;
    [SerializeField] private Vector3 warningVfxLocalOffset = Vector3.zero;
    [SerializeField] private float explosionVfxLifetime = 5f;
    [SerializeField] private Vector3 explosionVfxOffset = Vector3.zero;
    [SerializeField] private GameObject hitVfxPrefab;
    [SerializeField] private float hitVfxLifetime = 4f;
    [SerializeField] private Vector3 hitVfxLocalOffset = Vector3.zero;

    private KeywordRecognizer keywordRecognizer;

    private readonly Dictionary<EyeTargetable, Coroutine> activeBombs = new();
    private readonly Dictionary<EyeTargetable, GameObject> activeWarningVfx = new();

    private void Start()
    {
        keywordRecognizer = new KeywordRecognizer(new string[] { voiceCommand });
        keywordRecognizer.OnPhraseRecognized += OnPhraseRecognized;
        keywordRecognizer.Start();

        Debug.Log("Volcanic Bomb ready. Voice: " + voiceCommand);
    }

    private void Update()
    {
        if (CooldownRemaining > 0f)
        {
            CooldownRemaining -= Time.deltaTime;
        }

        if (Keyboard.current == null) return;
        if (keybind == Key.None) return;

        if (keybind != Key.None && Keyboard.current[keybind].wasPressedThisFrame)
        {
            CastVolcanicBomb();
        }
    }

    private void CastVolcanicBomb()
    {
        if (IsOnCooldown)
        {
            Debug.Log("Volcanic Bomb is on cooldown.");
            return;
        }

        if (targetingSystem == null)
        {
            Debug.LogWarning("VolcanicBomb is missing TargetingSystem reference.");
            return;
        }

        EyeTargetable target = targetingSystem.CurrentTarget;

        if (target == null || !target.CanBeTargeted)
        {
            Debug.Log("No valid target selected for Volcanic Bomb.");
            return;
        }

        if (characterAnimator != null && !string.IsNullOrWhiteSpace(castTrigger))
        {
            characterAnimator.SetTrigger(castTrigger);
        }

        if (activeBombs.TryGetValue(target, out Coroutine existingBomb))
        {
            StopCoroutine(existingBomb);
            activeBombs.Remove(target);
        }

        if (activeWarningVfx.TryGetValue(target, out GameObject existingWarningVfx))
        {
            if (existingWarningVfx != null)
            {
                Destroy(existingWarningVfx);
            }

            activeWarningVfx.Remove(target);
        }

        CooldownRemaining = cooldownDuration;

        activeBombs[target] = StartCoroutine(BombRoutine(target));

        Debug.Log("Volcanic Bomb placed on: " + target.name);
    }

    private IEnumerator BombRoutine(EyeTargetable target)
    {
        if (target == null) yield break;

        GameObject warningVfx = null;

        if (warningVfxPrefab != null)
        {
            warningVfx = Instantiate(warningVfxPrefab, target.transform);
            warningVfx.transform.localPosition = warningVfxLocalOffset;
            warningVfx.transform.localRotation = Quaternion.identity;

            activeWarningVfx[target] = warningVfx;
        }

        yield return new WaitForSeconds(delayBeforeExplosion);

        if (target == null || !target.CanBeTargeted)
        {
            CleanupTarget(target, warningVfx);
            yield break;
        }

        Animator targetAnimator = target.GetComponentInChildren<Animator>();

        if (targetAnimator != null && !string.IsNullOrWhiteSpace(targetExplosionTrigger))
        {
            targetAnimator.SetTrigger(targetExplosionTrigger);
        }

        Vector3 explosionOrigin =
            target.GetTargetPoint() + explosionVfxOffset;

        if (warningVfx != null)
        {
            Destroy(warningVfx);
        }

        activeWarningVfx.Remove(target);

        if (explosionVfxPrefab != null)
        {
            GameObject explosionVfx = Instantiate(
                explosionVfxPrefab,
                explosionOrigin,
                Quaternion.identity
            );

            Destroy(explosionVfx, explosionVfxLifetime);
        }

        DamageTargets(explosionOrigin);

        activeBombs.Remove(target);

        Debug.Log("Volcanic Bomb exploded.");
    }

    private void DamageTargets(Vector3 origin)
    {
        Collider[] hits = Physics.OverlapSphere(origin, explosionRadius);

        HashSet<NPCHealth> damagedTargets = new();

        foreach (Collider hit in hits)
        {
            NPCHealth npcHealth = hit.GetComponentInParent<NPCHealth>();

            if (npcHealth == null) continue;
            if (damagedTargets.Contains(npcHealth)) continue;

            damagedTargets.Add(npcHealth);

            npcHealth.TakeDamage(damage);

            SpawnHitVfx(npcHealth.transform);
        }
    }

    private void SpawnHitVfx(Transform target)
    {
        if (hitVfxPrefab == null) return;
        if (target == null) return;

        GameObject hitVfx = Instantiate(hitVfxPrefab, target);
        hitVfx.transform.localPosition = hitVfxLocalOffset;
        hitVfx.transform.localRotation = Quaternion.identity;

        Destroy(hitVfx, hitVfxLifetime);
    }

    private void CleanupTarget(EyeTargetable target, GameObject warningVfx)
    {
        if (warningVfx != null)
        {
            Destroy(warningVfx);
        }

        if (target != null)
        {
            activeWarningVfx.Remove(target);
            activeBombs.Remove(target);
        }
    }

    private void OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        if (args.text.ToLower() == voiceCommand.ToLower())
        {
            CastVolcanicBomb();
        }
    }

    private void OnDestroy()
    {
        foreach (GameObject warningVfx in activeWarningVfx.Values)
        {
            if (warningVfx != null)
            {
                Destroy(warningVfx);
            }
        }

        activeWarningVfx.Clear();
        activeBombs.Clear();

        if (keywordRecognizer == null) return;

        if (keywordRecognizer.IsRunning)
        {
            keywordRecognizer.Stop();
        }

        keywordRecognizer.Dispose();
    }
}