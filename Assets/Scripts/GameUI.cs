using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerHealth playerHealth;

    [Header("UI")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text waveText;
    [SerializeField] private TMP_Text enemiesText;

    private void Update()
    {
        if (playerHealth != null)
        {
            healthSlider.maxValue = playerHealth.MaxHealth;
            healthSlider.value = playerHealth.CurrentHealth;

            healthText.text = "Health: " + playerHealth.CurrentHealth + " / " + playerHealth.MaxHealth;
        }
    }
}