using UnityEngine;

public class Mini_Boss_3 : MonoBehaviour
{
    // Core Stats (less challenging than Fallen_Hero)
    public int maxHealth = 1000;
    private int currentHealth;
    public int attackDamage = 35;
    public int defense = 7;
    public float movementSpeed = 2.0f;
    public float detectionRange = 10.0f;
    public float attackRange = 2.0f;
    public float attackCooldown = 2.0f;
    public float deathAnimationDuration = 1.5f;

    // Special Ability: Summon Minions (using the "Skeleton" prefab)
    // You can assign this in the Inspector from your EnemyPrefab folder.
    // If not assigned, a fallback skeleton will be created.
    public GameObject skeletonPrefab;
    public int minionsToSummon = 2;
    public float summonCooldown = 10.0f;
    private float lastSummonTime = 0f;

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

        // Retrieve attack and detection colliders.
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
            Debug.LogError("Mini_Boss_3 attack collider not found!");

        if (detectionCollider == null)
            Debug.LogError("Mini_Boss_3 detection collider not found!");
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
        {
            Debug.LogError("Player GameObject not found!");
        }

        if (rb != null)
            rb.freezeRotation = true;
    }

    void FixedUpdate()
    {
        if (isDead)
            return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Follow the player if within detection range.
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

        // When within melee range and not attacking, decide between a melee attack or summoning minions.
        if (isPlayerNearby && distanceToPlayer <= attackRange && !isAttacking)
        {
            // 30% chance to summon minions if the ability is off cooldown.
            if (Time.time >= lastSummonTime + summonCooldown && Random.value < 0.3f)
            {
                SummonMinions();
            }
            else
            {
                AttackPlayer();
            }
        }

        StickToPlatform();
    }

    // Normal melee attack using the "attack" trigger.
    private void AttackPlayer()
    {
        if (isDead || isAttacking)
            return;

        isAttacking = true;
        lastAttackTime = Time.time;
        animator.SetTrigger("attack");
        Debug.Log("Mini_Boss_3 attacked the player!");

        Invoke(nameof(EnableAttackCollider), 0.2f);
        Invoke(nameof(DisableAttackCollider), 0.5f);
        Invoke(nameof(ResetAttack), attackCooldown);
    }

    // Special Ability: Summon minions by instantiating the "Skeleton" prefab.
    private void SummonMinions()
    {
        if (isDead || isAttacking)
            return;

        isAttacking = true;
        lastSummonTime = Time.time;
        Debug.Log("Mini_Boss_3 is summoning minions!");

        // Trigger a summoning animation (using the "attack" trigger)
        animator.SetTrigger("attack");

        // If no prefab is assigned, create a fallback skeleton.
        if (skeletonPrefab == null)
        {
            Debug.LogWarning("Skeleton prefab not assigned in the Inspector! Creating a fallback skeleton.");
            skeletonPrefab = new GameObject("Skeleton");
            // Add basic components to the fallback skeleton.
            skeletonPrefab.AddComponent<SpriteRenderer>();
            skeletonPrefab.AddComponent<BoxCollider2D>();
            skeletonPrefab.AddComponent<Rigidbody2D>();
            skeletonPrefab.AddComponent<Skeleton_Stats>();
        
        }

        // Spawn the specified number of minions near the boss.
        for (int i = 0; i < minionsToSummon; i++)
        {
            Vector3 spawnPosition = transform.position + (Vector3)Random.insideUnitCircle * 1.5f;
            Instantiate(skeletonPrefab, spawnPosition, Quaternion.identity);
        }

        Invoke(nameof(ResetAttack), attackCooldown);
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

    // Follows the player by moving toward their horizontal position.
    private void FollowPlayer()
    {
        if (isAttacking)
            return;

        animator.SetBool("isWalking", true);
        FlipTowardsPlayer();
        Vector3 targetPosition = new Vector3(player.position.x, transform.position.y, transform.position.z);
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, movementSpeed * Time.deltaTime);
    }

    // Flips the boss sprite to face the player.
    private void FlipTowardsPlayer()
    {
        if (player == null)
            return;

        Vector3 scale = transform.localScale;
        scale.x = (player.position.x > transform.position.x) ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    // Uses a raycast to ensure the boss sticks to the ground.
    private void StickToPlatform()
    {
        if (rb != null)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 1.2f, LayerMask.GetMask("Ground"));
            if (hit.collider == null)
            {
                rb.gravityScale = 1f;
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y - 2f);
            }
            else
            {
                rb.gravityScale = 0f;
                rb.linearVelocity = Vector2.zero;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead)
            return;

        if (collision.gameObject.CompareTag("Lava"))
        {
            Debug.Log("Mini_Boss_3 fell into lava!");
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
                Debug.Log("Mini_Boss_3 successfully damaged the player!");
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        int finalDamage = Mathf.Max(damage - defense, 0);
        currentHealth -= finalDamage;
        Debug.Log($"Mini_Boss_3 took {finalDamage} damage, remaining health: {currentHealth}");

        animator.SetBool("takeDamage", true);
        Invoke(nameof(ResetTakeDamage), 0.3f);

        if (currentHealth <= 0)
            Die();
    }

    private void ResetTakeDamage()
    {
        animator.SetBool("takeDamage", false);
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;
        animator.SetTrigger("die");
        Debug.Log("Mini_Boss_3 has died.");

        if (attackCollider != null)
            attackCollider.enabled = false;
        if (rb != null)
            rb.constraints = RigidbodyConstraints2D.FreezeAll;

        Invoke(nameof(DestroyMiniBoss3), deathAnimationDuration);
    }

    private void DestroyMiniBoss3()
    {
        Destroy(gameObject);
    }
}
