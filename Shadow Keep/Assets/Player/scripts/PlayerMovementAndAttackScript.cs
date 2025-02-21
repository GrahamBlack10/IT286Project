using System;
using System.Xml;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovementScript : MonoBehaviour
{
    public float horizontalMovementSpeed = 10;
    public float jumpHeight = 10;
    public bool isAttacking = false;
    public double attackBuffer = 0.05;
    private bool isGrounded = true;
    public bool healing = false;
    public float healingPerSecond = 10;
    private float healingCounter = 0;
    public float timePerHealIcon = 1;
    //private string attackType = "closeRangeAttack";
    private string directionFacing = "right";
    private double healLength = 5;
    private double healCount = 0;
    public Rigidbody2D myRidgidBody;
    public SpriteRenderer mySprite;
    public Animator animator;
    public GameObject healingIcon;
    public GameObject projectile;
    public BoxCollider2D playerCollider;
    public CapsuleCollider2D groundCollider;
    public PolygonCollider2D closeRangeAttackCollider;
    public PlayerInformationScript playerInformationScript;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        float scaleFactor = (float)(transform.localScale.y/4.204167);
        horizontalMovementSpeed = (float)(horizontalMovementSpeed * Math.Sqrt(scaleFactor));
        jumpHeight = (float)(jumpHeight * Math.Sqrt(scaleFactor));
    }

    // Update is called once per frame
    void Update()
    {
        if(playerInformationScript.isAlive){
            if(Input.GetKey(KeyCode.A)){
                myRidgidBody.linearVelocityX = -horizontalMovementSpeed;
                if(!isAttacking){
                    flipPlayer("left");
                }
            }else if(Input.GetKey(KeyCode.D)){
                myRidgidBody.linearVelocityX = horizontalMovementSpeed;
                if(!isAttacking){
                    flipPlayer("right");
                }
            }else{
                myRidgidBody.linearVelocityX = 0;
            }
            if(Input.GetKeyDown(KeyCode.W) && isGrounded && !isAttacking){
                myRidgidBody.linearVelocityY = jumpHeight;
                isGrounded = false;
                animator.SetBool("isJumping", !isGrounded);
            }
            if(Input.GetKeyDown(KeyCode.Space) && !isAttacking && !healing){
                //initializing healing
                healing = true;
                healingCounter = timePerHealIcon;
                playerInformationScript.drainPower(playerInformationScript.healAbilityPowerCost);
            }

            if(healing){
                //healing player over time and spawing heal icons
                healingCounter += Time.deltaTime;
                healCount += Time.deltaTime;
                playerInformationScript.healPlayer(healingPerSecond*Time.deltaTime);
                if(healingCounter >= timePerHealIcon){
                    float playerWidth = playerCollider.size.x;
                    float playerHeight = playerCollider.size.y;
                    float xVal = UnityEngine.Random.Range(-playerWidth/2, playerWidth/2);
                    //float yVal = UnityEngine.Random.Range(-playerHeight/2, playerHeight/4);
                    float yVal = -playerHeight/2;
                    healingCounter -= timePerHealIcon;
                    GameObject newHealIcon = Instantiate(healingIcon, transform);
                    newHealIcon.transform.localPosition = new Vector3(xVal, yVal, -1);
                    newHealIcon.transform.localScale *= (float)(transform.localScale.x/4.204167);
                }
                if(healCount >= healLength){
                    healCount = 0;
                    healingCounter = timePerHealIcon;
                    healing = false;
                }
            }
            if(Input.GetMouseButtonDown(0) && !isAttacking && isGrounded){
                //initiate close range attack
                isAttacking = true;
                closeRangeAttackCollider.enabled = true;
                animator.SetTrigger("closeRangeAttack"); 
                //attackType = "closeRangeAttack";
                playerInformationScript.drainPower(playerInformationScript.closeRangeAttackPowerCost);
            }
            if(Input.GetMouseButtonDown(1) && !isAttacking){
                //initiate long range attack
                isAttacking = true;
                animator.SetTrigger("longRangeAttack");
                playerInformationScript.drainPower(playerInformationScript.longRangeAttackPowerCost);
                animator.SetBool("isJumping", false);
            }
            animator.SetFloat("xVelocity", math.abs(myRidgidBody.linearVelocityX));
            animator.SetFloat("yVelocity", myRidgidBody.linearVelocityY);
        }else{
            myRidgidBody.linearVelocityX = 0;
            //myRidgidBody.linearVelocityY = 0;
            animator.SetBool("isJumping", false);
        }
    }

    private void flipPlayer(string direction){
        if(directionFacing == "right" && directionFacing != direction){
            mySprite.flipX = true;
            directionFacing = "left";
            flipPlayerColliders();
        }else if(directionFacing == "left" && directionFacing != direction){
            mySprite.flipX = false;
            directionFacing = "right";
            flipPlayerColliders();
        }
    }

    private void flipPlayerColliders(){
        playerCollider.offset = new Vector2(playerCollider.offset.x*-1, playerCollider.offset.y);
        groundCollider.offset = new Vector2(groundCollider.offset.x*-1, groundCollider.offset.y);
        //closeRangeAttackCollider.offset = new Vector2(closeRangeAttackCollider.offset.x*-1, closeRangeAttackCollider.offset.y);
        Vector2[] points = closeRangeAttackCollider.points;
        for (int i = 0; i < points.Length; i++)
        {
            points[i].x *= -1; // Flip each point horizontally
        }
        closeRangeAttackCollider.points = points;
    }

    private void OnTriggerEnter2D(Collider2D collision){
        if(collision.IsTouching(groundCollider)){
            isGrounded = true;
            animator.SetBool("isJumping", !isGrounded);
            if (collision.gameObject.CompareTag("Lava")){
                playerInformationScript.setHealth(0);
            }
        }else if(collision.IsTouching(closeRangeAttackCollider)){
            if(collision.gameObject.CompareTag("Enemy")){
                //deal damage to enemy
            }
        }
    }

    public void stopCloseRangeAttack(){
        isAttacking = false;
        closeRangeAttackCollider.enabled = false;
    }

    public void stopLongRangeAttack(){
        isAttacking = false;
        if(isGrounded == false){
            animator.SetBool("isJumping", true);
        }
    }

    public void instantiateProjectile(){
        float scaleFactor = (float)(transform.localScale.y/4.204167);
        float xVal = (float)0.1*scaleFactor;
        float yVal = (float)0.1*scaleFactor;
        if(directionFacing == "left"){
            xVal = -xVal;
        }
        GameObject newProjectile = Instantiate(projectile, transform);
        newProjectile.transform.localPosition = new Vector3(xVal, yVal, (float)-0.1492127);
        if(directionFacing == "left"){
            projectileScript projectileScript = newProjectile.GetComponent<projectileScript>();
            projectileScript.setDirectionFacingToLeft();
        }
    }
}
