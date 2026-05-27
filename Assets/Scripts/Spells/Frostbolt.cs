using UnityEngine;
using System.Collections;

public class Frostbolt : MonoBehaviour
{
    [Header("Cooldown")]
    [SerializeField] private float cooldownDuration = 2f;

    public float CooldownDuration => cooldownDuration;
    public float CooldownRemaining { get; private set; }
    public bool IsOnCooldown => CooldownRemaining > 0f;

    [Header("Damage")]
    [SerializeField] private int keyboardDamage = 1;
    [SerializeField] private int voiceVolleyDamage = 1;

    [Header("Projectile Prefabs")]
    [SerializeField] private GameObject keyboardProjectilePrefab;
    [SerializeField] private GameObject voiceVolleyProjectilePrefab;

    [Header("Casting")]
    [SerializeField] private float castDelay = 0.35f;
    [SerializeField] private float keyboardProjectileSpeed = 18f;
    [SerializeField] private float voiceVolleyProjectileSpeed = 18f;

    private void Update()
    {
        if (CooldownRemaining > 0f)
        {
            CooldownRemaining -= Time.deltaTime;
        }
    }

    public void Cast(SpellCastRequest request)
    {
        if (IsOnCooldown && !request.BypassCooldown)
        {
            Debug.Log("Frostbolt is on cooldown.");
            return;
        }

        if (!request.HasValidTarget)
        {
            Debug.Log("No valid target selected for frostbolt.");
            return;
        }

        if (request.CastPoint == null)
        {
            Debug.LogWarning("Frostbolt cast request has no cast point.");
            return;
        }

        if (!request.BypassCooldown)
        {
            CooldownRemaining = cooldownDuration;
        }

        StartCoroutine(SpawnFrostboltAfterDelay(request));
    }

    private IEnumerator SpawnFrostboltAfterDelay(SpellCastRequest request)
    {
        yield return new WaitForSeconds(castDelay);

        if (!request.HasValidTarget || request.CastPoint == null)
        {
            yield break;
        }

        GameObject prefab = GetPrefabForRequest(request);

        if (prefab == null)
        {
            Debug.LogWarning("Frostbolt is missing projectile prefab.");
            yield break;
        }

        GameObject frostbolt = Instantiate(
            prefab,
            request.CastPoint.position,
            request.CastPoint.rotation
        );

        Projectile projectile = frostbolt.GetComponent<Projectile>();

        if (projectile == null)
        {
            projectile = frostbolt.AddComponent<Projectile>();
        }

        projectile.LaunchAt(
            request.Target.transform,
            GetSpeedForRequest(request),
            GetDamageForRequest(request)
        );

        Debug.Log("Frostbolt fired at target: " + request.Target.name);
    }

    private GameObject GetPrefabForRequest(SpellCastRequest request)
    {
        if (request.InputSource == SpellInputSource.Voice && voiceVolleyProjectilePrefab != null)
        {
            return voiceVolleyProjectilePrefab;
        }

        return keyboardProjectilePrefab;
    }

    private float GetSpeedForRequest(SpellCastRequest request)
    {
        return request.InputSource == SpellInputSource.Voice
            ? voiceVolleyProjectileSpeed
            : keyboardProjectileSpeed;
    }

    private int GetDamageForRequest(SpellCastRequest request)
    {
        return request.InputSource == SpellInputSource.Voice
            ? voiceVolleyDamage
            : keyboardDamage;
    }
}