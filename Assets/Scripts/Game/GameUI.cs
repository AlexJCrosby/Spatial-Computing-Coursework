using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    [Header("Spell References")]
    [SerializeField] private EyeTargetingFireballCaster fireballCaster;
    [SerializeField] private TelekinesisSpellCaster telekinesisCaster;
    [SerializeField] private AOEKnockbackSpellCaster knockbackCaster;
    [SerializeField] private EyeTargetingFrostboltCaster frostboltCaster;


    [Header("Action Bar Cooldown Overlays")]
    [SerializeField] private Image fireCooldownOverlay;
    [SerializeField] private Image levitateCooldownOverlay;
    [SerializeField] private Image knockCooldownOverlay;
    [SerializeField] private Image frostCooldownOverlay;

    [Header("References")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private WaveManager waveManager;

    [Header("UI")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text waveText;
    [SerializeField] private TMP_Text enemiesText;
    [SerializeField] private GameObject gameOverPanel;
    
    private void UpdateActionBarCooldowns()
    {
        if (fireballCaster != null && fireCooldownOverlay != null)
        {
            fireCooldownOverlay.fillAmount =
                fireballCaster.CooldownRemaining / fireballCaster.CooldownDuration;
        }

        if (telekinesisCaster != null && levitateCooldownOverlay != null)
        {
            levitateCooldownOverlay.fillAmount =
                telekinesisCaster.CooldownRemaining / telekinesisCaster.CooldownDuration;
        }

        if (knockbackCaster != null && knockCooldownOverlay != null)
        {
            knockCooldownOverlay.fillAmount =
                knockbackCaster.CooldownRemaining / knockbackCaster.CooldownDuration;
        }
        if (frostboltCaster != null && frostCooldownOverlay != null)
        {
            frostCooldownOverlay.fillAmount =
                frostboltCaster.CooldownRemaining / frostboltCaster.CooldownDuration;
        }
    }

    private void Update()
    {
        UpdateActionBarCooldowns();

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