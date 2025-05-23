using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerInformationScript : MonoBehaviour
{
    public Animator animator;
    public PlayerMovementScript playerMovementAndAttackScript;
    public SpriteRenderer spriteRenderer;
    public PlayerSoundScript playerSoundScript;
    private cameraScript cameraScript;

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

    [Header("Ability Power Costs")]
    public int closeRangeAttackPowerCost = 5;
    public int healAbilityPowerCost = 25;
    public int longRangeAttackPowerCost = 10;
    public int doubleJumpPowerCost = 5;
    public int dashPowerCost = 15; 

    [Header("Ability Cooldowns")]
    public bool closeRangeAttackUsed = false;
    public int closeRangeAttackCoolDown = 1;
    public double closeRangeTimer = 0;

    public bool longRangeAttackUsed = false;
    public int longRangeAttackCoolDown = 1;
    public double longRangeAttackTimer = 0;

    public bool healAbilityUsed = false;
    public int healAbilityCoolDown = 7;
    public double healAbilityTimer = 0;

    public bool doubleJumpUsed = false;
    public int doubleJumpCoolDown = 2;
    public double doubleJumpTimer = 0;

    public bool dashUsed = false; // Optional: for tracking if dash has been used
    public int dashCoolDown = 3;
    public double dashTimer = 0;

    void Start()
    {
        cameraScript = GameObject.Find("Main Camera").GetComponent<cameraScript>();
    }

    void Update()
    {
        if (health <= 0 || power <= 0)
        {
            setAliveToFalse();
            count += Time.deltaTime;
            if (count >= deathAnimationTime)
            {
                resetScene();
            }
        }
        else
        {
            regenPower();
            updatePowerBar();
            updateHealthBar();
        }

        // Ability cooldowns
        if (closeRangeAttackUsed)
        {
            closeRangeTimer += Time.deltaTime;
            if (closeRangeTimer >= closeRangeAttackCoolDown)
            {
                closeRangeTimer = 0;
                closeRangeAttackUsed = false;
            }
        }

        if (longRangeAttackUsed)
        {
            longRangeAttackTimer += Time.deltaTime;
            if (longRangeAttackTimer >= longRangeAttackCoolDown)
            {
                longRangeAttackTimer = 0;
                longRangeAttackUsed = false;
            }
        }

        if (healAbilityUsed)
        {
            healAbilityTimer += Time.deltaTime;
            if (healAbilityTimer >= healAbilityCoolDown)
            {
                healAbilityTimer = 0;
                healAbilityUsed = false;
            }
        }

        if (doubleJumpUsed)
        {
            doubleJumpTimer += Time.deltaTime;
            if (doubleJumpTimer >= doubleJumpCoolDown)
            {
                doubleJumpTimer = 0;
                doubleJumpUsed = false;
            }
        }

        if (dashUsed)
        {
            dashTimer += Time.deltaTime;
            if (dashTimer >= dashCoolDown)
            {
                dashTimer = 0;
                dashUsed = false;
            }
        }
    }

    void setAliveToFalse()
    {
        animator.SetBool("isAlive", false);
        isAlive = false;
    }

    void resetScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void takeDamage(float amount)
    {
        health -= amount;
        playerMovementAndAttackScript.animator.SetTrigger("hit");
        updateHealthBar();
        playerSoundScript.playGruntSound();
        cameraScript.shakeCamera();
    }

    public void setHealth(float value)
    {
        health = value;
        updateHealthBar();
    }

    public void updateHealthBar()
    {
        _healthBarFill.fillAmount = health / maxHealth;
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

    public void drainPower(float amount)
    {
        power -= amount;
        updatePowerBar();
        if (power < 0)
        {
            power = 0;
            setHealth(0);
            updateHealthBar();
        }
    }

    public void updatePowerBar()
    {
        _powerBarFill.fillAmount = power / maxPower;
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

    public void healPlayer(float amount)
    {
        health += amount;
        updateHealthBar();
        if (health > maxHealth)
        {
            health = maxHealth;
        }
    }

    void regenPower()
    {
        if (!playerMovementAndAttackScript.isAttacking && !playerMovementAndAttackScript.healing)
        {
            power += powerRegenPerSecond * Time.deltaTime;
            updatePowerBar();
            if (power > maxPower)
            {
                power = maxPower;
            }
        }
    }

    public float getAttackDamage()
    {
        return attackDamage;
    }

    public int getProjectileDamage()
    {
        return projectileDamage;
    }

    public float getHealth()
    {
        return health;
    }

    public float getPower()
    {
        return power;
    }
}
