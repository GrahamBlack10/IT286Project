using UnityEngine;

public class MushroomProjectile : MonoBehaviour
{
    public int damage = 8;
    public float lifetime = 3f;
    private ParticleSystem poisonTrail;

     public int poisonDamage = 2;
     public int poisonTicks = 3;
    public float poisonInterval = 1f;

    void Start()
    {
        poisonTrail = GetComponentInChildren<ParticleSystem>();
        poisonTrail?.Play();
        Destroy(gameObject, lifetime);
    }

   void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerInformationScript player = collision.GetComponent<PlayerInformationScript>();
            if (player != null)
            {
                player.takeDamage(damage);
                Debug.Log("Player hit by mushroom projectile!");

                // Start poison effect
                StartCoroutine(ApplyPoison(player));
            }

            Destroy(gameObject);
        }
        else if (!collision.isTrigger)
        {
            Destroy(gameObject);
        }
    }

    private System.Collections.IEnumerator ApplyPoison(PlayerInformationScript player)
    {
        for (int i = 0; i < poisonTicks; i++)
        {
            yield return new WaitForSeconds(poisonInterval);
            if (player != null)
            {
                player.takeDamage(poisonDamage);
                Debug.Log("Player takes poison damage.");
            }
        }
    }
}
