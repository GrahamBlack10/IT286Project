using UnityEngine;

public class Fallen_Hero : MonoBehaviour
{
    // Core Stats
    public int maxHealth = 1500;
    private int currentHealth;
    public int attackDamage = 50;
    public int defense = 10;
    public float movementSpeed = 2.0f;
    public float detectionRange = 12.0f;
    public float attackRange = 2.5f;
    public float attackCooldown = 2.5f;
    public float deathAnimationDuration = 2.0f;

    // Special Abilities: Jump and Invisibility Strike
    public float jumpForce = 8.0f;
    public float jumpCooldown = 5.0f;
    private float lastJumpTime = 0f;

    public float invisStrikeCooldown = 7.0f;
    private float lastInvisStrikeTime = 0f;

    private float lastAttackTime;
    private bool isAttacking = false;
    private bool isDead = false;
    private bool isPlayerNearby = false;

    private Transform player;
    private PlayerMovementScript playerMovement;
    private PlayerInformationScript playerInfo;
    private Rigidbody2D rb;
    private Animator animator;
    private PolygonCollider2D attackCollider;
    private PolygonCollider2D detectionCollider;
    private PolygonCollider2D playerAttackCollider;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

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
            attackCollider.enabled = false;
        else
            Debug.LogError("Fallen-Hero attack collider not found!");

        if (detectionCollider == null)
            Debug.LogError("Fallen-Hero detection collider not found!");
        else
            detectionCollider.isTrigger = true;

        GameObject playerObj = GameObject.Find("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerMovement = playerObj.GetComponent<PlayerMovementScript>();
            playerInfo = playerObj.GetComponent<PlayerInformationScript>();
            playerAttackCollider = playerMovement.closeRangeAttackCollider;
        }
        else
            Debug.LogError("Player GameObject not found!");

        if (rb != null)
            rb.freezeRotation = true;
    }

    void FixedUpdate()
    {
        if (isDead)
            return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Determine if the player is nearby and follow if so.
        if (distanceToPlayer <= detectionRange)
        {
            isPlayerNearby = true;
            if (!isAttacking)
                FollowPlayer();
        }
        else
        {
            isPlayerNearby = false;
            if (!isAttacking)
                animator.SetBool("isWalking", false);
        }

        // Attempt a jump if grounded, not attacking, and if player is not too close
        if (IsGrounded() && !isAttacking && distanceToPlayer > attackRange && Time.time >= lastJumpTime + jumpCooldown)
        {
            Jump();
        }

        // If the player is within melee range and not currently attacking, decide between a normal attack or invisibility strike.
        if (isPlayerNearby && distanceToPlayer <= attackRange && !isAttacking)
        {
            if (Time.time >= lastInvisStrikeTime + invisStrikeCooldown)
                InvisibilityStrike();
            else
                AttackPlayer();
        }

        StickToPlatform();
    }

    // Normal Melee Attack
    private void AttackPlayer()
    {
        if (isDead || isAttacking)
            return;

        isAttacking = true;
        lastAttackTime = Time.time;
        animator.SetTrigger("attack");
        Debug.Log("Fallen-Hero attacked the player!");

        Invoke(nameof(EnableAttackCollider), 0.2f);
        Invoke(nameof(DisableAttackCollider), 0.5f);
        Invoke(nameof(ResetAttack), attackCooldown);
    }

    // Special Move: Invisibility Strike
    private void InvisibilityStrike()
    {
        if (isDead || isAttacking)
            return;

        isAttacking = true;
        lastInvisStrikeTime = Time.time;

        // Turn invisible (simulate by disabling the sprite renderer briefly)
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.enabled = false;

        // Teleport near the player (with a slight horizontal offset)
        Vector3 strikePosition = player.position;
        strikePosition.x += (player.position.x < transform.position.x ? 1f : -1f) * 1.0f;
        transform.position = strikePosition;

        // Reappear immediately
        if (sr != null)
            sr.enabled = true;

        animator.SetTrigger("attack"); // using same attack parameter for invis strike
        Debug.Log("Fallen-Hero performed an invisibility strike!");

        Invoke(nameof(EnableAttackCollider), 0.1f);
        Invoke(nameof(DisableAttackCollider), 0.3f);
        Invoke(nameof(ResetAttack), attackCooldown);
    }

    // Special Move: Jump
    private void Jump()
    {
        if (isDead)
            return;

        lastJumpTime = Time.time;
        animator.SetTrigger("jump");
        Debug.Log("Fallen-Hero jumped!");
        rb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
    }

    private void EnableAttackCollider()
    {
        if (attackCollider != null)
            attackCollider.enabled = true;
    }

    private void DisableAttackCollider()
    {
        if (attackCollider != null)
            attackCollider.enabled = false;
    }

    private void ResetAttack()
    {
        if (!isDead)
        {
            isAttacking = false;
            if (!isPlayerNearby)
                animator.SetBool("isWalking", false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead)
            return;

        if (collision.gameObject.CompareTag("Lava"))
        {
            Debug.Log("Fallen-Hero fell into lava!");
            Die();
        }
        else if (collision == playerAttackCollider && playerMovement.isAttacking)
        {
            if (playerInfo != null)
                TakeDamage((int)playerInfo.getAttackDamage());
        }
        else if (collision.CompareTag("Player") && attackCollider.enabled)
        {
            if (playerInfo != null)
            {
                playerInfo.takeDamage(attackDamage);
                Debug.Log("Fallen-Hero successfully damaged the player!");
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        int finalDamage = Mathf.Max(damage - defense, 0);
        currentHealth -= finalDamage;
        Debug.Log($"Fallen-Hero took {finalDamage} damage, remaining health: {currentHealth}");

        animator.SetBool("takeDamage", true);
        Invoke(nameof(ResetTakeDamage), 0.3f);

        if (currentHealth <= 0)
            Die();
    }

    private void ResetTakeDamage()
    {
        animator.SetBool("takeDamage", false);
    }

    private void FollowPlayer()
    {
        if (isAttacking)
            return;

        animator.SetBool("isWalking", true);
        FlipTowardsPlayer();
        Vector3 targetPosition = new Vector3(player.position.x, transform.position.y, transform.position.z);
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, movementSpeed * Time.deltaTime);
    }

    // Flips the boss to face the player based on relative positions.
    private void FlipTowardsPlayer()
    {
        if (player == null)
            return;

        Vector3 scale = transform.localScale;
        scale.x = (player.position.x > transform.position.x) ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    // Ensures the boss sticks to the platform (basic ground detection)
    private void StickToPlatform()
    {
        if (rb != null)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 1.2f, LayerMask.GetMask("Ground"));
            if (hit.collider == null)
            {
                rb.gravityScale = 1f;
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y - 2f);
                animator.SetBool("fall", true);
            }
            else
            {
                rb.gravityScale = 0f;
                rb.linearVelocity = Vector2.zero;
                animator.SetBool("fall", false);
            }
        }
    }

    // Checks if the boss is grounded using a raycast
    private bool IsGrounded()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 1.2f, LayerMask.GetMask("Ground"));
        return hit.collider != null;
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;
        animator.SetTrigger("die");
        Debug.Log("Fallen-Hero has died.");

        if (attackCollider != null)
            attackCollider.enabled = false;
        if (rb != null)
            rb.constraints = RigidbodyConstraints2D.FreezeAll;

        Invoke(nameof(DestroyFallenHero), deathAnimationDuration);
    }

    private void DestroyFallenHero()
    {
        Destroy(gameObject);
    }
}
