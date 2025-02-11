using UnityEngine;

public class cameraScript : MonoBehaviour
{
    Camera mainCamera;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mainCamera = Camera.main;
        mainCamera.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
