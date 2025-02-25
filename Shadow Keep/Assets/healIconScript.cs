using Unity.VisualScripting;
using UnityEngine;
using System;

public class healIconScript : MonoBehaviour
{
    public float velocity = 0.5f;
    public float lifeSpan = 3;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject player = GameObject.Find("Player");
        float scaleFactor = (float)(player.transform.localScale.x/4.204167);
        velocity *= scaleFactor;
    }

    // Update is called once per frame
    void Update()
    {
        float scaleFactor = (float)(transform.localScale.y/0.05);
        velocity = (float)(velocity * Math.Sqrt(scaleFactor));
        transform.position += new Vector3(0, velocity*Time.deltaTime, 0);

        lifeSpan -= Time.deltaTime;
        if(lifeSpan <= 0){
            Destroy(gameObject);
        }
    }
}
