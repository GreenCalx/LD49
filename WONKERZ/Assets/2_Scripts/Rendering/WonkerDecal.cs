using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schnibble;
using Schnibble.Rendering;


public class WonkerDecal : MonoBehaviour
{

    public DecalRenderer decalRenderer;
    private CameraManager CM;

    void Start()
    {
        CM = Access.CameraManager();
        if (decalRenderer.cam==null)
        {
            decalRenderer.SetCamera(CM.active_camera?.cam);
        }
    }

    public void SetAnimationTime(float t)
    {
        decalRenderer.decalMat.SetFloat("_AnimationTime", t);
    }

    void Update()
    {
        if (decalRenderer.cam!=CM.active_camera.cam)
        {
            this.Log("cam change on decal");
            decalRenderer.SetCamera(CM.active_camera.cam);
        }

    }
}
