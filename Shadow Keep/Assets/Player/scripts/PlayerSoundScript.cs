using UnityEngine;

public class PlayerSoundScript : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip jumpSoundEffect;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void playJumpSoundEffect(){
        audioSource.PlayOneShot(jumpSoundEffect);
    }
}
