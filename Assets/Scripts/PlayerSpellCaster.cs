using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSpellCaster : MonoBehaviour
{
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private Transform spellCastPoint;

    private void Update()
    {
        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            CastFireball();
        }
    }

    private void CastFireball()
    {
        Instantiate(
            fireballPrefab,
            spellCastPoint.position,
            transform.rotation
        );
    }
}