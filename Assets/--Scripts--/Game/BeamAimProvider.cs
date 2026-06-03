using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class BeamAimProvider : MonoBehaviour
{
    [SerializeField] private bool useMouseFallback = false;
    [SerializeField] private bool logDebugInfo = false;

    private InputDevice beamDevice;
    private Vector2Control unifiedScreenGazePosition;
    private Vector2Control viewportGazePosition;

    private void Start()
    {
        Debug.Log("BeamAimProvider Start is running.");

        foreach (InputDevice device in InputSystem.devices)
        {
            if (device.GetType().Name == "BeamEyeTrackerInputDevice")
            {
                beamDevice = device;

                unifiedScreenGazePosition =
                    device.TryGetChildControl<Vector2Control>("unifiedScreenGazePosition");

                viewportGazePosition =
                    device.TryGetChildControl<Vector2Control>("viewportGazePosition");

                if (unifiedScreenGazePosition != null)
                {
                    Debug.Log("Beam unified screen gaze position connected.");
                }
                else if (viewportGazePosition != null)
                {
                    Debug.Log("Beam viewport gaze position connected.");
                }
                else
                {
                    Debug.LogWarning("Beam found, but no continuous gaze position control found.");
                }

                return;
            }
        }

        Debug.LogWarning("Beam device not found.");
    }

    public Vector2 GetAimScreenPosition()
    {
        if (beamDevice != null)
        {
            if (unifiedScreenGazePosition != null)
            {
                Vector2 gazePosition = unifiedScreenGazePosition.ReadValue();

                gazePosition.y = Screen.height - gazePosition.y;

                if (logDebugInfo)
                {
                    Debug.Log("Unified screen gaze: " + gazePosition);
                }

                return gazePosition;
            }

            if (viewportGazePosition != null)
            {
                Vector2 viewportPosition = viewportGazePosition.ReadValue();

                if (logDebugInfo)
                {
                    Debug.Log("Viewport gaze: " + viewportPosition);
                }

                return new Vector2(
                    viewportPosition.x * Screen.width,
                    viewportPosition.y * Screen.height
                );
            }
        }

        if (useMouseFallback && Mouse.current != null)
        {
            return Mouse.current.position.ReadValue();
        }

        return new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
    }
}