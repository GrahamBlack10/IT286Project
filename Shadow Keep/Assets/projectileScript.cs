using UnityEngine;
using System;

public class projectileScript : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public PolygonCollider2D projectileCollider;
    public Rigidbody2D myRigidbody;
    private string directionFacing = "right";
    private float movementSpeed = 20f;
    private float lifeSpan = 3f;
    private float timer = 0f;
    private bool scaled = false;
    public PlayerInformationScript playerInformationScript;
    public int projectileDamage;

    // Start is called once before the first frame update
    void Start()
    {
        // Scale movementSpeed based on the projectile's y-scale.
        float scaleFactor = transform.localScale.y / 1f;
        movementSpeed = movementSpeed * Mathf.Sqrt(scaleFactor);
        
        // Set the projectile's initial velocity.
        myRigidbody.linearVelocity = new Vector2(movementSpeed, myRigidbody.linearVelocity.y);

        if(directionFacing == "left")
        {
            flipProjectile();
        }
        
        // Get the player's projectile damage.
        playerInformationScript = GameObject.Find("Player").GetComponent<PlayerInformationScript>();
        projectileDamage = playerInformationScript.getProjectileDamage();
        scaled = true;
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if(timer >= lifeSpan)
        {
            Destroy(gameObject);
        }
    }

    private void flipProjectile()
    {
        spriteRenderer.flipX = true;
        Vector2[] points = projectileCollider.points;
        for (int i = 0; i < points.Length; i++)
        {
            points[i].x *= -1; // Flip each point horizontally
        }
        projectileCollider.points = points;
        myRigidbody.linearVelocity = new Vector2(-movementSpeed, myRigidbody.linearVelocity.y);
    }

    public void setDirectionFacingToLeft()
    {
        directionFacing = "left";
    }

    public int getDamage()
    {
        return projectileDamage;
    }

    public void destroySelf()
    {
        Destroy(gameObject);
    }

  private void OnTriggerEnter2D(Collider2D collision)
{
    if (collision.gameObject.CompareTag("Enemy"))
    {
        Mini_Boss_1 enemy1 = collision.gameObject.GetComponent<Mini_Boss_1>();
        if (enemy1 != null)
        {
            enemy1.TakeDamage(projectileDamage);
        }
        else
        {
            Evil_Wizard enemy2 = collision.gameObject.GetComponent<Evil_Wizard>();
            if (enemy2 != null)
            {
                enemy2.TakeDamage(projectileDamage);
            }
            else
            {
                Skeleton_Stats enemy3 = collision.gameObject.GetComponent<Skeleton_Stats>();
                if (enemy3 != null)
                {
                    enemy3.TakeDamage(projectileDamage);
                }
                else
                {
                    FlyingEye enemy4 = collision.gameObject.GetComponent<FlyingEye>();
                    if (enemy4 != null)
                    {
                        enemy4.TakeDamage(projectileDamage);
                    }
                }
            }
        }
        Destroy(gameObject);
    }
    else if (!collision.gameObject.CompareTag("Player"))
    {
        Destroy(gameObject);
    }
}


}
