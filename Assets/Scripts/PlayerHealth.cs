using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 10;
    [SerializeField] private Animator characterAnimator;

    public int MaxHealth => maxHealth;
    public int CurrentHealth { get; private set; }

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

        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.enabled = false;
        }

        PlayerSpellCaster spellCaster = GetComponent<PlayerSpellCaster>();
        if (spellCaster != null)
        {
            spellCaster.enabled = false;
        }

        EyeTargetingFireballCaster eyeFireball = GetComponent<EyeTargetingFireballCaster>();
        if (eyeFireball != null)
        {
            eyeFireball.enabled = false;
        }

        TelekinesisSpellCaster telekinesis = GetComponent<TelekinesisSpellCaster>();
        if (telekinesis != null)
        {
            telekinesis.enabled = false;
        }

        AOEKnockbackSpellCaster knockback = GetComponent<AOEKnockbackSpellCaster>();
        if (knockback != null)
        {
            knockback.enabled = false;
        }
    }
}