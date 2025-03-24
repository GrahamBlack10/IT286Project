using UnityEngine;
using UnityEngine.UI;

public class Abilities : MonoBehaviour
{
    [Header("Ability 1")]
    public Image abilityImage1;
    public float ability1Cooldown = 5;
    bool isCooldown1 = false;
    public KeyCode ability1;

    [Header("Ability 2")]
    public Image abilityImage2;
    public float ability2Cooldown = 5;
    bool isCooldown2 = false;
    public KeyCode ability2;

    [Header("Ability 3")]
    public Image abilityImage3;
    public float ability3Cooldown = 5;
    bool isCooldown3 = false;
    public KeyCode ability3;

    // Start is called before the first frame update
    void Start()
    {
        abilityImage1.fillAmount = 0;
        abilityImage2.fillAmount = 0;
        abilityImage3.fillAmount = 0;
    }

    // Update is called once per frame
    void Update ()
    {
        Ability1();
        Ability2();
        Ability3();
    }
    void Ability1()
    {
        if (Input.GetKey(ability1) && isCooldown1 == false)
        {
            isCooldown1 = true;
            abilityImage1.fillAmount = 1;
        }
        if (isCooldown1)
        {
            abilityImage1.fillAmount -= 1 / ability1Cooldown * Time.deltaTime;

            if (abilityImage1.fillAmount <= 0)
            {
                abilityImage1.fillAmount = 0;
                isCooldown1 = false;
            }
        }
    }
    void Ability2()
    {
        if (Input.GetKey(ability2) && isCooldown2 == false)
        {
            isCooldown2 = true;
            abilityImage2.fillAmount = 1;
        }
        if (isCooldown2)
        {
            abilityImage2.fillAmount -= 1 / ability2Cooldown * Time.deltaTime;

            if (abilityImage2.fillAmount <= 0)
            {
                abilityImage2.fillAmount = 0;
                isCooldown2 = false;
            }
        }
    }
    void Ability3()
    {
        if (Input.GetKey(ability3) && isCooldown3 == false)
        {
            isCooldown3 = true;
            abilityImage3.fillAmount = 1;
        }
        if (isCooldown3)
        {
            abilityImage3.fillAmount -= 1 / ability3Cooldown * Time.deltaTime;

            if (abilityImage3.fillAmount <= 0)
            {
                abilityImage3.fillAmount = 0;
                isCooldown3 = false;
            }
        }
    }
}
