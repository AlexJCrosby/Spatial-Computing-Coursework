using UnityEngine;

public class NPCHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 3;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private float destroyDelayAfterDeath = 3f;

    private int currentHealth;
    private bool isDead;

    public bool IsDead => isDead;

    private void Awake()
    {
        currentHealth = maxHealth;

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;

        currentHealth -= damageAmount;

        Debug.Log(name + " took damage. Health: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            PlayHitAnimation();
        }
    }

    private void PlayHitAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("Hit");
        }
    }

    private void Die()
    {
        isDead = true;

        Debug.Log(name + " died.");

        if (animator != null)
        {
            animator.SetBool("Dead", true);
        }

        GoblinAI goblinAI = GetComponent<GoblinAI>();
        if (goblinAI != null)
        {
            goblinAI.enabled = false;
        }

        Destroy(gameObject, destroyDelayAfterDeath);
    }
}