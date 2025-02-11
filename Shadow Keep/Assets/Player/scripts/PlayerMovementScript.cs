using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

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
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.A)){
            myRidgidBody.linearVelocityX = -horizontalMovementSpeed;
            if(directionFacing == "right" && !isAttacking){
                mySprite.flipX = true;
                directionFacing = "left";
            }
        }else if(Input.GetKey(KeyCode.D)){
            myRidgidBody.linearVelocityX = horizontalMovementSpeed;
            if(directionFacing == "left"  && !isAttacking){
                mySprite.flipX = false;
                directionFacing = "right";
            }
        }else{
            myRidgidBody.linearVelocityX = 0;
        }
        if(Input.GetKeyDown(KeyCode.W) && isGrounded){
            Vector3 currentScale = transform.localScale; 
            myRidgidBody.linearVelocityY = jumpHeight;
            isGrounded = false;
            animator.SetBool("isJumping", !isGrounded);
        }

        if(isAttacking){
            attackTimer += Time.deltaTime;
            if(attackType=="closeRangeAttack"){
                animator.SetBool("closeRangeAttacking", false);
                if(attackTimer >= closeRangeAttackAnimationLength + attackBuffer){
                    isAttacking = false;
                    attackTimer = 0;
                }
            }
        }
        if(Input.GetMouseButtonDown(0) && !isAttacking && isGrounded){
            isAttacking = true;
            animator.SetBool("closeRangeAttacking", true);  
            attackType = "closeRangeAttack";
        }
        animator.SetFloat("xVelocity", math.abs(myRidgidBody.linearVelocityX));
        animator.SetFloat("yVelocity", myRidgidBody.linearVelocityY);
    }

    private void OnTriggerEnter2D(Collider2D collision){
        isGrounded = true;
        animator.SetBool("isJumping", !isGrounded);
    }
}
