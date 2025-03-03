using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    private GameObject[] enemies; // Array to hold all enemies
    public GameObject FinishPoint; // Assign the portal prefab in the Inspector

    void Start()
    {
        // Find all enemies in the scene
        enemies = GameObject.FindGameObjectsWithTag("Enemy");
        FinishPoint.SetActive(false); // Hide the portal at the start
    }

    void Update()
    {
        if (AllEnemiesDefeated())
        {
            FinishPoint.SetActive(true); // Show the FinishPoint portal
        }
    }

    bool AllEnemiesDefeated()
    {
        foreach (GameObject enemy in enemies)
        {
            if (enemy != null) return false; // If any enemy still exists, return false
        }
        return true; // All enemies are defeated
    }
}
// Code found from multiple sources from youtube tutorials and unity documentation