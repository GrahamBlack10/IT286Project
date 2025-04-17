using UnityEngine;

public class PlayerSoundScript : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip jumpSoundEffect;
    public AudioClip dashSoundEffect;
    public AudioClip swordSwingSound;
    public AudioClip swordStrikeSound;
    public AudioClip healSound;
    public AudioClip footStepSound;
    public AudioClip gruntSound;
    public AudioClip slashSound;
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

    public void playDashSoundEffect(){
        audioSource.PlayOneShot(dashSoundEffect);
    }

    public void playSwordSwingSound(){
        audioSource.PlayOneShot(swordSwingSound);
    }

    public void playSwordStrikeSound(){
        audioSource.PlayOneShot(swordStrikeSound);
    }

    public void playHealSound(){
        audioSource.PlayOneShot(healSound);
    }

    public void playFootStepSound(){
        audioSource.PlayOneShot(footStepSound);
    }

    public void playGruntSound(){
        audioSource.PlayOneShot(gruntSound);
    }

    public void playSlashSound(){
        audioSource.PlayOneShot(slashSound);
    }
}
