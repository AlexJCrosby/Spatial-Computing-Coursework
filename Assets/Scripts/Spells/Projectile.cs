using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float lifetime = 5f;

    [Header("Impact VFX")]
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private float destroyExplosionAfter = 4f;
    [SerializeField] private float destroyDetachedVisualAfter = 2f;

    [SerializeField] private bool homeToTarget = true;
    [SerializeField] private float turnSpeed = 12f;
    
    private float currentSpeed;
    private Rigidbody rb;
    private Transform target;
    private int damage;
    private bool hasHit;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (!homeToTarget) return;
        if (hasHit) return;
        if (target == null) return;
        if (rb == null) return;

        Vector3 targetPoint = GetTargetPoint(target);
        Vector3 desiredDirection = (targetPoint - transform.position).normalized;

        if (desiredDirection == Vector3.zero) return;

        Vector3 newDirection = Vector3.Slerp(
            transform.forward,
            desiredDirection,
            turnSpeed * Time.fixedDeltaTime
        );

        transform.rotation = Quaternion.LookRotation(newDirection);
        rb.linearVelocity = transform.forward * currentSpeed;
    }

    public void LaunchAt(Transform newTarget, float speed, int newDamage)
    {
        target = newTarget;
        damage = newDamage;
        currentSpeed = speed;

        Vector3 direction = GetLaunchDirection();

        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }

        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            rb.linearVelocity = transform.forward * speed;
        }

        Debug.Log("Projectile launched at " + newTarget.name + " direction " + direction);

        Destroy(gameObject, lifetime);
    }

    private Vector3 GetLaunchDirection()
    {
        if (target == null)
        {
            return transform.forward;
        }

        Vector3 targetPoint = GetTargetPoint(target);
        Vector3 direction = targetPoint - transform.position;

        if (direction == Vector3.zero)
        {
            return transform.forward;
        }

        return direction.normalized;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasHit) return;

        NPCHealth npcHealth = collision.collider.GetComponentInParent<NPCHealth>();

        Hit(npcHealth);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;

        NPCHealth npcHealth = other.GetComponentInParent<NPCHealth>();

        Hit(npcHealth);
    }

    private void Hit(NPCHealth npcHealth)
    {
        hasHit = true;

        if (npcHealth != null)
        {
            npcHealth.TakeDamage(damage);
        }

        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(
                explosionPrefab,
                transform.position,
                explosionPrefab.transform.rotation
            );

            Destroy(explosion, destroyExplosionAfter);
        }

        if (transform.childCount > 0)
        {
            Transform visualChild = transform.GetChild(0);
            visualChild.SetParent(null);
            Destroy(visualChild.gameObject, destroyDetachedVisualAfter);
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