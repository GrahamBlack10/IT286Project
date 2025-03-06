using UnityEngine;
using System.Collections;

public class Evil_Wizard : MonoBehaviour
{
    public int maxHealth = 700; // increased health
    private int currentHealth;
    public int attackDamage = 35; // lowered physical attack damage
    public int defense = 8; // lowered defense
    public float movementSpeed = 2.5f; // slightly faster movement
    public float detectionRange = 10.0f; // increased detection range
    public float attackRange = 2.0f; // same attack range
    public float attackCooldown = 2.0f; // slightly longer attack cooldown
    public int spellHealthBoost = 120; // amount of health to regenerate per buff cycle
    public int spellDamageBoost = 25; // amount of attack increase per buff cycle
    public float deathAnimationDuration = 1.0f; // duration of death animation

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

        // Only update movement animations if not attacking.
        if (distanceToPlayer <= detectionRange)
        {
            isPlayerNearby = true;
            if (!isAttacking)
                FollowPlayer();
        }
        else
        {
            isPlayerNearby = false;
            // Only play Idle if not already in Idle state.
            if (!isAttacking && !animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
                animator.Play("Idle");
        }

        // Attack if within melee range and not already attacking.
        if (isPlayerNearby && distanceToPlayer <= attackRange && !isAttacking)
        {
            Debug.Log("Evil-Wizard is attempting to attack!");
            AttackPlayer();
        }

        StickToPlatform();
    }

    private void AttackPlayer()
    {
        if (isDead || isAttacking)
            return;

        isAttacking = true;
        lastAttackTime = Time.time;
        // Play the Attack animation (make sure the clip is set to not loop in your Animator)
        animator.Play("Attack");
        Debug.Log("Evil-Wizard attacked the player!");

        Invoke(nameof(EnableAttackCollider), 0.2f);
        Invoke(nameof(DisableAttackCollider), 0.5f);
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
            // Transition to Idle if player is not nearby.
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

        if (currentHealth <= 0)
            Die();
    }

    private void FollowPlayer()
    {
        if (isAttacking)
            return;

        // Only play "Run" if not already running.
        if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Run"))
            animator.Play("Run");

        FlipTowardsPlayer();
        Vector3 targetPosition = new Vector3(player.position.x, transform.position.y, transform.position.z);
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, movementSpeed * Time.deltaTime);
    }

    // Flips the wizard to face the player based on relative x positions.
    private void FlipTowardsPlayer()
    {
        if (player == null)
            return;

        Vector3 scale = transform.localScale;
        if (player.position.x > transform.position.x)
            scale.x = Mathf.Abs(scale.x); // Face right.
        else
            scale.x = -Mathf.Abs(scale.x); // Face left.
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
        // Play the Death animation (ensure the clip is set to not loop)
        animator.Play("Death");
        Debug.Log("Evil-Wizard has died.");

        if (attackCollider != null)
            attackCollider.enabled = false;
        if (rb != null)
            rb.constraints = RigidbodyConstraints2D.FreezeAll;

        // Delay destruction until the death animation has finished.
        Invoke(nameof(DestroyEvilWizard), deathAnimationDuration);
    }

    private void DestroyEvilWizard()
    {
        Destroy(gameObject);
    }
}
