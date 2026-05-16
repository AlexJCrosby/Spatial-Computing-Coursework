using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;

    public Vector3 offset = new Vector3(0, 5, -7);

    private void LateUpdate()
    {
        transform.position = target.position + offset;
    }
}