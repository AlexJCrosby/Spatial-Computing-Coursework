using UnityEngine;

public class GoblinAI : MonoBehaviour
{
    [Header("Gravity")]
    [SerializeField] private float gravity = -15f;
    private Vector3 verticalVelocity;

    [Header("Target")]
    [SerializeField] private Transform player;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private int damage = 1;
    [SerializeField] private float obstacleCheckDistance = 1.2f;
    [SerializeField] private float obstacleCheckHeight = 0.8f;
    [SerializeField] private float avoidanceStrength = 1.5f;
    [SerializeField] private LayerMask obstacleLayers;

    [Header("Attack")]
    [SerializeField] private float attackCooldown = 1.2f;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    private CharacterController controller;
    private int nextAttackIndex = 1;
    private float lastAttackTime;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }
    }

    private void Update()
    {
        if (player == null) return;

        Vector3 horizontalMovement = Vector3.zero;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange)
        {
            FacePlayer();

            if (distanceToPlayer > attackRange)
            {
                horizontalMovement = GetMovementTowardsPlayer();
                SetSpeed(1f);
            }
            else
            {
                SetSpeed(0f);
                TryAttack();
            }
        }
        else
        {
            SetSpeed(0f);
        }

        MoveWithGravity(horizontalMovement);
    }

    private Vector3 GetMovementTowardsPlayer()
    {
        Vector3 desiredDirection = player.position - transform.position;
        desiredDirection.y = 0f;

        if (desiredDirection.sqrMagnitude < 0.001f)
        {
            return Vector3.zero;
        }

        desiredDirection.Normalize();

        Vector3 rayOrigin = transform.position + Vector3.up * obstacleCheckHeight;

        bool blockedAhead = Physics.Raycast(
            rayOrigin,
            desiredDirection,
            obstacleCheckDistance,
            obstacleLayers
        );

        if (blockedAhead)
        {
            Vector3 leftDirection = Quaternion.Euler(0f, -60f, 0f) * desiredDirection;
            Vector3 rightDirection = Quaternion.Euler(0f, 60f, 0f) * desiredDirection;

            bool blockedLeft = Physics.Raycast(
                rayOrigin,
                leftDirection,
                obstacleCheckDistance,
                obstacleLayers
            );

            bool blockedRight = Physics.Raycast(
                rayOrigin,
                rightDirection,
                obstacleCheckDistance,
                obstacleLayers
            );

            if (!blockedLeft && blockedRight)
            {
                desiredDirection = leftDirection;
            }
            else if (blockedLeft && !blockedRight)
            {
                desiredDirection = rightDirection;
            }
            else if (!blockedLeft && !blockedRight)
            {
                desiredDirection =
                    Vector3.Distance(transform.position + leftDirection, player.position) <
                    Vector3.Distance(transform.position + rightDirection, player.position)
                        ? leftDirection
                        : rightDirection;
            }
            else
            {
                desiredDirection = -desiredDirection;
            }
        }

        return desiredDirection * moveSpeed * avoidanceStrength;
    }

    private void MoveWithGravity(Vector3 horizontalMovement)
    {
        if (controller == null)
        {
            Debug.LogWarning(name + " has no CharacterController. Add one to the goblin prefab.");
            transform.position += horizontalMovement * Time.deltaTime;
            return;
        }

        if (controller.isGrounded && verticalVelocity.y < 0f)
        {
            verticalVelocity.y = -2f;
        }

        verticalVelocity.y += gravity * Time.deltaTime;

        Vector3 finalMovement = horizontalMovement + verticalVelocity;

        controller.Move(finalMovement * Time.deltaTime);
    }

    private void TryAttack()
    {
        if (animator == null) return;

        if (Time.time > lastAttackTime + attackCooldown)
        {
            animator.SetInteger("AttackIndex", nextAttackIndex);

            nextAttackIndex++;

            if (nextAttackIndex > 2)
            {
                nextAttackIndex = 1;
            }

            lastAttackTime = Time.time;

            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }

            Invoke(nameof(ResetAttackIndex), 0.1f);
        }
    }

    private void ResetAttackIndex()
    {
        if (animator != null)
        {
            animator.SetInteger("AttackIndex", 0);
        }
    }

    private void SetSpeed(float speed)
    {
        if (animator != null)
        {
            animator.SetFloat("Speed", speed);
        }
    }

    private void FacePlayer()
    {
        Vector3 lookDirection = player.position - transform.position;
        lookDirection.y = 0f;

        if (lookDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(lookDirection);
        }
    }
}