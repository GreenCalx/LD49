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

        public Color jumpColor;
        public Color latencyColor;
        public Color maxColor;
        public float latencyBlinkTime = 0.1f;

        void Start()
        {
            CM = Access.CameraManager();
            if (decalRenderer.cam == null)
            {
                decalRenderer.SetCamera(CM.active_camera?.cam);
            }
        }

        public void SetJumpTime(float t) {
            decalRenderer.decalMat.color = jumpColor;
            decalRenderer.decalMat.SetColor("_ColorMaxTime", maxColor);
            SetAnimationTime(t);
        }

        public void SetLatencyTime(float t) {
            latencyColor.a = Mathf.Floor((t % latencyBlinkTime) / (latencyBlinkTime != 0.0f ? (latencyBlinkTime * 0.5f) : 1.0f));
            decalRenderer.decalMat.color = latencyColor;
            decalRenderer.decalMat.SetColor("_ColorMaxTime" ,latencyColor);
            SetAnimationTime(t);
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
