using System;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovementScript : MonoBehaviour
{
    public float horizontalMovementSpeed = 10;
    public float jumpHeight = 10;
    public bool isAttacking = false;
    public double attackBuffer = 0.05;
    private bool isGrounded = true;
    private string attackType = "closeRangeAttack";
    private float attackTimer = 0;
    private string directionFacing = "right";
    private double closeRangeAttackAnimationLength = 0.45;
    public Rigidbody2D myRidgidBody;
    public SpriteRenderer mySprite;
    public Animator animator;
    public BoxCollider2D playerCollider;
    public CapsuleCollider2D groundCollider;
    public PolygonCollider2D closeRangeAttackCollider;
    public PlayerInformationScript playerInformationScript;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
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
            if(Input.GetKeyDown(KeyCode.W) && isGrounded){
                float scaleFactor = (float)(transform.localScale.y/4.204167);
                float scaledJumpHeight = (float)(jumpHeight * Math.Sqrt(scaleFactor));
                myRidgidBody.linearVelocityY = scaledJumpHeight;
                isGrounded = false;
                animator.SetBool("isJumping", !isGrounded);
            }

            if(isAttacking){
                attackTimer += Time.deltaTime;
                if(attackType=="closeRangeAttack"){
                    animator.SetBool("closeRangeAttacking", false);
                    if(attackTimer >= closeRangeAttackAnimationLength + attackBuffer){
                        //stop close range attack
                        isAttacking = false;
                        closeRangeAttackCollider.enabled = false;
                        attackTimer = 0;
                    }
                }
            }
            if(Input.GetMouseButtonDown(0) && !isAttacking && isGrounded){
                //initiate close range attack
                isAttacking = true;
                closeRangeAttackCollider.enabled = true;
                animator.SetBool("closeRangeAttacking", true);  
                attackType = "closeRangeAttack";
                playerInformationScript.drainPower(playerInformationScript.closeRangeAttackPowerCost);
            }
            animator.SetFloat("xVelocity", math.abs(myRidgidBody.linearVelocityX));
            animator.SetFloat("yVelocity", myRidgidBody.linearVelocityY);
        }else{
            myRidgidBody.linearVelocityX = 0;
            myRidgidBody.linearVelocityY = 0;
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
        closeRangeAttackCollider.offset = new Vector2(closeRangeAttackCollider.offset.x*-1, closeRangeAttackCollider.offset.y);
    }

    private void OnTriggerEnter2D(Collider2D collision){
        if(collision.IsTouching(groundCollider)){
            isGrounded = true;
            animator.SetBool("isJumping", !isGrounded);
            if (collision.gameObject.CompareTag("Lava")){
                playerInformationScript.health = 0;
            }
        }else if(collision.IsTouching(closeRangeAttackCollider)){
            if(collision.gameObject.CompareTag("Enemy")){
                //deal damage to enemy
            }
        }
    }
}
