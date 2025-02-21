using UnityEngine;
using System;

public class projectileScript : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public PolygonCollider2D projectileCollider;
    public Rigidbody2D myRigidbody;
    private string directionFacing = "right";
    private float movementSpeed = 20;
    private float lifeSpan = 3;
    private float timer = 0;
    public PlayerInformationScript playerInformationScript;
    public int projectileDamage;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        float scaleFactor = (float)(transform.localScale.y/1);
        movementSpeed = (float)(movementSpeed * Math.Sqrt(scaleFactor));
        myRigidbody.linearVelocityX = movementSpeed; 

        if(directionFacing == "left"){
            flipProjectile();
        }
        playerInformationScript = GameObject.Find("Player").GetComponent<PlayerInformationScript>();
        projectileDamage = playerInformationScript.getProjectileDamage();
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if(timer >= lifeSpan){
            Destroy(gameObject);
        }
    }

    private void flipProjectile(){
        spriteRenderer.flipX = true;
        Vector2[] points = projectileCollider.points;
        for (int i = 0; i < points.Length; i++)
        {
            points[i].x *= -1; // Flip each point horizontally
        }
        projectileCollider.points = points;
        myRigidbody.linearVelocityX = -movementSpeed;
    }

    public void setDirectionFacingToLeft(){
        directionFacing = "left";
    }

    public int getDamage(){
        return projectileDamage;
    }

    public void destroySelf(){
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision){
        if(!collision.gameObject.CompareTag("Enemy") && !collision.gameObject.CompareTag("Player")){
            Destroy(gameObject);
        }
    }
}
