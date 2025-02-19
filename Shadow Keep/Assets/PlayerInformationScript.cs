using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerInformationScript : MonoBehaviour
{
    public Animator animator;
    public PlayerMovementScript playerMovementAndAttackScript;
    public const float maxHealth = 100;
    private float health = maxHealth;
    public const float maxPower = 100;
    private float power = maxPower;
    public float attackDamage { get; private set; } = 50;
    public float powerRegenPerSecond = 5;
    public bool isAlive = true;
    private const double deathAnimationTime = 1.5;
    private double count = 0;

    public int closeRangeAttackPowerCost = 5;
    public int healAbilityPowerCost = 25;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        regenPower();
        if(health <= 0 || power <= 0){
            setAliveToFalse();
            //add death animation
            //add death menu to restart
            count += Time.deltaTime;
            if(count >= deathAnimationTime){
                resetScene();
            }
        }
    }

    void setAliveToFalse(){
        animator.SetBool("isAlive", false);
        isAlive = false;
    }

    void resetScene(){
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void takeDamage(float amount){
        health -= amount;
    }

    public void setHealth(float value){
        health = value;
    }

    public void drainPower(float amount){
        power -= amount;
    }

    public void healPlayer(float amount){
        health += amount;
        if(health > maxHealth){
            health = maxHealth;
        }
    }
    void regenPower(){
        if(!playerMovementAndAttackScript.isAttacking && !playerMovementAndAttackScript.healing){
            power += powerRegenPerSecond*Time.deltaTime;
            if(power > maxPower){
                power = maxPower;
            }
        }
    }

    public float getAttackDamage(){
        return attackDamage;
    }

    public float getHealth(){
        return health;
    }

    public float getPower(){
        return power;
    }
}
