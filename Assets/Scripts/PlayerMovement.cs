using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController controller;
    private PlayerInputActions inputActions;

    [Header("Movement")]
    public float forwardSpeed = 5f;
    public float backwardSpeed = 2.5f;
    public float strafeSpeed = 5f;
    public float turnSpeed = 140f;

    [Header("Jumping")]
    public float jumpHeight = 1.5f;
    public float gravity = -9.81f;

    private float forwardBackInput;
    private float turnInput;
    private float strafeInput;

    private Vector3 verticalVelocity;
    private Vector3 lastGroundedHorizontalMove;
    private Vector3 lockedAirHorizontalMove;

    private bool jumpRequested;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        inputActions = new PlayerInputActions();

        inputActions.Player.ForwardBack.performed += ctx => forwardBackInput = ctx.ReadValue<float>();
        inputActions.Player.ForwardBack.canceled += ctx => forwardBackInput = 0f;

        inputActions.Player.Turn.performed += ctx => turnInput = ctx.ReadValue<float>();
        inputActions.Player.Turn.canceled += ctx => turnInput = 0f;

        inputActions.Player.Strafe.performed += ctx => strafeInput = ctx.ReadValue<float>();
        inputActions.Player.Strafe.canceled += ctx => strafeInput = 0f;

        inputActions.Player.Jump.performed += ctx => jumpRequested = true;
    }

    private void OnEnable()
    {
        inputActions.Enable();
    }

    private void Update()
    {
        bool isGrounded = controller.isGrounded;

        // Allow turning both on the ground and in mid-air.
        // This changes facing direction, but not the locked jump trajectory.
        transform.Rotate(Vector3.up * turnInput * turnSpeed * Time.deltaTime);

        Vector3 horizontalMove;

        if (isGrounded)
        {
            horizontalMove = CalculateGroundMovement();
            lastGroundedHorizontalMove = horizontalMove;

            if (verticalVelocity.y < 0)
            {
                verticalVelocity.y = -2f;
            }

            if (jumpRequested)
            {
                verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                lockedAirHorizontalMove = lastGroundedHorizontalMove;
            }
        }
        else
        {
            horizontalMove = lockedAirHorizontalMove;
        }

        verticalVelocity.y += gravity * Time.deltaTime;

        Vector3 finalMove = horizontalMove + verticalVelocity;
        controller.Move(finalMove * Time.deltaTime);

        jumpRequested = false;
    }

    private Vector3 CalculateGroundMovement()
    {
        float currentForwardSpeed = forwardBackInput >= 0
            ? forwardBackInput * forwardSpeed
            : forwardBackInput * backwardSpeed;

        Vector3 forwardMove = transform.forward * currentForwardSpeed;
        Vector3 strafeMove = transform.right * strafeInput * strafeSpeed;

        return forwardMove + strafeMove;
    }

    public void TryJump()
    {
        jumpRequested = true;
    }
}