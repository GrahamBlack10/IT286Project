using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerInformationScript : MonoBehaviour
{
    public Animator animator;
    public PlayerMovementScript playerMovementAndAttackScript;
    public const float maxHealth = 100;
    private float health = maxHealth;
    [SerializeField] private Image _healthBarFill;
    [SerializeField] private Image _powerBarFill;
    public const float maxPower = 100;
    private float power = maxPower;
    public float attackDamage { get; private set; } = 50;
    public int projectileDamage = 50;
    public float powerRegenPerSecond = 5;
    public bool isAlive = true;
    private const double deathAnimationTime = 1.5;
    private double count = 0;

    public int closeRangeAttackPowerCost = 5;
    public int healAbilityPowerCost = 25;
    public int longRangeAttackPowerCost = 10;
    public int doubleJumpPowerCost = 5;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(health <= 0 || power <= 0){
            setAliveToFalse();
            //add death menu to restart
            count += Time.deltaTime;
            if(count >= deathAnimationTime){
                resetScene();
            }
        }else{
            regenPower();
            updatePowerBar();
            updateHealthBar();
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
        playerMovementAndAttackScript.animator.SetTrigger("hit");
        updateHealthBar();
    }

    public void setHealth(float value){
        health = value;
        updateHealthBar();
    }
    //update health bar after taking damage
    public void updateHealthBar()
    {
        _healthBarFill.fillAmount = health / maxHealth;
        //make gradient color
        if (health > maxHealth / 2)
        {
            _healthBarFill.color = Color.green;
        }
        else if (health > maxHealth / 4)
        {
            _healthBarFill.color = Color.yellow;
        }
        else
        {
            _healthBarFill.color = Color.red;
        }
    }
    public void drainPower(float amount){
        power -= amount;
        updatePowerBar();
        // if power is less than 0, set it to 0 and set the health to 0 and update the health bar
        if (power < 0)
        {
            power = 0;
            setHealth(0);
            updateHealthBar();
        }

    }

    //update power bar after draining power
    public void updatePowerBar()
    {
        _powerBarFill.fillAmount = power / maxPower;
        // make gradient color 
        if (power > maxPower / 2)
        {
            _powerBarFill.color = Color.cyan;
        }
        else if (power > maxPower / 4)
        {
            _powerBarFill.color = Color.yellow;
        }
        else
        {
            _powerBarFill.color = Color.red;
        }


    }
    public void healPlayer(float amount){
        health += amount;
        updateHealthBar();
        if (health > maxHealth){
            health = maxHealth;
        }
    }
    void regenPower(){
        if(!playerMovementAndAttackScript.isAttacking && !playerMovementAndAttackScript.healing){
            power += powerRegenPerSecond*Time.deltaTime;
            updatePowerBar();
            if (power > maxPower){
                power = maxPower;
            }
        }
    }

    public float getAttackDamage(){
        return attackDamage;
    }

    public int getProjectileDamage(){
        return projectileDamage;
    }

    public float getHealth(){
        return health;
    }

    public float getPower(){
        return power;
    }
}
