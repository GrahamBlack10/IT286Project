using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerInformationScript : MonoBehaviour
{
    public const float maxHealth = 100;
    public float health = maxHealth;
    public const float maxPower = 100;
    public float power = maxPower;
    public bool isAlive = true;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(health <= 0){
            isAlive = false;
            //add death animation
            //add death menu to restart
            resetScene();
        }
    }

    void resetScene(){
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
