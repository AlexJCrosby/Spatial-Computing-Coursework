using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSpellCaster : MonoBehaviour
{
    [Header("Fireball Setup")]
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private Transform spellCastPoint;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private BeamAimProvider aimProvider;

    [Header("Aiming")]
    [SerializeField] private LineRenderer aimLine;
    [SerializeField] private float aimDepth = 10f;
    [SerializeField] private float heldFireballFollowSpeed = 12f;

    private GameObject activeFireball;
    private Vector3 currentAimWorldPosition;
    private bool isPreparingFireball;

    private void Start()
    {
        if (aimLine != null)
        {
            aimLine.positionCount = 2;
            aimLine.enabled = false;
        }
    }

    private void Update()
    {
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            TriggerFireball();
        }

        if (isPreparingFireball)
        {
            UpdateAimTarget();
            UpdateHeldFireballPosition();
            UpdateAimLine();
        }
    }

    public void TriggerFireball()
    {
        if (!isPreparingFireball)
        {
            SummonFireball();
        }
        else
        {
            LaunchFireball();
        }
    }

    private void SummonFireball()
    {
        if (fireballPrefab == null || spellCastPoint == null || playerCamera == null)
        {
            Debug.LogWarning("PlayerSpellCaster is missing required references.");
            return;
        }

        activeFireball = Instantiate(
            fireballPrefab,
            spellCastPoint.position,
            spellCastPoint.rotation
        );

        if (aimLine != null)
        {
            aimLine.enabled = true;
        }

        isPreparingFireball = true;

        Debug.Log("Fireball prepared. Aim with eyes, press R again to launch.");
    }

    private void UpdateAimTarget()
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

        Vector3 screenPosition = new Vector3(
            aimScreenPosition.x,
            aimScreenPosition.y,
            aimDepth
        );

        currentAimWorldPosition = playerCamera.ScreenToWorldPoint(screenPosition);
    }

    private void UpdateHeldFireballPosition()
    {
        if (activeFireball == null) return;

        activeFireball.transform.position = Vector3.Lerp(
            activeFireball.transform.position,
            spellCastPoint.position,
            heldFireballFollowSpeed * Time.deltaTime
        );
    }

    private void UpdateAimLine()
    {
        if (aimLine == null || activeFireball == null) return;

        aimLine.SetPosition(0, activeFireball.transform.position);
        aimLine.SetPosition(1, currentAimWorldPosition);
    }

    private void LaunchFireball()
    {
        if (activeFireball == null)
        {
            isPreparingFireball = false;
            return;
        }

        Vector3 launchDirection = currentAimWorldPosition - activeFireball.transform.position;

        FireballProjectile projectile = activeFireball.GetComponent<FireballProjectile>();

        if (projectile != null)
        {
            projectile.Launch(launchDirection);
        }
        else
        {
            Debug.LogWarning("Fireball prefab is missing FireballProjectile script.");
        }

        if (aimLine != null)
        {
            aimLine.enabled = false;
        }

        activeFireball = null;
        isPreparingFireball = false;

        Debug.Log("Fireball launched.");
    }
}