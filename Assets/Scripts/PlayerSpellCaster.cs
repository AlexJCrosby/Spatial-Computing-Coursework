using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSpellCaster : MonoBehaviour
{
    [Header("Fireball Setup")]
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private Transform spellCastPoint;
    [SerializeField] private Camera playerCamera;

    [Header("Held Fireball Settings")]
    [SerializeField] private float minDepth = 1.5f;
    [SerializeField] private float maxDepth = 12f;
    [SerializeField] private float depthMoveSpeed = 4f;

    private HeldFireball activeFireball;
    private float currentDepth = 3f;

    private void Update()
    {
        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            CastFireball();
        }

        if (activeFireball != null)
        {
            UpdateHeldFireball();
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

        if (playerCamera == null)
        {
            Debug.LogWarning("Player camera is not assigned.");
            return;
        }

        if (activeFireball != null)
        {
            Destroy(activeFireball.gameObject);
        }

        GameObject fireballObject = Instantiate(
            fireballPrefab,
            spellCastPoint.position,
            spellCastPoint.rotation
        );

        activeFireball = fireballObject.GetComponent<HeldFireball>();

        if (activeFireball == null)
        {
            Debug.LogWarning("Fireball prefab needs a HeldFireball script.");
        }

        currentDepth = 3f;

        Debug.Log("Summoned fireball.");
    }

    private void UpdateHeldFireball()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        if (Mouse.current.leftButton.isPressed)
        {
            currentDepth += depthMoveSpeed * Time.deltaTime;
        }

        if (Mouse.current.rightButton.isPressed)
        {
            currentDepth -= depthMoveSpeed * Time.deltaTime;
        }

        currentDepth = Mathf.Clamp(currentDepth, minDepth, maxDepth);

        Vector3 screenPosition = new Vector3(
            mousePosition.x,
            mousePosition.y,
            currentDepth
        );

        Vector3 worldPosition = playerCamera.ScreenToWorldPoint(screenPosition);

        activeFireball.MoveTo(worldPosition);
    }
}