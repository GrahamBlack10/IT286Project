using System;
using NUnit.Framework;
using UnityEngine;

public class cameraScript : MonoBehaviour
{
    Camera mainCamera;
    GameObject player;
    public float cameraShakeDuration = 0.5f;
    public float cameraShakeMagnitude = 1f;
    private float cameraShakeCounter = 0;
    private bool isCameraShaking = false;
    private float offsetX = 0;
    private float offsetY = 0;
    private float originalYPos;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mainCamera = Camera.main;
        mainCamera.enabled = true;

        player = GameObject.Find("Player");
    }

    // Update is called once per frame
    void Update()
    {
        if(isCameraShaking){
            cameraShakeCounter += Time.deltaTime;

            offsetX = UnityEngine.Random.Range(-1f, 1f) * cameraShakeMagnitude;
            offsetY = UnityEngine.Random.Range(-1f, 1f) * cameraShakeMagnitude;

            if(cameraShakeCounter >= cameraShakeDuration){
                cameraShakeCounter = 0;
                isCameraShaking = false;
                offsetX = 0;
                offsetY = 0;
                transform.position = new Vector3(player.transform.position.x + offsetX, originalYPos, transform.position.z);
            }else{
                transform.position = new Vector3(player.transform.position.x, originalYPos + offsetX, transform.position.z);
            }
        }else{
                    transform.position = new Vector3(player.transform.position.x, transform.position.y, transform.position.z);
        }
    }

    [ContextMenu("Test Shake")]
    public void shakeCamera(){
        isCameraShaking = true;
        originalYPos = transform.position.y;
    }
}
