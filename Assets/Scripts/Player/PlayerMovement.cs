using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController controller;
    private PlayerInputActions inputActions;
    private Animator animator;

    [Header("Animation")]
    [SerializeField] private Animator characterAnimator;
    [SerializeField] private string speedParameterName = "Speed";
    [SerializeField] private string moveXParameterName = "MoveX";
    [SerializeField] private string moveZParameterName = "MoveZ";

    [Header("Movement")]
    public float forwardSpeed = 5f;
    public float backwardSpeed = 2.5f;
    public float strafeSpeed = 5f;
    public float turnSpeed = 140f;
    public float mouseTurnSpeed = 0.15f;

    [Header("Jumping")]
    public float jumpHeight = 1.5f;
    public float gravity = -9.81f;
    public float idleJumpForwardNudge = 2f;

    private float forwardBackInput;
    private float turnInput;
    private float strafeInput;

    private Vector2 lookInput;
    private bool leftClickHeld;
    private bool rightClickHeld;

    private Vector3 verticalVelocity;
    private Vector3 lastGroundedHorizontalMove;
    private Vector3 lockedAirHorizontalMove;

    private bool jumpRequested;
    private bool jumpedFromIdle;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        animator = characterAnimator;
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        inputActions = new PlayerInputActions();

        inputActions.Player.ForwardBack.performed += ctx => forwardBackInput = ctx.ReadValue<float>();
        inputActions.Player.ForwardBack.canceled += ctx => forwardBackInput = 0f;

        inputActions.Player.Turn.performed += ctx => turnInput = ctx.ReadValue<float>();
        inputActions.Player.Turn.canceled += ctx => turnInput = 0f;

        inputActions.Player.Strafe.performed += ctx => strafeInput = ctx.ReadValue<float>();
        inputActions.Player.Strafe.canceled += ctx => strafeInput = 0f;

        inputActions.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Look.canceled += ctx => lookInput = Vector2.zero;

        inputActions.Player.LeftClick.performed += ctx => leftClickHeld = true;
        inputActions.Player.LeftClick.canceled += ctx => leftClickHeld = false;

        inputActions.Player.RightClick.performed += ctx => rightClickHeld = true;
        inputActions.Player.RightClick.canceled += ctx => rightClickHeld = false;

        inputActions.Player.Jump.performed += ctx => jumpRequested = true;
    }

    private void OnEnable()
    {
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    private void Update()
    {

        bool isGrounded = controller.isGrounded;

        HandleTurning();

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

                jumpedFromIdle = lastGroundedHorizontalMove.magnitude < 0.1f;
                lockedAirHorizontalMove = lastGroundedHorizontalMove;
            }
        }
        else
        {
            horizontalMove = lockedAirHorizontalMove;

            if (jumpedFromIdle && WantsForwardMovement())
            {
                horizontalMove = transform.forward * idleJumpForwardNudge;
                lockedAirHorizontalMove = horizontalMove;
                jumpedFromIdle = false;
            }
        }

        UpdateAnimation(horizontalMove);

        verticalVelocity.y += gravity * Time.deltaTime;

        Vector3 finalMove = horizontalMove + verticalVelocity;
        controller.Move(finalMove * Time.deltaTime);

        jumpRequested = false;
    }

    private void UpdateAnimation(Vector3 horizontalMove)
    {
        if (animator == null) return;

        float speed = horizontalMove.magnitude;

        animator.SetFloat(speedParameterName, speed, 0.05f, Time.deltaTime);

        float moveX = strafeInput;
        float moveZ = forwardBackInput;

        if (leftClickHeld && rightClickHeld)
        {
            moveZ = 1f;
        }

        animator.SetFloat(moveXParameterName, moveX, 0.05f, Time.deltaTime);
        animator.SetFloat(moveZParameterName, moveZ, 0.05f, Time.deltaTime);
    }

    private void HandleTurning()
    {
        transform.Rotate(Vector3.up * turnInput * turnSpeed * Time.deltaTime);

        if (rightClickHeld)
        {
            transform.Rotate(Vector3.up * lookInput.x * mouseTurnSpeed);
        }
    }

    private Vector3 CalculateGroundMovement()
    {
        float effectiveForwardBackInput = forwardBackInput;

        if (leftClickHeld && rightClickHeld)
        {
            effectiveForwardBackInput = 1f;
        }

        float currentForwardSpeed = effectiveForwardBackInput >= 0
            ? effectiveForwardBackInput * forwardSpeed
            : effectiveForwardBackInput * backwardSpeed;

        Vector3 forwardMove = transform.forward * currentForwardSpeed;
        Vector3 strafeMove = transform.right * strafeInput * strafeSpeed;

        return forwardMove + strafeMove;
    }

    private bool WantsForwardMovement()
    {
        return forwardBackInput > 0 || (leftClickHeld && rightClickHeld);
    }

    public void TryJump()
    {
        jumpRequested = true;
    }
}