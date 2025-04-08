using UnityEngine;

public class Fallen_Hero : MonoBehaviour
{
    // Core Stats
    public int maxHealth = 1500;
    private int currentHealth;
    public int attackDamage = 50;
    public int baseAttackDamage = 50;
    public int defense = 10;
    public float movementSpeed = 2.0f;
    public float detectionRange = 12.0f;
    public float attackRange = 2.5f;
    public float attackCooldown = 2.5f;
    public float deathAnimationDuration = 2.0f;

    // Special Abilities: Jump, Invisibility Strike, Heal
    public float jumpForce = 8.0f;
    public float jumpCooldown = 5.0f;
    private float lastJumpTime = 0f;

    public float invisStrikeCooldown = 5.0f;
    private float lastInvisStrikeTime = 0f;

    public float selfHealCooldown = 10.0f;
    private float lastHealTime = 0f;
    public int healAmount = 200;

    public float bonusDamageDuration = 5.0f;
    private float bonusDamageTimer = 0f;
    private bool bonusDamageActive = false;

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
    private bool wasGroundedLastFrame = true;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

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

        if (IsGrounded() && !isAttacking && distanceToPlayer > attackRange && Time.time >= lastJumpTime + jumpCooldown)
        {
            Jump();
        }

        if (isPlayerNearby && distanceToPlayer <= attackRange && !isAttacking)
        {
            if (Time.time >= lastInvisStrikeTime + invisStrikeCooldown)
                InvisibilityStrike();
            else
                AttackPlayer();
        }

        if (Time.time >= lastHealTime + selfHealCooldown && currentHealth < maxHealth / 2)
        {
            HealAndBuff();
        }

        if (bonusDamageActive)
        {
            bonusDamageTimer -= Time.deltaTime;
            if (bonusDamageTimer <= 0)
            {
                bonusDamageActive = false;
                attackDamage = baseAttackDamage;
            }
        }

        StickToPlatform();
    }

    private void AttackPlayer()
    {
        if (isDead || isAttacking)
            return;

        isAttacking = true;
        lastAttackTime = Time.time;
        animator.SetTrigger("attack");

        Invoke(nameof(EnableAttackCollider), 0.2f);
        Invoke(nameof(DisableAttackCollider), 0.5f);
        Invoke(nameof(ResetAttack), attackCooldown);
    }

    private void InvisibilityStrike()
    {
        if (isDead || isAttacking)
            return;

        isAttacking = true;
        lastInvisStrikeTime = Time.time;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = false;

        Vector3 strikePosition = player.position;
        strikePosition.x += (player.position.x < transform.position.x ? 1f : -1f);

        RaycastHit2D hit = Physics2D.Raycast(strikePosition, Vector2.down, 5f, LayerMask.GetMask("Ground"));
        if (hit.collider != null)
        {
            strikePosition.y = hit.point.y + 0.5f;
        }
        else
        {
            strikePosition.y = transform.position.y;
            Debug.LogWarning("Fallen-Hero InvisibilityStrike found no ground below target position!");
        }

        transform.position = strikePosition;
        if (sr != null) sr.enabled = true;

        animator.SetTrigger("attack");
        Invoke(nameof(EnableAttackCollider), 0.1f);
        Invoke(nameof(DisableAttackCollider), 0.3f);
        Invoke(nameof(ResetAttack), attackCooldown);
    }

    private void HealAndBuff()
    {
        lastHealTime = Time.time;
        currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth);
        bonusDamageActive = true;
        bonusDamageTimer = bonusDamageDuration;
        attackDamage = baseAttackDamage + 25;
        animator.SetTrigger("heal");
        Debug.Log("Fallen-Hero healed and gained bonus damage!");
    }

    private void Jump()
    {
        if (isDead || rb == null) return;

        lastJumpTime = Time.time;
        animator.SetTrigger("jump");
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        Debug.Log("Fallen-Hero jumped!");
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
            animator.SetBool("isWalking", isPlayerNearby);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead) return;

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
        else if (collision.CompareTag("Player") && attackCollider != null && attackCollider.enabled)
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
        if (isDead) return;

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
        if (isAttacking) return;

        animator.SetBool("isWalking", true);
        FlipTowardsPlayer();
        Vector3 targetPosition = new Vector3(player.position.x, transform.position.y, transform.position.z);
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, movementSpeed * Time.deltaTime);
    }

    private void FlipTowardsPlayer()
    {
        if (player == null) return;

        Vector3 scale = transform.localScale;
        scale.x = (player.position.x > transform.position.x) ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    private void StickToPlatform()
    {
        if (rb != null)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 1.2f, LayerMask.GetMask("Ground"));
            bool isGroundedNow = hit.collider != null;

            if (isGroundedNow)
            {
                animator.SetBool("fall", false);
            }
            else
            {
                if (wasGroundedLastFrame)
                {
                    animator.SetTrigger("fall");
                }
            }

            wasGroundedLastFrame = isGroundedNow;
        }
    }

    private bool IsGrounded()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 1.2f, LayerMask.GetMask("Ground"));
        return hit.collider != null;
    }

    private void Die()
    {
        if (isDead) return;

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
