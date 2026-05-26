using UnityEngine;

public class SimpleFireballProjectile : MonoBehaviour
{
    private Transform target;
    private Vector3 finalTargetPosition;
    private float speed;
    private bool launched;
    private bool targetLost;

    public void LaunchAt(Transform targetTransform, float projectileSpeed)
    {
        target = targetTransform;
        speed = projectileSpeed;
        launched = true;
        targetLost = false;

        finalTargetPosition = GetTargetPoint();

        Destroy(gameObject, 6f);
    }

    private void Update()
    {
        if (!launched) return;

        if (target != null && !targetLost)
        {
            NPCHealth npcHealth = target.GetComponentInParent<NPCHealth>();

            if (npcHealth != null && npcHealth.IsDead)
            {
                targetLost = true;
            }
            else
            {
                finalTargetPosition = GetTargetPoint();
            }
        }
        else
        {
            targetLost = true;
        }

        Vector3 direction = finalTargetPosition - transform.position;

        if (direction.magnitude < 0.1f)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 moveDirection = direction.normalized;

        transform.position += moveDirection * speed * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(moveDirection);
    }

    private Vector3 GetTargetPoint()
    {
        if (target == null)
        {
            return finalTargetPosition;
        }

        EyeTargetable eyeTargetable = target.GetComponent<EyeTargetable>();

        if (eyeTargetable != null)
        {
            return eyeTargetable.GetTargetPoint();
        }

        Collider targetCollider = target.GetComponentInChildren<Collider>();

        if (targetCollider != null)
        {
            return targetCollider.bounds.center;
        }

        return target.position + Vector3.up * 1.2f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<PlayerMovement>() != null)
        {
            return;
        }

        NPCHealth npcHealth = other.GetComponentInParent<NPCHealth>();

        if (npcHealth != null && !npcHealth.IsDead)
        {
            npcHealth.TakeDamage(1);
        }

        Debug.Log("Projectile hit: " + other.name);

        Destroy(gameObject);
    }
}