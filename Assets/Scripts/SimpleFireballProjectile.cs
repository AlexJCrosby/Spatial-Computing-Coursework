using UnityEngine;

public class SimpleFireballProjectile : MonoBehaviour
{
    private Transform target;
    private float speed;
    private bool launched;

    public void LaunchAt(Transform targetTransform, float projectileSpeed)
    {
        target = targetTransform;
        speed = projectileSpeed;
        launched = true;

        Destroy(gameObject, 6f);
    }

    private void Update()
    {
        if (!launched || target == null) return;

        Vector3 targetPosition = target.position;
        Vector3 direction = (targetPosition - transform.position).normalized;

        transform.position += direction * speed * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(direction);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<PlayerMovement>() != null)
        {
            return;
        }

        NPCHealth npcHealth = other.GetComponentInParent<NPCHealth>();

        if (npcHealth != null)
        {
            npcHealth.TakeDamage(1);
        }

        Debug.Log("Fireball hit: " + other.name);

        Destroy(gameObject);
    }
}