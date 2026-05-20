using UnityEngine;

public class FireballProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 12f;
    [SerializeField] private float lifetime = 5f;

    private Vector3 direction;
    private bool launched;

    public void Launch(Vector3 launchDirection)
    {
        direction = launchDirection.normalized;
        launched = true;

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (!launched) return;

        transform.position += direction * speed * Time.deltaTime;
    }
}