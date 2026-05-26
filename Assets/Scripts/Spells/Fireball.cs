using UnityEngine;
using System.Collections;

public class Fireball : MonoBehaviour
{
    [Header("Cooldown")]
    [SerializeField] private float cooldownDuration = 2f;

    public float CooldownDuration => cooldownDuration;
    public float CooldownRemaining { get; private set; }
    public bool IsOnCooldown => CooldownRemaining > 0f;

    [Header("Setup")]
    [SerializeField] private GameObject fireballPrefab;

    [Header("Casting")]
    [SerializeField] private float castDelay = 0.35f;
    [SerializeField] private float projectileSpeed = 14f;

    private void Update()
    {
        if (CooldownRemaining > 0f)
        {
            CooldownRemaining -= Time.deltaTime;
        }
    }

    public void Cast(SpellCastRequest request)
    {
        if (IsOnCooldown)
        {
            Debug.Log("Fireball is on cooldown.");
            return;
        }

        if (!request.HasValidTarget)
        {
            Debug.Log("No valid target selected for fireball.");
            return;
        }

        if (fireballPrefab == null)
        {
            Debug.LogWarning("Fireball is missing fireball prefab.");
            return;
        }

        if (request.CastPoint == null)
        {
            Debug.LogWarning("Fireball cast request has no cast point.");
            return;
        }

        CooldownRemaining = cooldownDuration;

        StartCoroutine(SpawnFireballAfterDelay(request));
    }

    private IEnumerator SpawnFireballAfterDelay(SpellCastRequest request)
    {
        yield return new WaitForSeconds(castDelay);

        if (!request.HasValidTarget || request.CastPoint == null)
        {
            yield break;
        }

        GameObject fireball = Instantiate(
            fireballPrefab,
            request.CastPoint.position,
            request.CastPoint.rotation
        );

        SimpleFireballProjectile projectile =
            fireball.GetComponent<SimpleFireballProjectile>();

        if (projectile == null)
        {
            projectile = fireball.AddComponent<SimpleFireballProjectile>();
        }

        projectile.LaunchAt(request.Target.transform, projectileSpeed);

        Debug.Log("Fireball fired at target: " + request.Target.name);
    }
}