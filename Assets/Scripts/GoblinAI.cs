using UnityEngine;

public class GoblinAI : MonoBehaviour
{
    [Header("Gravity")]
    [SerializeField] private float gravity = -9.81f;
    private Vector3 verticalVelocity;

    [Header("Target")]
    [SerializeField] private Transform player;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private int damage = 1;

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

        ApplyGravity();

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange)
        {
            FacePlayer();

            if (distanceToPlayer > attackRange)
            {
                MoveTowardsPlayer();
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
    }

    private void ApplyGravity()
    {
        if (controller == null) return;

        if (controller.isGrounded && verticalVelocity.y < 0)
        {
            verticalVelocity.y = -2f;
        }

        verticalVelocity.y += gravity * Time.deltaTime;

        controller.Move(verticalVelocity * Time.deltaTime);
    }

    private void MoveTowardsPlayer()
    {
        Vector3 direction = player.position - transform.position;
        direction.y = 0f;
        direction.Normalize();

        Vector3 movement = direction * moveSpeed * Time.deltaTime;

        if (controller != null)
        {
            controller.Move(movement);
        }
        else
        {
            transform.position += movement;
        }
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