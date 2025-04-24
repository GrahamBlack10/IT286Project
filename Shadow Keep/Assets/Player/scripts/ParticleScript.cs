using UnityEngine;

public class ParticleScript : MonoBehaviour
{ 
    public ParticleSystem footstepParticles;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void playFootstepEffect()
    {
        footstepParticles.Play();
    }

    public void setParticleStartSizeMultiplier(float multiple){
        var main = footstepParticles.main;
        main.startSize = 1.0f * multiple;
    }
}
