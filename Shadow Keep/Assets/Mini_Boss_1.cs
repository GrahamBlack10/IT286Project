using UnityEngine;

public class Mini_Boss_1 : MonoBehaviour
{
    public int maxHealth = 300;
    private int currentHealth;
    public int attackDamage = 50;
    public int defense = 10;
    public float movementSpeed = 2.0f;
    public float detectionRange = 8.0f;
    public float attackRange = 2.0f;
    public float attackCooldown = 1.5f;
    public float spellCooldown = 10.0f;
    public int spellHealthBoost = 100;
    public int spellDamageBoost = 20;

    private float lastAttackTime;
    private float lastSpellTime;
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
            Debug.LogError("Mini-Boss attack collider not found!");
        }

        if (detectionCollider == null)
        {
            Debug.LogError("Mini-Boss detection collider not found!");
        }
        else
        {
            detectionCollider.isTrigger = true;
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

        // Prevent Mini-Boss from being pushed by the player
        if (rb != null)
        {
            rb.freezeRotation = true;
        }
    }

    void FixedUpdate()
    {
        if (isDead) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Detect player within range
        if (distanceToPlayer <= detectionRange)
        {
            isPlayerNearby = true;
            FollowPlayer();
        }
        else
        {
            isPlayerNearby = false;
            animator.SetBool("Walking", false);
        }

        // Attack player when within attack range
        if (isPlayerNearby && distanceToPlayer <= attackRange && !isAttacking)
        {
            Debug.Log("Mini-Boss is attempting to attack!");
            AttackPlayer();
        }

        // Cast a spell if the cooldown has passed
        if (Time.time - lastSpellTime >= spellCooldown && !isAttacking)
        {
            CastSpell();
        }

        StickToPlatform();
    }

    private void AttackPlayer()
    {
        if (isDead || isAttacking) return;

        isAttacking = true;
        lastAttackTime = Time.time;
        animator.SetTrigger("Attack");
        Debug.Log("Mini-Boss attacked the player!");

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

    private void CastSpell()
    {
        if (isDead) return;

        lastSpellTime = Time.time;
        animator.SetTrigger("Cast");
        Debug.Log("Mini-Boss casts a spell!");

        // Randomly boost health or attack damage
        if (Random.value < 0.5f)
        {
            currentHealth = Mathf.Min(currentHealth + spellHealthBoost, maxHealth);
            Debug.Log($"Mini-Boss gained {spellHealthBoost} health!");
        }
        else
        {
            attackDamage += spellDamageBoost;
            Debug.Log($"Mini-Boss gained {spellDamageBoost} attack damage!");
        }

        Invoke(nameof(ResetCastAnimation), 1.0f); // Ensure the cast animation resets properly
    }

    private void ResetCastAnimation()
    {
        animator.ResetTrigger("Cast");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("Lava"))
        {
            Debug.Log("Mini-Boss fell into lava!");
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
                Debug.Log("Mini-Boss successfully damaged the player!");
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        int finalDamage = Mathf.Max(damage - defense, 0);
        currentHealth -= finalDamage;
        Debug.Log($"Mini-Boss took {finalDamage} damage, remaining health: {currentHealth}");

        if (!isTakingDamage)
        {
            isTakingDamage = true;
            animator.SetTrigger("Hurt");
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
    }

     private void FollowPlayer()
    {
        if (player != null && isPlayerNearby)
        {
            animator.SetBool("Walking", true);
            Vector3 targetPosition = new Vector3(player.position.x, transform.position.y, transform.position.z);
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, movementSpeed * Time.deltaTime);

            // Flip direction based on player position
         //   if ((player.position.x < transform.position.x && transform.localScale.x > 0) ||
        //        (player.position.x > transform.position.x && transform.localScale.x < 0))
          //  {
          //      transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * (player.position.x < transform.position.x ? -1 : 1),
          //                                         transform.localScale.y, transform.localScale.z);
            }
        }
   // }

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
                rb.linearVelocity = new Vector2(0, 0);
            }
        }
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;
        animator.SetTrigger("Death");
        Debug.Log("Mini-Boss has died.");

        animator.SetBool("Walking", false);
        animator.ResetTrigger("Attack");
        animator.ResetTrigger("Cast");

        if (attackCollider != null) attackCollider.enabled = false;
        if (rb != null) rb.constraints = RigidbodyConstraints2D.FreezeAll;

        Invoke(nameof(DestroyMiniBoss), 2f);
    }

    private void DestroyMiniBoss()
    {
        Destroy(gameObject);
    }
}
