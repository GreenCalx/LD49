using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wonkerz;

[RequireComponent(typeof(Camera))]
public class InitCamera : GameCamera
{
    // Proccessed by CameraManager in the OnSceneLoaded
    public GameCamera nextCam;
    protected override void Awake()
    {
        camType = CAM_TYPE.INIT;
        cam = GetComponent<Camera>();
    }

    void OnEnable() {
        Access.GetMgr<AudioListenerManager>().SetListener(this.gameObject);
        Access.GetMgr<CameraManager>().changeCamera(nextCam, false);
    }
}
