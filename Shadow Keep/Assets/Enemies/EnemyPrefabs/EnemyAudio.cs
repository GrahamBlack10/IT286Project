using UnityEngine;

public class EnemyAudio : MonoBehaviour
{
    public AudioSource audioSource;

    public AudioClip attackClip;
    public AudioClip hurtClip;
    public AudioClip deathClip;
    public AudioClip specialClip;

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    public void PlayAttack()
    {
        if (attackClip != null)
            audioSource.PlayOneShot(attackClip);
    }

    public void PlayHurt()
    {
        if (hurtClip != null)
            audioSource.PlayOneShot(hurtClip);
    }

    public void PlayDeath()
    {
        if (deathClip != null)
            audioSource.PlayOneShot(deathClip);
    }

    public void PlaySpecial()
    {
        if (specialClip != null)
            audioSource.PlayOneShot(specialClip);
    }
}
