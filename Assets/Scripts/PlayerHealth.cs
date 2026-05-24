using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 10;

    public int MaxHealth => maxHealth;
    public int CurrentHealth { get; private set; }

    private void Awake()
    {
        CurrentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        CurrentHealth -= amount;
        CurrentHealth = Mathf.Max(CurrentHealth, 0);

        Debug.Log("Player health: " + CurrentHealth);

        if (CurrentHealth <= 0)
        {
            Debug.Log("Player died / game over.");
        }
    }
}