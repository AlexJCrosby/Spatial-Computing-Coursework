using UnityEngine;

public class SpellProjectile: MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 14f;
    [SerializeField] private float lifetime = 5f;

    [Header("Impact VFX")]
    [SerializeField] private GameObject impactVfxPrefab;
    [SerializeField] private float impactVfxLifetime = 4f;

    private Transform target;
    private bool hasHit;
    private int damage;

    public void LaunchAt(Transform newTarget, float newSpeed, int newDamage)
    {
        target = newTarget;
        speed = newSpeed;
        damage = newDamage;

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (hasHit) return;

        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 targetPoint = GetTargetPoint(target);
        Vector3 direction = (targetPoint - transform.position).normalized;

        transform.position += direction * speed * Time.deltaTime;

        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;

        NPCHealth npcHealth = other.GetComponentInParent<NPCHealth>();

        if (npcHealth == null) return;

        Hit(npcHealth);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasHit) return;

        NPCHealth npcHealth = collision.collider.GetComponentInParent<NPCHealth>();

        if (npcHealth == null) return;

        Hit(npcHealth);
    }

    private void Hit(NPCHealth npcHealth)
    {
        hasHit = true;

        npcHealth.TakeDamage(damage);

        if (impactVfxPrefab != null)
        {
            GameObject impact = Instantiate(
                impactVfxPrefab,
                transform.position,
                Quaternion.identity
            );

            Destroy(impact, impactVfxLifetime);
        }

        Destroy(gameObject);
    }

    private Vector3 GetTargetPoint(Transform targetTransform)
    {
        EyeTargetable targetable = targetTransform.GetComponentInParent<EyeTargetable>();

        if (targetable != null)
        {
            return targetable.GetTargetPoint();
        }

        return targetTransform.position;
    }
}