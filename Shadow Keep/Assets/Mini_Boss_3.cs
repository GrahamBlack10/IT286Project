using UnityEngine;

public class Mini_Boss_3 : MonoBehaviour
{
    // Core Stats (less challenging than Fallen_Hero)
    public int maxHealth = 450;
    private int currentHealth;
    public int attackDamage = 35;
    public int baseAttackDamage = 35;
    public int defense = 7;
    public float movementSpeed = 2.0f;
    public float detectionRange = 10.0f;
    public float attackRange = 2.0f;
    public float attackCooldown = 2.0f;
    public float deathAnimationDuration = 1.5f;

    // Heal and Buff
    public float selfHealCooldown = 12.0f;
    private float lastHealTime = 0f;
    public int healAmount = 150;
    public float bonusDamageDuration = 4.0f;
    private float bonusDamageTimer = 0f;
    private bool bonusDamageActive = false;

    // Special Ability: Summon Minions
    public GameObject skeletonPrefab;
    public int minionsToSummon = 3;
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

    private EnemyAudio audioManager;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        audioManager = GetComponent<EnemyAudio>();

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

        if (isPlayerNearby && distanceToPlayer <= attackRange && !isAttacking)
        {
            if (Time.time >= lastSummonTime + summonCooldown && Random.value < 0.3f)
            {
                SummonMinions();
            }
            else
            {
                AttackPlayer();
            }
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
        Debug.Log("Mini_Boss_3 attacked the player!");
        audioManager.PlayAttack();

        Invoke(nameof(EnableAttackCollider), 0.2f);
        Invoke(nameof(DisableAttackCollider), 0.5f);
        Invoke(nameof(ResetAttack), attackCooldown);
    }

    private void SummonMinions()
    {
        if (isDead || isAttacking)
            return;

        isAttacking = true;
        lastSummonTime = Time.time;
        Debug.Log("Mini_Boss_3 is summoning minions!");

        animator.SetTrigger("attack");

        if (skeletonPrefab == null)
        {
            Debug.LogWarning("Skeleton prefab not assigned in the Inspector! Creating a fallback skeleton.");
            skeletonPrefab = new GameObject("Skeleton");
            skeletonPrefab.AddComponent<SpriteRenderer>();
            skeletonPrefab.AddComponent<BoxCollider2D>();
            skeletonPrefab.AddComponent<Rigidbody2D>();
            skeletonPrefab.AddComponent<Skeleton_Stats>();
        }

        for (int i = 0; i < minionsToSummon; i++)
        {
            Vector3 spawnPosition = transform.position + (Vector3)Random.insideUnitCircle * 1.5f;
            Instantiate(skeletonPrefab, spawnPosition, Quaternion.identity);
        }

        Invoke(nameof(ResetAttack), attackCooldown);
    }

    private void HealAndBuff()
    {
        lastHealTime = Time.time;
        currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth);
        bonusDamageActive = true;
        bonusDamageTimer = bonusDamageDuration;
        attackDamage = baseAttackDamage + 20;
        animator.SetTrigger("heal");
        Debug.Log("Mini_Boss_3 healed and gained bonus damage!");
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

    private void FollowPlayer()
    {
        if (isAttacking)
            return;

        animator.SetBool("isWalking", true);
        FlipTowardsPlayer();
        Vector3 targetPosition = new Vector3(player.position.x, transform.position.y, transform.position.z);
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, movementSpeed * Time.deltaTime);
    }

    private void FlipTowardsPlayer()
    {
        if (player == null)
            return;

        Vector3 scale = transform.localScale;
        scale.x = (player.position.x > transform.position.x) ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

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
        audioManager.PlayHurt();

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
