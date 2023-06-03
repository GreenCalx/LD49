using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schnibble;


public class WonkerDecal : MonoBehaviour
{

    public DecalRenderer decalRenderer;
    public CarController cont;

    void Start()
    {
        if (decalRenderer.cam==null)
        {
            decalRenderer.SetCamera(Access.CameraManager().active_camera?.GetComponent<Camera>());
        }
    }

    public void SetAnimationTime(float t)
    {
        decalRenderer.mat.SetFloat("_AnimationTime", t);             
    }

    void Update()
    {
        if (decalRenderer.cam!=Access.CameraManager().active_camera.GetComponent<Camera>())
        {
            this.Log("cam change on decal");
            decalRenderer.SetCamera(Access.CameraManager().active_camera.GetComponent<Camera>());
        }

    }
}
