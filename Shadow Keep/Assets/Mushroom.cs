using UnityEngine;

public class Mushroom : MonoBehaviour
{
    public int maxHealth = 80;
    private int currentHealth;
    public int attackDamage = 8;
    public int defense = 4;
    public float movementSpeed = 1.5f;
    public float detectionRange = 3.0f;
    public float attackRange = 1.0f;
    public float attackCooldown = 2.0f;

    public GameObject projectilePrefab;
    public Transform projectileSpawnPoint;
    public float projectileSpeed = 5f;

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

        PolygonCollider2D[] colliders = GetComponents<PolygonCollider2D>();
        foreach (PolygonCollider2D collider in colliders)
        {
            if (collider.isTrigger)
                detectionCollider = collider;
            else
                attackCollider = collider;
        }

        if (attackCollider != null) attackCollider.enabled = false;
        if (detectionCollider != null) detectionCollider.isTrigger = true;

        GameObject playerObj = GameObject.Find("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerMovement = playerObj.GetComponent<PlayerMovementScript>();
            playerInfo = playerObj.GetComponent<PlayerInformationScript>();
            playerAttackCollider = playerMovement.closeRangeAttackCollider;
        }

        if (rb != null)
        {
            rb.freezeRotation = true;
            rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        }
    }

    void FixedUpdate()
    {
        if (isDead) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

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

        if (isPlayerNearby && distanceToPlayer <= attackRange && !isAttacking)
        {
            AttackPlayer();
        }
    }

    private void AttackPlayer()
    {
        if (isDead || isAttacking) return;

        isAttacking = true;
        lastAttackTime = Time.time;
        animator.SetTrigger("attack");

        Invoke(nameof(FireProjectile), 0.3f);
        Invoke(nameof(ResetAttack), attackCooldown);
    }

    private void FireProjectile()
    {
        if (projectilePrefab != null && projectileSpawnPoint != null && player != null)
        {
            GameObject projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);
            Vector2 direction = (player.position - transform.position).normalized;
            projectile.GetComponent<Rigidbody2D>().linearVelocity = direction * projectileSpeed;
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
                Debug.Log("Mushroom successfully damaged the player!");
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

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
