using UnityEngine;
using System.Collections;

public class Evil_Wizard : MonoBehaviour
{
    private EnemyAudio audioManager;
    public int maxHealth = 700;
    private int currentHealth;
    public int attackDamage = 35;
    public int baseAttackDamage = 35;
    public int defense = 8;
    public float movementSpeed = 2.5f;
    public float detectionRange = 10.0f;
    public float attackRange = 2.0f;
    public float attackCooldown = 2.0f;
    public int spellHealthBoost = 120;
    public int spellDamageBoost = 25;
    public float deathAnimationDuration = 1.0f;

    // Heal & Buff
    public float healCooldown = 10f;
    private float lastHealTime = 0f;
    private float buffDuration = 5f;
    private float buffTimer = 0f;
    private bool isBuffed = false;

    // Summoning
    public GameObject mushroomPrefab;
    public float summonCooldown = 10f;
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
            Debug.LogError("Evil-Wizard attack collider not found!");

        if (detectionCollider == null)
            Debug.LogError("Evil-Wizard detection collider not found!");
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
            if (!isAttacking && !animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
                animator.Play("Idle");
        }

        if (isPlayerNearby && distanceToPlayer <= attackRange && !isAttacking)
        {
            AttackPlayer();
        }

        if (Time.time >= lastHealTime + healCooldown && currentHealth < maxHealth / 2f)
        {
            HealAndBuff();
        }

        if (Time.time >= lastSummonTime + summonCooldown && !isAttacking && isPlayerNearby)
        {
            SummonMushroom();
        }

        if (isBuffed)
        {
            buffTimer -= Time.deltaTime;
            if (buffTimer <= 0f)
            {
                isBuffed = false;
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
        animator.Play("Attack");
        Debug.Log("Evil-Wizard attacked the player!");

        Invoke(nameof(EnableAttackCollider), 0.2f);
        Invoke(nameof(DisableAttackCollider), 0.5f);
        Invoke(nameof(ResetAttack), attackCooldown);
          audioManager.PlayAttack();
    }

    private void HealAndBuff()
    {
        lastHealTime = Time.time;
        currentHealth = Mathf.Min(currentHealth + spellHealthBoost, maxHealth);
        attackDamage = baseAttackDamage + spellDamageBoost;
        isBuffed = true;
        buffTimer = buffDuration;
        animator.Play("Cast");
        Debug.Log("Evil-Wizard healed and gained bonus damage!");
    }

    private void SummonMushroom()
    {
        lastSummonTime = Time.time;
        animator.Play("Summon");

        Vector3 spawnPos = transform.position + new Vector3(1f, 0, 0);
        if (mushroomPrefab != null)
        {
            Instantiate(mushroomPrefab, spawnPos, Quaternion.identity);
            Debug.Log("Evil-Wizard summoned a Mushroom!");
        }
        else
        {
            Debug.LogWarning("Mushroom prefab not assigned in Evil_Wizard.");
        }
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
                animator.Play("Idle");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead)
            return;

        if (collision.gameObject.CompareTag("Lava"))
        {
            Debug.Log("Evil-Wizard fell into lava!");
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
                Debug.Log("Evil-Wizard successfully damaged the player!");
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        int finalDamage = Mathf.Max(damage - defense, 0);
        currentHealth -= finalDamage;
        Debug.Log($"Evil-Wizard took {finalDamage} damage, remaining health: {currentHealth}");

         audioManager.PlayHurt();

        if (currentHealth <= 0)
            Die();
    }

    private void FollowPlayer()
    {
        if (isAttacking)
            return;

        if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Run"))
            animator.Play("Run");

        FlipTowardsPlayer();
        Vector3 targetPosition = new Vector3(player.position.x, transform.position.y, transform.position.z);
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, movementSpeed * Time.deltaTime);
    }

    private void FlipTowardsPlayer()
    {
        if (player == null)
            return;

        Vector3 scale = transform.localScale;
        if (player.position.x > transform.position.x)
            scale.x = Mathf.Abs(scale.x);
        else
            scale.x = -Mathf.Abs(scale.x);
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

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;
        animator.Play("Death");
        Debug.Log("Evil-Wizard has died.");

        if (attackCollider != null)
            attackCollider.enabled = false;
        if (rb != null)
            rb.constraints = RigidbodyConstraints2D.FreezeAll;

        Invoke(nameof(DestroyEvilWizard), deathAnimationDuration);
    }

    private void DestroyEvilWizard()
    {
        Destroy(gameObject);
    }
}
