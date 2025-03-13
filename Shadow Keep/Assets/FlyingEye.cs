using UnityEngine;

public class FlyingEye : MonoBehaviour
{
    public int maxHealth = 150;
    private int currentHealth;
    public int attackDamage = 25;
    public int defense = 3;
    public float movementSpeed = 4.5f;
    public float detectionRange = 6.0f; // Flying Eye detects player within this range
    public float attackRange = 2.5f; // Flying Eye attacks only when this close
    public float attackCooldown = 1.0f;
    public float hoverHeight = 2.0f; // Maintains this height relative to the player
    private float lastAttackTime;
    private bool isAttacking = false;
    private bool isTakingDamage = false;
    private bool isDead = false;
    private bool isPlayerNearby = false;

    private Transform player;
    private PlayerMovementScript playerMovement;
    private PlayerInformationScript playerInfo;
    private Vector3 initialPosition;
    private Rigidbody2D rb;
    private Animator animator;
    private PolygonCollider2D attackCollider;
    private PolygonCollider2D detectionCollider;
    private PolygonCollider2D playerAttackCollider;

    void Start()
    {
        currentHealth = maxHealth;
        initialPosition = transform.position;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // Disable gravity so the enemy floats
        if (rb != null)
        {
            rb.gravityScale = 0; // Removes gravity effect so it can hover
            rb.freezeRotation = true;
        }

        // Get attack and detection colliders
        PolygonCollider2D[] colliders = GetComponents<PolygonCollider2D>();
        foreach (PolygonCollider2D collider in colliders)
        {
            if (collider.isTrigger)
                detectionCollider = collider;
            else
                attackCollider = collider;
        }

        if (attackCollider != null)
        {
            attackCollider.enabled = false;
        }
        else
        {
            Debug.LogError("Flying Eye attack collider not found!");
        }

        if (detectionCollider == null)
        {
            Debug.LogError("Flying Eye detection collider not found!");
        }
        else
        {
            detectionCollider.isTrigger = true; // Ensure detection collider is a trigger
        }

        GameObject playerObj = GameObject.Find("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerMovement = playerObj.GetComponent<PlayerMovementScript>();
            playerInfo = playerObj.GetComponent<PlayerInformationScript>();
            playerAttackCollider = playerMovement.closeRangeAttackCollider;
        }
        else
        {
            Debug.LogError("Player GameObject not found!");
        }
    }

    void FixedUpdate()
    {
        if (isDead) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Detect player when they're within the detection range
        if (distanceToPlayer <= detectionRange)
        {
            isPlayerNearby = true;
            // Dive if the player is very close, otherwise hover
            if (distanceToPlayer <= attackRange * 2)  // Tweak this threshold as needed
            {
                DiveTowardsPlayer();
            }
            else
            {
                animator.SetBool("isDiving", false);
                HoverTowardsPlayer();
            }
        }
        else
        {
            isPlayerNearby = false;
            animator.SetBool("isFlying", false);
        }

        // Attack when the player is close enough
        if (isPlayerNearby && distanceToPlayer <= attackRange && !isAttacking)
        {
            AttackPlayer();
        }
    }

    private void HoverTowardsPlayer()
    {
        if (player == null) return;

        animator.SetBool("isFlying", true);

        // Move towards the player's position with a hover offset
        Vector3 targetPosition = new Vector3(
            player.position.x,
            player.position.y + hoverHeight, // maintain hover height
            transform.position.z
        );

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, movementSpeed * Time.deltaTime);

        // Flip sprite based on horizontal direction
        if ((player.position.x < transform.position.x && transform.localScale.x > 0) ||
            (player.position.x > transform.position.x && transform.localScale.x < 0))
        {
            transform.localScale = new Vector3(
                Mathf.Abs(transform.localScale.x) * (player.position.x < transform.position.x ? -1 : 1),
                transform.localScale.y,
                transform.localScale.z
            );
        }
    }

    private void DiveTowardsPlayer()
    {
        if (player == null) return;

        animator.SetBool("isDiving", true); // Optionally trigger a dive animation

        // Target the player's actual position (no hover offset)
        Vector3 targetPosition = new Vector3(
            player.position.x,
            player.position.y,
            transform.position.z
        );

        // Increase speed during the dive to simulate a fast descent
        float diveSpeed = movementSpeed * 1.5f;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, diveSpeed * Time.deltaTime);

        // Flip sprite based on horizontal direction
        if ((player.position.x < transform.position.x && transform.localScale.x > 0) ||
            (player.position.x > transform.position.x && transform.localScale.x < 0))
        {
            transform.localScale = new Vector3(
                Mathf.Abs(transform.localScale.x) * (player.position.x < transform.position.x ? -1 : 1),
                transform.localScale.y,
                transform.localScale.z
            );
        }
    }

    private void AttackPlayer()
    {
        if (isDead || isAttacking) return;

        isAttacking = true;
        lastAttackTime = Time.time;
        animator.SetTrigger("attack");

        Invoke(nameof(EnableAttackCollider), 0.2f);
        Invoke(nameof(DisableAttackCollider), 0.5f);
        Invoke(nameof(ResetAttack), attackCooldown);
    }

    private void EnableAttackCollider()
    {
        if (attackCollider != null)
        {
            attackCollider.enabled = true;
        }
    }

    private void DisableAttackCollider()
    {
        if (attackCollider != null)
        {
            attackCollider.enabled = false;
        }
    }

    private void ResetAttack()
    {
        isAttacking = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("Lava"))
        {
            Debug.Log("Flying Eye fell into lava!");
            Die();
        }
        else if (collision == playerAttackCollider && playerMovement.isAttacking)
        {
            if (playerInfo != null)
            {
                TakeDamage((int)playerInfo.getAttackDamage());
            }
        }
        else if (collision.CompareTag("Player") && attackCollider.enabled)
        {
            if (playerInfo != null)
            {
                playerInfo.takeDamage(attackDamage);
                Debug.Log("Flying Eye successfully damaged the player!");
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        int finalDamage = Mathf.Max(damage - defense, 0);
        currentHealth -= finalDamage;
        Debug.Log($"Flying Eye took {finalDamage} damage, remaining health: {currentHealth}");

        if (!isTakingDamage)
        {
            isTakingDamage = true;
            animator.SetBool("takeDamage", true);
            Invoke(nameof(ResetTakingDamage), 0.3f);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void ResetTakingDamage()
    {
        isTakingDamage = false;
        animator.SetBool("takeDamage", false);
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;
        animator.SetTrigger("die");
        Debug.Log("Flying Eye has died.");

        animator.SetBool("isFlying", false);
        animator.ResetTrigger("attack");

        if (attackCollider != null) attackCollider.enabled = false;
        if (rb != null) rb.constraints = RigidbodyConstraints2D.FreezeAll;

        Invoke(nameof(DestroyFlyingEye), 2f);
    }

    private void DestroyFlyingEye()
    {
        Destroy(gameObject);
    }
}
