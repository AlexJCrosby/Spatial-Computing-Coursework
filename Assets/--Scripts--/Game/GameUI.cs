using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private WaveManager waveManager;

    [Header("Health UI")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TMP_Text healthText;

    [Header("Wave UI")]
    [SerializeField] private TMP_Text waveText;

    [Header("Enemy Count UI")]
    [SerializeField] private bool showEnemiesRemaining = false;
    [SerializeField] private TMP_Text enemiesText;

    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text gameOverText;
    [SerializeField] private Button playAgainButton;
    [SerializeField] private TMP_Text waveReachedText;

    private bool gameOverShown;

    private void Start()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (playAgainButton != null)
        {
            playAgainButton.onClick.AddListener(PlayAgain);
        }
    }

    private void Update()
    {
        UpdateHealthUI();
        UpdateWaveUI();
        UpdateGameOverUI();
    }

    private void UpdateHealthUI()
    {
        if (playerHealth == null) return;

        if (healthSlider != null)
        {
            healthSlider.maxValue = playerHealth.MaxHealth;
            healthSlider.value = playerHealth.CurrentHealth;
        }

        if (healthText != null)
        {
            healthText.text = "Health: " + playerHealth.CurrentHealth + " / " + playerHealth.MaxHealth;
        }
    }

    private void UpdateWaveUI()
    {
        if (waveManager == null) return;

        if (waveText != null)
        {
            waveText.text = "Wave: " + waveManager.CurrentWave;
        }

        if (enemiesText != null)
        {
            enemiesText.gameObject.SetActive(showEnemiesRemaining);

            if (showEnemiesRemaining)
            {
                enemiesText.text = "Enemies: " + waveManager.EnemiesRemaining;
            }
        }
    }

    private void UpdateGameOverUI()
    {
        if (playerHealth == null) return;
        if (gameOverShown) return;

        if (playerHealth.CurrentHealth <= 0)
        {
            gameOverShown = true;

            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
            }

            if (gameOverText != null)
            {
                gameOverText.text = "Game Over";
            }

            if (waveReachedText != null && waveManager != null)
            {
                waveReachedText.text =
                    "You survived until Wave " + waveManager.CurrentWave + "!";
            }
        }
    }

    private void PlayAgain()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}