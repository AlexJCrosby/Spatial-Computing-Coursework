using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSpellCaster : MonoBehaviour
{
    [Header("Fireball Setup")]
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private Transform spellCastPoint;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private BeamAimProvider aimProvider;

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
        if (fireballPrefab == null || spellCastPoint == null || playerCamera == null)
        {
            Debug.LogWarning("PlayerSpellCaster is missing required references.");
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
        currentDepth = 3f;

        Debug.Log("Summoned fireball.");
    }

    private void UpdateHeldFireball()
    {
        Vector2 aimScreenPosition;

        if (aimProvider != null)
        {
            aimScreenPosition = aimProvider.GetAimScreenPosition();
        }
        else
        {
            aimScreenPosition = Mouse.current.position.ReadValue();
        }

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
            aimScreenPosition.x,
            aimScreenPosition.y,
            currentDepth
        );

        Vector3 worldPosition = playerCamera.ScreenToWorldPoint(screenPosition);

        activeFireball.MoveTo(worldPosition);
    }
}