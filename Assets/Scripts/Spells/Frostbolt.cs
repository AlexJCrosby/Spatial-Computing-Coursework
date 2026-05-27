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
    [SerializeField] private int damage = 1;

    [Header("Setup")]
    [SerializeField] private GameObject frostboltPrefab;

    [Header("Casting")]
    [SerializeField] private float castDelay = 0.35f;
    [SerializeField] private float projectileSpeed = 18f;

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

        if (frostboltPrefab == null)
        {
            Debug.LogWarning("Frostbolt is missing frostbolt prefab.");
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

        GameObject frostbolt = Instantiate(
            frostboltPrefab,
            request.CastPoint.position,
            request.CastPoint.rotation
        );

        Projectile projectile =
            frostbolt.GetComponent<Projectile>();

        if (projectile == null)
        {
            projectile = frostbolt.AddComponent<Projectile>();
        }

        projectile.LaunchAt(request.Target.transform, projectileSpeed, damage);

        Debug.Log("Frostbolt fired at target: " + request.Target.name);
    }
}