using UnityEngine;

public class Skeleton_Stats : MonoBehaviour
{
    // Stats variables
    public int maxHealth = 100;
    private int currentHealth;
    public int attackDamage = 15;
    public int defense = 5;
    public float movementSpeed = 2.5f;
    public float detectionRange = 5.0f; // Distance at which skeleton detects player
    public float attackRange = 1.5f; // Range within which the skeleton attacks
    public float attackCooldown = 2.0f; // Time between attacks
    private float lastAttackTime;
    private bool isAttacking = false;
    private Transform player;
    private PlayerInformationScript playerInfo;
    private Rigidbody2D rb;
    private Animator animator;
    public BoxCollider2D skeletonCollider;
    public CapsuleCollider2D groundCollider;
    public PolygonCollider2D attackCollider;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        skeletonCollider = GetComponent<BoxCollider2D>();
        groundCollider = GetComponent<CapsuleCollider2D>();
        attackCollider = GetComponent<PolygonCollider2D>();
        attackCollider.enabled = false; // Ensure attack collider is disabled at start
        
        GameObject playerObj = GameObject.Find("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerInfo = playerObj.GetComponent<PlayerInformationScript>();
        }
        else
        {
            Debug.LogError("Player GameObject not found in the scene!");
        }
    }

    void Update()
    {
        if (player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            if (distanceToPlayer <= detectionRange)
            {
                FollowPlayer();
            }
            else
            {
                animator.SetBool("isWalking", false);
                isAttacking = false;
                animator.ResetTrigger("attack");
            }

            if (distanceToPlayer <= attackRange && playerInfo.isAlive)
            {
                if (!isAttacking)
                {
                    AttackPlayer();
                }
            }
            else
            {
                isAttacking = false;
                animator.ResetTrigger("attack");
                attackCollider.enabled = false; 
            }
        }
        else
        {
            isAttacking = false;
            animator.ResetTrigger("attack");
            attackCollider.enabled = false;
        }
    }

    private void FixedUpdate()
    {
        if (rb != null)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 1.2f, LayerMask.GetMask("Ground"));
            if (hit.collider == null)
            {
                rb.gravityScale = 1f;
            }
            else
            {
                rb.gravityScale = 0f;
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Lava"))
        {
            Die();
        }
        else if (collision.gameObject.CompareTag("Player"))
        {
            PlayerInformationScript playerInfo = collision.gameObject.GetComponent<PlayerInformationScript>();
            if (playerInfo != null)
            {
                playerInfo.takeDamage(attackDamage);
                Debug.Log("Skeleton successfully hit the player!");
            }
        }
    }

    private void FollowPlayer()
    {
        if (player != null)
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

    private void AttackPlayer()
    {
        if (player != null && Vector3.Distance(transform.position, player.position) <= attackRange && playerInfo.isAlive)
        {
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                isAttacking = true;
                animator.SetTrigger("attack");
                attackCollider.enabled = true; // Enable attack collider
                Debug.Log("Skeleton attacks the player!");

                if (playerInfo != null)
                {
                    playerInfo.takeDamage(attackDamage);
                    Debug.Log("Skeleton dealt damage to the player!");
                }

                lastAttackTime = Time.time;
                Invoke("DisableAttackCollider", 1.0f); // Extended attack duration
            }
        }
        else
        {
            isAttacking = false;
            animator.ResetTrigger("attack");
            attackCollider.enabled = false;
        }
    }

    private void DisableAttackCollider()
    {
        attackCollider.enabled = false;
    }

    private void Die()
    {
        animator.SetTrigger("die");
        Debug.Log("Skeleton has died.");
        Destroy(gameObject, 2f);
    }
}
