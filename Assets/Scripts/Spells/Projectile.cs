using UnityEngine;

namespace ZakhanSpellsPack
{
    public class Projectile : MonoBehaviour
    {
        public GameObject ExplosionPrefab;
        public float DestroyExplosion = 4.0f;
        public float DestroyChildren = 2.0f;
        public Vector2 Velocity;
        private bool hasHit;

        Rigidbody rb;
        void Start()
        {
            rb = gameObject.GetComponent<Rigidbody>();
            rb.linearVelocity = Velocity;

        }

        void OnCollisionEnter(Collision col)
        {
            if (hasHit) return;
            hasHit = true;

            if (ExplosionPrefab != null)
            {
                GameObject exp = Instantiate(
                    ExplosionPrefab,
                    transform.position,
                    ExplosionPrefab.transform.rotation
                );

                Destroy(exp, DestroyExplosion);
            }

            if (transform.childCount > 0)
            {
                Transform child = transform.GetChild(0);
                child.SetParent(null);
                Destroy(child.gameObject, DestroyChildren);
            }

            Destroy(gameObject);
        }
    }
}
