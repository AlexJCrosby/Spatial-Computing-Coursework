using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;

    public Vector3 offset = new Vector3(0, 3, -6);
    public float followSmoothness = 10f;
    public float lookHeight = 1.5f;
    public float mouseCameraSpeed = 0.15f;


    private PlayerInputActions inputActions;
    private Vector2 lookInput;

    private bool leftClickHeld;
    private bool rightClickHeld;

    private float cameraYawOffset = 0f;
    private float cameraPitchOffset = 1f;

    [SerializeField] private float minPitch = -10f;
    [SerializeField] private float maxPitch = 45f;
    [SerializeField] private float mouseVerticalSpeed = 0.1f;

    private void Awake()
    {
        inputActions = new PlayerInputActions();

        inputActions.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Look.canceled += ctx => lookInput = Vector2.zero;

        inputActions.Player.LeftClick.performed += ctx => leftClickHeld = true;
        inputActions.Player.LeftClick.canceled += ctx => leftClickHeld = false;

        inputActions.Player.RightClick.performed += ctx => rightClickHeld = true;
        inputActions.Player.RightClick.canceled += ctx => rightClickHeld = false;
    }

    private void OnEnable()
    {
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        if (leftClickHeld && !rightClickHeld)
        {
            cameraYawOffset += lookInput.x * mouseCameraSpeed;
        }

        if (leftClickHeld || rightClickHeld)
        {
            cameraPitchOffset -= lookInput.y * mouseVerticalSpeed;

            cameraPitchOffset = Mathf.Clamp(
                cameraPitchOffset,
                minPitch,
                maxPitch
            );
        }

        if (rightClickHeld)
        {
            cameraYawOffset = 0f;
        }

        if (rightClickHeld)
        {
            cameraYawOffset = 0f;
        }

        Quaternion cameraRotation =
            target.rotation *
            Quaternion.Euler(cameraPitchOffset, cameraYawOffset, 0); 

        Vector3 desiredPosition = target.position + cameraRotation * offset;

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            followSmoothness * Time.deltaTime
        );

        transform.LookAt(target.position + Vector3.up * lookHeight);
    }
}