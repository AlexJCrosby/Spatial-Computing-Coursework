using UnityEngine;

public class NPCDamageTextSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NPCHealth npcHealth;
    [SerializeField] private FloatingCombatText combatTextPrefab;

    [Header("Positioning")]
    [SerializeField] private Vector3 spawnOffset = new Vector3(0f, 2.6f, 0f);
    [SerializeField] private float randomHorizontalOffset = 0.25f;

    private void Awake()
    {
        if (npcHealth == null)
        {
            npcHealth = GetComponent<NPCHealth>();
        }
    }

    private void OnEnable()
    {
        if (npcHealth != null)
        {
            npcHealth.OnDamaged += SpawnDamageText;
        }
    }

    private void OnDisable()
    {
        if (npcHealth != null)
        {
            npcHealth.OnDamaged -= SpawnDamageText;
        }
    }

    private void SpawnDamageText(int damageAmount)
    {
        if (combatTextPrefab == null) return;
        if (damageAmount <= 0) return;

        Vector3 randomOffset = new Vector3(
            Random.Range(-randomHorizontalOffset, randomHorizontalOffset),
            0f,
            Random.Range(-randomHorizontalOffset, randomHorizontalOffset)
        );

        FloatingCombatText textInstance = Instantiate(
            combatTextPrefab,
            transform.position + spawnOffset + randomOffset,
            Quaternion.identity
        );

        textInstance.SetText(damageAmount.ToString());
    }
}