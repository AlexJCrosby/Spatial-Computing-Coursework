using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSpellCaster : MonoBehaviour
{
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private Transform spellCastPoint;

    private void Update()
    {
        // Keyboard fallback for testing
        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            CastFireball();
        }
    }

    public void CastFireball()
    {
        if (fireballPrefab == null)
        {
            Debug.LogWarning("Fireball prefab is not assigned.");
            return;
        }

        if (spellCastPoint == null)
        {
            Debug.LogWarning("Spell cast point is not assigned.");
            return;
        }

        Instantiate(
            fireballPrefab,
            spellCastPoint.position,
            spellCastPoint.rotation
        );

        Debug.Log("Fireball cast.");
    }
}