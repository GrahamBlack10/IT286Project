using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public static SceneController instance;

    // When the game starts, check if there is already a SceneController
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            Destroy(gameObject);
        }
       
    }
    // Load the next level
    public void NextLevel()
    {
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1);
    }

    // Load a specific scene
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadSceneAsync(sceneName);
    }
}
// https://www.youtube.com/watch?v=E25JWfeCFPA&list=LL&index=6 used for reference
