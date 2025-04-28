using UnityEngine;

public class swordStrikeParticleScript : MonoBehaviour
{
    public ParticleSystem swordStrikeParticles;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void playSwordStrikeParticleEffect()
    {
        swordStrikeParticles.Play();
    }

    public void setParticleStartSizeMultiplier(float multiple){
        var main = swordStrikeParticles.main;
        main.startSize = 1.0f * multiple;
    }

}
