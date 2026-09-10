using System;
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

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public bool IsDead => isDead;

    public event Action<int, int, int> OnHealthChanged;
    public event Action<int> OnDamaged;
    public event Action OnDied;

    private void Awake()
    {
        currentHealth = maxHealth;

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        OnHealthChanged?.Invoke(currentHealth, maxHealth, 0);
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;

        int previousHealth = currentHealth;

        currentHealth -= damageAmount;
        currentHealth = Mathf.Max(currentHealth, 0);

        int actualDamageTaken = previousHealth - currentHealth;

        Debug.Log(name + " took damage. Health: " + currentHealth);

        OnHealthChanged?.Invoke(currentHealth, maxHealth, actualDamageTaken);
        OnDamaged?.Invoke(actualDamageTaken);

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
        if (isDead) return;

        isDead = true;

        Debug.Log(name + " died.");

        OnDied?.Invoke();

        GoblinAI goblinAI = GetComponent<GoblinAI>();
        if (goblinAI != null)
        {
            goblinAI.enabled = false;
        }

        CharacterController controller = GetComponent<CharacterController>();
        if (controller != null)
        {
            controller.enabled = false;
        }

        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
            animator.SetInteger("AttackIndex", 0);
            animator.SetBool("Dead", true);
        }

        Destroy(gameObject, destroyDelayAfterDeath);
    }
}