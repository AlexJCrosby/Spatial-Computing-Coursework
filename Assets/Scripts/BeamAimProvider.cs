using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class BeamAimProvider : MonoBehaviour
{
    [SerializeField] private bool useMouseFallback = false;

    private InputDevice beamDevice;

    private ButtonControl topLeft;
    private ButtonControl topMiddle;
    private ButtonControl topRight;
    private ButtonControl centerLeft;
    private ButtonControl centerRight;
    private ButtonControl bottomLeft;
    private ButtonControl bottomMiddle;
    private ButtonControl bottomRight;

    private void Start()
    {
        Debug.Log("BeamAimProvider Start is running.");
        foreach (InputDevice device in InputSystem.devices)
        {
            if (device.GetType().Name == "BeamEyeTrackerInputDevice")
            {
                beamDevice = device;

                topLeft = device.TryGetChildControl<ButtonControl>("isLookingAtTopLeftCorner");
                topMiddle = device.TryGetChildControl<ButtonControl>("isLookingAtTopMiddle");
                topRight = device.TryGetChildControl<ButtonControl>("isLookingAtTopRightCorner");
                centerLeft = device.TryGetChildControl<ButtonControl>("isLookingAtCenterLeft");
                centerRight = device.TryGetChildControl<ButtonControl>("isLookingAtCenterRight");
                bottomLeft = device.TryGetChildControl<ButtonControl>("isLookingAtBottomLeftCorner");
                bottomMiddle = device.TryGetChildControl<ButtonControl>("isLookingAtBottomMiddle");
                bottomRight = device.TryGetChildControl<ButtonControl>("isLookingAtBottomRightCorner");

                Debug.Log("Beam region controls connected.");
                return;
            }
        }

        Debug.LogWarning("Beam device not found.");
    }

    public Vector2 GetAimScreenPosition()
    {
        if (beamDevice != null)
        {
            if (topLeft != null && topLeft.isPressed)
                return new Vector2(Screen.width * 0.2f, Screen.height * 0.8f);

            if (topMiddle != null && topMiddle.isPressed)
                return new Vector2(Screen.width * 0.5f, Screen.height * 0.8f);

            if (topRight != null && topRight.isPressed)
                return new Vector2(Screen.width * 0.8f, Screen.height * 0.8f);

            if (centerLeft != null && centerLeft.isPressed)
                return new Vector2(Screen.width * 0.2f, Screen.height * 0.5f);

            if (centerRight != null && centerRight.isPressed)
                return new Vector2(Screen.width * 0.8f, Screen.height * 0.5f);

            if (bottomLeft != null && bottomLeft.isPressed)
                return new Vector2(Screen.width * 0.2f, Screen.height * 0.2f);

            if (bottomMiddle != null && bottomMiddle.isPressed)
                return new Vector2(Screen.width * 0.5f, Screen.height * 0.2f);

            if (bottomRight != null && bottomRight.isPressed)
                return new Vector2(Screen.width * 0.8f, Screen.height * 0.2f);
        }

        if (useMouseFallback && Mouse.current != null)
            return Mouse.current.position.ReadValue();

        return new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
    }
}