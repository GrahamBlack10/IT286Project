using UnityEngine;

public class Mushroom : MonoBehaviour
{
    // Updated stats for the Mushroom enemy
    public int maxHealth = 80;          // Mushroom has moderate health
    private int currentHealth;
    public int attackDamage = 8;        // Lower attack damage
    public int defense = 4;             // Slightly higher defense
    public float movementSpeed = 1.5f;  // Slower movement speed
    public float detectionRange = 3.0f; // Shorter detection range
    public float attackRange = 1.0f;    // Shorter attack range
    public float attackCooldown = 2.0f; // Slower attack cooldown

    private float lastAttackTime;
    private bool isAttacking = false;
    private bool isTakingDamage = false;
    private bool isDead = false;
    private bool isPlayerNearby = false;  // Ensures the mushroom only attacks when the player is in range

    private Transform player;
    private PlayerMovementScript playerMovement;
    private PlayerInformationScript playerInfo;
    private Vector3 initialPosition;
    private Rigidbody2D rb;
    private Animator animator;
    private PolygonCollider2D attackCollider;    // Mushroom's attack collider
    private PolygonCollider2D detectionCollider;   // Detection area collider
    private PolygonCollider2D playerAttackCollider; // Player's attack collider

    void Start()
    {
        currentHealth = maxHealth;
        initialPosition = transform.position;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // Get attack and detection colliders
        PolygonCollider2D[] colliders = GetComponents<PolygonCollider2D>();
        foreach (PolygonCollider2D collider in colliders)
        {
            if (collider.isTrigger)
                detectionCollider = collider; // Set as detection area
            else
                attackCollider = collider; // Set as attack collider
        }

        if (attackCollider != null)
        {
            attackCollider.enabled = false;
        }
        else
        {
            Debug.LogError("Mushroom attack collider not found!");
        }

        if (detectionCollider == null)
        {
            Debug.LogError("Mushroom detection collider not found!");
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

        // Prevent the mushroom from being pushed by the player
        if (rb != null)
        {
            rb.freezeRotation = true;
            rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        }
    }

    void FixedUpdate()
    {
        if (isDead) return; // Stop all logic if dead

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Detect player when they're within the detection range
        if (distanceToPlayer <= detectionRange)
        {
            isPlayerNearby = true;
            FollowPlayer();
        }
        else
        {
            isPlayerNearby = false;
            animator.SetBool("isWalking", false);
        }

        // Attack when player is close enough
        if (isPlayerNearby && distanceToPlayer <= attackRange && !isAttacking)
        {
            AttackPlayer();
        }
    }

    private void AttackPlayer()
    {
        if (isDead || isAttacking) return; // Attack only if alive and not already attacking

        isAttacking = true;
        lastAttackTime = Time.time;
        animator.SetTrigger("attack");

        Invoke(nameof(EnableAttackCollider), 0.2f); // Enable attack collider during the attack animation
        Invoke(nameof(DisableAttackCollider), 0.5f); // Disable it shortly after
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
            Debug.Log("Mushroom fell into lava!");
            Die(); // Kill mushroom when it touches lava
        }
        else if (collision == playerAttackCollider && playerMovement.isAttacking)
        {
            if (playerInfo != null)
            {
                TakeDamage((int)playerInfo.getAttackDamage()); // Take damage only if player is attacking
            }
        }
        else if (collision.CompareTag("Player") && attackCollider.enabled)
        {
            if (playerInfo != null)
            {
                playerInfo.takeDamage(attackDamage);
                Debug.Log("Mushroom successfully damaged the player!");
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return; // Prevent damage after death

        int finalDamage = Mathf.Max(damage - defense, 0);
        currentHealth -= finalDamage;
        Debug.Log($"Mushroom took {finalDamage} damage, remaining health: {currentHealth}");

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

    private void FollowPlayer()
    {
        if (player != null && isPlayerNearby)
        {
            animator.SetBool("isWalking", true);
            Vector3 targetPosition = new Vector3(player.position.x, transform.position.y, transform.position.z);
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, movementSpeed * Time.deltaTime);

            // Flip direction based on player position
            if ((player.position.x < transform.position.x && transform.localScale.x > 0) ||
                (player.position.x > transform.position.x && transform.localScale.x < 0))
            {
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * (player.position.x < transform.position.x ? -1 : 1),
                                                   transform.localScale.y, transform.localScale.z);
            }
        }
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;
        animator.SetTrigger("die");
        Debug.Log("Mushroom has died.");

        animator.SetBool("isWalking", false);
        animator.ResetTrigger("attack");

        if (attackCollider != null) attackCollider.enabled = false;
        if (rb != null) rb.constraints = RigidbodyConstraints2D.FreezeAll;

        Invoke(nameof(DestroyMushroom), 2f);
    }

    private void DestroyMushroom()
    {
        Destroy(gameObject);
    }
}
