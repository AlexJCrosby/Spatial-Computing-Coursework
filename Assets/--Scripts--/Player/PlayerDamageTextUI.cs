using System.Collections;
using TMPro;
using UnityEngine;

public class PlayerDamageTextUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private TMP_Text damageText;

    [Header("Text")]
    [SerializeField] private string prefix = "-";
    [SerializeField] private float lifetime = 0.8f;

    [Header("Motion")]
    [SerializeField] private Vector2 startAnchoredPosition = new Vector2(0f, 80f);
    [SerializeField] private Vector2 endAnchoredPosition = new Vector2(0f, 140f);

    private RectTransform rectTransform;
    private Coroutine activeRoutine;

    private void Awake()
    {
        rectTransform = damageText != null ? damageText.GetComponent<RectTransform>() : null;

        if (damageText != null)
        {
            damageText.gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnDamaged += ShowDamageText;
        }
    }

    private void OnDisable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnDamaged -= ShowDamageText;
        }
    }

    private void ShowDamageText(int damageAmount)
    {
        if (damageText == null) return;
        if (damageAmount <= 0) return;

        if (activeRoutine != null)
        {
            StopCoroutine(activeRoutine);
        }

        activeRoutine = StartCoroutine(DamageTextRoutine(damageAmount));
    }

    private IEnumerator DamageTextRoutine(int damageAmount)
    {
        damageText.gameObject.SetActive(true);
        damageText.text = prefix + damageAmount;

        float elapsed = 0f;

        Color startColor = damageText.color;
        startColor.a = 1f;
        damageText.color = startColor;

        while (elapsed < lifetime)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / lifetime;

            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = Vector2.Lerp(
                    startAnchoredPosition,
                    endAnchoredPosition,
                    t
                );
            }

            Color color = damageText.color;
            color.a = Mathf.Lerp(1f, 0f, t);
            damageText.color = color;

            yield return null;
        }

        damageText.gameObject.SetActive(false);
    }
}