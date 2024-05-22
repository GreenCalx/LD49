using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schnibble;
using Schnibble.Rendering;

namespace Wonkerz
{


    public class WonkerDecal : MonoBehaviour
    {
        [Header("# WonkerDecal")]
        public DecalRenderer decalRenderer;
        private CameraManager CM;

        void Start()
        {
            CM = Access.CameraManager();
            if (decalRenderer.cam == null)
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
            // TODO : Subscribe to cam manager to get updated
            if (CM != null && decalRenderer.cam != CM.active_camera.cam)
            {
                this.Log("cam change on decal");
                decalRenderer.SetCamera(CM.active_camera.cam);
            }

        }
    }
}
