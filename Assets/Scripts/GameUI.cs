using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private WaveManager waveManager;

    [Header("UI")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text waveText;
    [SerializeField] private TMP_Text enemiesText;
    [SerializeField] private GameObject gameOverPanel;

    private void Update()
    {
        if (playerHealth != null)
        {
            if (healthSlider != null)
            {
                healthSlider.maxValue = playerHealth.MaxHealth;
                healthSlider.value = playerHealth.CurrentHealth;
            }

            if (healthText != null)
            {
                healthText.text = "Health: " + playerHealth.CurrentHealth + " / " + playerHealth.MaxHealth;
            }

            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(playerHealth.CurrentHealth <= 0);
            }
        }

        if (waveManager != null)
        {
            if (waveText != null)
            {
                waveText.text = "Wave: " + waveManager.CurrentWave;
            }

            if (enemiesText != null)
            {
                enemiesText.text = "Enemies: " + waveManager.EnemiesRemaining;
            }
        }
    }
}