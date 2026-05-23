using UnityEngine;

public class Levitatable : MonoBehaviour
{
    [SerializeField] private float minimumHeight = 1.5f;
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float throwPullDistance = 10f;

    private GoblinAI goblinAI;
    private CharacterController characterController;
    private Rigidbody rb;
    private Animator animator;

    private bool isLevitating;
    private bool isBeingThrownOrPulled;
    private Vector3 targetPosition;

    private void Awake()
    {
        goblinAI = GetComponent<GoblinAI>();
        characterController = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        if (!isLevitating && !isBeingThrownOrPulled) return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        if (isBeingThrownOrPulled && Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            EndLevitate();
        }
    }

    public void BeginLevitate(Vector3 startTargetPosition)
    {
        isLevitating = true;
        isBeingThrownOrPulled = false;

        if (goblinAI != null)
        {
            goblinAI.enabled = false;
        }

        if (characterController != null)
        {
            characterController.enabled = false;
        }

        if (rb != null)
        {
            rb.isKinematic = true;
        }

        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
            animator.SetInteger("AttackIndex", 0);
        }

        SetLevitateTarget(startTargetPosition);
    }

    public void SetLevitateTarget(Vector3 newTargetPosition)
    {
        newTargetPosition.y = Mathf.Max(newTargetPosition.y, minimumHeight);
        targetPosition = newTargetPosition;
    }

    public void PullTowards(Vector3 playerPosition)
    {
        Vector3 direction = (playerPosition - transform.position).normalized;
        targetPosition = transform.position + direction * throwPullDistance;

        isLevitating = false;
        isBeingThrownOrPulled = true;
    }

    public void ThrowAwayFrom(Vector3 playerPosition)
    {
        Vector3 direction = (transform.position - playerPosition).normalized;
        targetPosition = transform.position + direction * throwPullDistance;

        isLevitating = false;
        isBeingThrownOrPulled = true;
    }

    public void EndLevitate()
    {
        isLevitating = false;
        isBeingThrownOrPulled = false;

        if (characterController != null)
        {
            characterController.enabled = true;
        }

        if (goblinAI != null)
        {
            goblinAI.enabled = true;
        }

        if (rb != null)
        {
            rb.isKinematic = false;
        }
    }
    public void ArcMoveTo(Vector3 endPosition, float arcHeight, float duration)
    {
        StopAllCoroutines();
        StartCoroutine(ArcMoveRoutine(endPosition, arcHeight, duration));
    }

    private System.Collections.IEnumerator ArcMoveRoutine(Vector3 endPosition, float arcHeight, float duration)
    {
        Vector3 startPosition = transform.position;
        float elapsed = 0f;

        GoblinAI goblinAI = GetComponent<GoblinAI>();
        CharacterController controller = GetComponent<CharacterController>();

        if (goblinAI != null) goblinAI.enabled = false;
        if (controller != null) controller.enabled = false;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            Vector3 flatPosition = Vector3.Lerp(startPosition, endPosition, t);
            float arc = Mathf.Sin(t * Mathf.PI) * arcHeight;

            transform.position = flatPosition + Vector3.up * arc;

            yield return null;
        }

        transform.position = endPosition;

        if (controller != null) controller.enabled = true;
        if (goblinAI != null) goblinAI.enabled = true;
    }
}