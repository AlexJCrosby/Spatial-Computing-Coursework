using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    public static bool ShowEnemyHealthBars { get; private set; } = true;

    [Header("References")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private WaveManager waveManager;

    [Header("Health UI")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TMP_Text healthText;

    [Header("Wave UI")]
    [SerializeField] private TMP_Text waveText;

    [Header("Enemy Count UI")]
    [SerializeField] private bool showEnemiesRemaining = true;
    [SerializeField] private TMP_Text enemiesText;

    [Header("Enemy Health Bar Toggle")]
    [SerializeField] private bool showEnemyHealthBarsOnStart = true;
    [SerializeField] private Key enemyHealthBarToggleKey = Key.V;
    [SerializeField] private bool requireCtrlForHealthBarToggle = true;

    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text gameOverText;
    [SerializeField] private TMP_Text waveReachedText;
    [SerializeField] private Button playAgainButton;

    private bool gameOverShown;

    private void Start()
    {
        ShowEnemyHealthBars = showEnemyHealthBarsOnStart;
        RefreshAllEnemyHealthBars();

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
        HandleEnemyHealthBarToggle();

        UpdateHealthUI();
        UpdateWaveUI();
        UpdateGameOverUI();
    }

    private void HandleEnemyHealthBarToggle()
    {
        if (Keyboard.current == null) return;

        bool togglePressed =
            Keyboard.current[enemyHealthBarToggleKey].wasPressedThisFrame;

        if (!togglePressed) return;

        bool ctrlHeld =
            Keyboard.current.leftCtrlKey.isPressed ||
            Keyboard.current.rightCtrlKey.isPressed;

        if (requireCtrlForHealthBarToggle && !ctrlHeld) return;

        ShowEnemyHealthBars = !ShowEnemyHealthBars;
        RefreshAllEnemyHealthBars();

        Debug.Log("Enemy health bars visible: " + ShowEnemyHealthBars);
    }

    private void RefreshAllEnemyHealthBars()
    {
        EnemyHealthBar[] healthBars =
            FindObjectsByType<EnemyHealthBar>(FindObjectsSortMode.None);

        foreach (EnemyHealthBar healthBar in healthBars)
        {
            healthBar.Refresh();
        }
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
            healthText.text =
                "Health: " + playerHealth.CurrentHealth + " / " + playerHealth.MaxHealth;
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
                    "You survived until Wave " + waveManager.CurrentWave;
            }
        }
    }

    private void PlayAgain()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}