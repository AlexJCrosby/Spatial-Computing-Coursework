using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NPCHealth npcHealth;
    [SerializeField] private Canvas healthBarCanvas;
    [SerializeField] private Slider healthSlider;

    [Header("Positioning")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 2.2f, 0f);
    [SerializeField] private bool followTarget = true;

    [Header("Visibility")]
    [SerializeField] private bool hideWhenFullHealth = false;
    [SerializeField] private bool hideWhenDead = true;

    [Header("Facing")]
    [SerializeField] private bool faceCamera = true;

    private Camera mainCamera;
    private Transform targetRoot;

    private void Awake()
    {
        if (npcHealth == null)
        {
            npcHealth = GetComponentInParent<NPCHealth>();
        }

        if (healthBarCanvas == null)
        {
            healthBarCanvas = GetComponentInChildren<Canvas>();
        }

        targetRoot = npcHealth != null ? npcHealth.transform : transform.root;
        mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        if (npcHealth != null)
        {
            npcHealth.OnHealthChanged += HandleHealthChanged;
            npcHealth.OnDied += HandleDied;
        }

        Refresh();
    }

    private void OnDisable()
    {
        if (npcHealth != null)
        {
            npcHealth.OnHealthChanged -= HandleHealthChanged;
            npcHealth.OnDied -= HandleDied;
        }
    }

    private void LateUpdate()
    {
        if (followTarget && targetRoot != null)
        {
            transform.position = targetRoot.position + worldOffset;
        }

        if (faceCamera && mainCamera != null)
        {
            transform.rotation = mainCamera.transform.rotation;
        }
    }

    private void HandleHealthChanged(int currentHealth, int maxHealth, int damageTaken)
    {
        Refresh();
    }

    private void HandleDied()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (npcHealth == null || healthSlider == null) return;

        healthSlider.minValue = 0;
        healthSlider.maxValue = npcHealth.MaxHealth;
        healthSlider.value = npcHealth.CurrentHealth;

        bool shouldShow = GameUI.ShowEnemyHealthBars;

        if (hideWhenFullHealth && npcHealth.CurrentHealth >= npcHealth.MaxHealth)
        {
            shouldShow = false;
        }

        if (hideWhenDead && npcHealth.IsDead)
        {
            shouldShow = false;
        }

        if (healthBarCanvas != null)
        {
            healthBarCanvas.gameObject.SetActive(shouldShow);
        }
    }
}