using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wonkerz
{

    public class CinematicAutoFocus : MonoBehaviour
    {
        public CameraFocusable camFocus;
        public bool showFocus = false;

        private bool isOn = false;

        // Start is called before the first frame update
        public void Launch()
        {
            PlayerCamera cam = Access.CameraManager()?.active_camera as PlayerCamera;

            camFocus.enableFocusActions = false;
            cam.SetSecondaryFocus(camFocus, showFocus);

            isOn = true;
        }

        public void Stop()
        {
            PlayerCamera cam = Access.CameraManager()?.active_camera as PlayerCamera;
            cam.resetFocus();

            camFocus.enableFocusActions = true;

            isOn = false;
        }

        void OnDestroy()
        {
            if (isOn)
            Stop();
        }
    }
}
