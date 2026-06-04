using UnityEngine;
using System;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 10;
    [SerializeField] private Animator characterAnimator;

    public int MaxHealth => maxHealth;
    public int CurrentHealth { get; private set; }
    public event Action<int> OnDamaged;

    private bool isDead;

    private void Awake()
    {
        CurrentHealth = maxHealth;

        if (characterAnimator == null)
        {
            characterAnimator = GetComponentInChildren<Animator>();
        }
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        CurrentHealth -= amount;
        CurrentHealth = Mathf.Max(CurrentHealth, 0);
        OnDamaged?.Invoke(amount);

        Debug.Log("Player health: " + CurrentHealth);

        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;

        Debug.Log("Player died.");

        if (characterAnimator != null)
        {
            characterAnimator.SetBool("Dead", true);
        }

        DisableIfPresent<PlayerMovement>();
        DisableIfPresent<PlayerSpellCaster>();
        DisableIfPresent<Fireball>();
        DisableIfPresent<Frostbolt>();
        DisableIfPresent<TelekinesisSpellCaster>();
        DisableIfPresent<AOEKnockback>();
    }

    private void DisableIfPresent<T>() where T : MonoBehaviour
    {
        T component = GetComponent<T>();

        if (component != null)
        {
            component.enabled = false;
        }
    }
}