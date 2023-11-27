using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class InitCamera : GameCamera
{
    // Proccessed by CameraManager in the OnSceneLoaded
    public GameCamera nextCam;
    // Start is called before the first frame update
    void Start()
    {
        camType = CAM_TYPE.INIT;
        cam = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
