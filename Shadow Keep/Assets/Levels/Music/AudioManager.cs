using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource musicSource;

    [Header("Music Clips")]
    public AudioClip background;             // For scenes 0-3 and 5
    public AudioClip alternateBackground;    // For scene 4 only

    public static AudioManager instance;

    private void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded; // Listen for scene changes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        musicSource.clip = background;
        musicSource.Play();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        int index = scene.buildIndex;

        // Scene 0-3 and 5 use background
        if ((index >= 0 && index <= 3) || index == 5)
        {
            if (musicSource.clip != background)
            {
                musicSource.clip = background;
                musicSource.Play();
            }
        }
        // Scene 4 uses alternate music
        else if (index == 4)
        {
            if (musicSource.clip != alternateBackground)
            {
                musicSource.clip = alternateBackground;
                musicSource.Play();
            }
        }
    }
}