using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;

    public Vector3 offset = new Vector3(0, 3, -6);
    public float followSmoothness = 10f;
    public float lookHeight = 1.5f;

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 desiredPosition = target.position + target.rotation * offset;

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            followSmoothness * Time.deltaTime
        );

        transform.LookAt(target.position + Vector3.up * lookHeight);
    }
}