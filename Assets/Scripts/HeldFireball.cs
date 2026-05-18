using UnityEngine;

public class HeldFireball : MonoBehaviour
{
    [SerializeField] private float followSpeed = 12f;

    public void MoveTo(Vector3 targetPosition)
    {
        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            followSpeed * Time.deltaTime
        );
    }
}