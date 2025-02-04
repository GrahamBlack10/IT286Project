using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovementScript : MonoBehaviour
{
    public float horizontalMovementSpeed = 10;
    public float jumpHeight = 10;
    private bool jumping = false;
    private string directionFacing = "right";
    public Rigidbody2D myRidgidBody;
    public SpriteRenderer mySprite;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.A)){
            transform.position = new Vector3(transform.position.x - horizontalMovementSpeed*Time.deltaTime, transform.position.y, transform.position.z);
            if(directionFacing == "right"){
                mySprite.flipX = true;
                directionFacing = "left";
            }
        }
        if(Input.GetKey(KeyCode.D)){
            transform.position = new Vector3(transform.position.x + horizontalMovementSpeed*Time.deltaTime, transform.position.y, transform.position.z);
            if(directionFacing == "left"){
                mySprite.flipX = false;
                directionFacing = "right";
            }
        }
        if(Input.GetKeyDown(KeyCode.W) && !jumping){
            myRidgidBody.linearVelocityY = jumpHeight;
            //jumping = true;
        }
    }
}
