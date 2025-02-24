using UnityEngine;

// This script is used to detect when the player reaches the finish point
public class FinishPoint : MonoBehaviour
{
    [SerializeField] bool goNextLevel;
    [SerializeField] string levelName;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the player has reached the finish point
        if (collision.CompareTag("Player"))
        {
            // go to next level
            if (goNextLevel)
            {
                SceneController.instance.NextLevel();
            }
            else
            {
                SceneController.instance.LoadScene(levelName);
            }

        }
    }
}
// https://www.youtube.com/watch?v=E25JWfeCFPA&list=LL&index=6 used for reference