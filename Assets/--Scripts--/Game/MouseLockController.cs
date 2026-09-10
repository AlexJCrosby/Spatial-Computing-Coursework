using UnityEngine;
using UnityEngine.InputSystem;

public class MouseLockController : MonoBehaviour
{
    private PlayerInputActions inputActions;

    private bool leftClickHeld;
    private bool rightClickHeld;
    private bool cursorLocked;

    private Vector2 savedMousePosition;

    private void Awake()
    {
        inputActions = new PlayerInputActions();

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
        UnlockCursor();
        inputActions.Disable();
    }

    private void Update()
    {
        bool shouldLockCursor = leftClickHeld || rightClickHeld;

        if (shouldLockCursor && !cursorLocked)
        {
            LockCursor();
        }
        else if (!shouldLockCursor && cursorLocked)
        {
            UnlockCursor();
        }
    }

    private void LockCursor()
    {
        if (Mouse.current != null)
        {
            savedMousePosition = Mouse.current.position.ReadValue();
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        cursorLocked = true;
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (Mouse.current != null)
        {
            Mouse.current.WarpCursorPosition(savedMousePosition);
        }

        cursorLocked = false;
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            UnlockCursor();
        }
    }
}