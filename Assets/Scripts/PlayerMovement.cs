using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController controller;
    private PlayerInputActions inputActions;

    public float moveSpeed = 5f;

    private Vector2 moveInput;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        inputActions = new PlayerInputActions();

        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero;
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
        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);

        controller.Move(move * moveSpeed * Time.deltaTime);
    }
}