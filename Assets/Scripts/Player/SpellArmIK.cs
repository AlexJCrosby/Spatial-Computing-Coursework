using UnityEngine;
using System.Collections;

public class SpellArmIK : MonoBehaviour
{
    [SerializeField] private Animator animator;

    [Header("IK")]
    [SerializeField] private float ikWeight = 0f;
    [SerializeField] private float aimDistance = 2f;
    [SerializeField] private float maxAimAngle = 45f;

    private AvatarIKGoal currentArm = AvatarIKGoal.RightHand;
    private Vector3 targetPoint;
    private bool hasTarget;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    public void AimAt(Vector3 point, AvatarIKGoal castingArm)
    {
        currentArm = castingArm;
        targetPoint = point;
        hasTarget = true;
        ikWeight = 1f;
    }

    public void AimAtLimited(
    Vector3 realTargetPoint,
    AvatarIKGoal castingArm,
    Transform characterRoot,
    Transform castPoint
)
    {
        currentArm = castingArm;

        Vector3 toTarget = realTargetPoint - castPoint.position;
        toTarget.y = 0f;

        if (toTarget == Vector3.zero)
        {
            toTarget = characterRoot.forward;
        }

        float signedAngle = Vector3.SignedAngle(
            characterRoot.forward,
            toTarget.normalized,
            Vector3.up
        );

        if (castingArm == AvatarIKGoal.LeftHand)
        {
            signedAngle = Mathf.Clamp(signedAngle, -maxAimAngle, 5f);
        }
        else
        {
            signedAngle = Mathf.Clamp(signedAngle, -5f, maxAimAngle);
        }

        Vector3 limitedDirection =
            Quaternion.AngleAxis(signedAngle, Vector3.up) * characterRoot.forward;

        targetPoint = castPoint.position + limitedDirection * aimDistance;

        hasTarget = true;
        ikWeight = 1f;
    }

    public void StopAiming()
    {
        StartCoroutine(FadeOutIK());
    }

    private System.Collections.IEnumerator FadeOutIK()
    {
        float startWeight = ikWeight;
        float duration = 0.25f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            ikWeight = Mathf.Lerp(startWeight, 0f, elapsed / duration);
            yield return null;
        }

        ikWeight = 0f;
        hasTarget = false;
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (animator == null || !hasTarget) return;

        animator.SetIKPositionWeight(currentArm, ikWeight);
        animator.SetIKRotationWeight(currentArm, ikWeight);

        animator.SetIKPosition(currentArm, targetPoint);

        Vector3 handPosition = animator.GetIKPosition(currentArm);
        Vector3 direction = targetPoint - handPosition;

        Debug.Log("IK running on " + currentArm);

        if (direction != Vector3.zero)
        {
            animator.SetIKRotation(currentArm, Quaternion.LookRotation(direction));
        }
    }
}