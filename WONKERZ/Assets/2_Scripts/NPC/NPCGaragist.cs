using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wonkerz
{

    public class NPCGaragist : NPCDialog
    {
        [Header("# NPCGaragist")]
        public int dialogIDAfterFreed = 1;
        public CameraFocusable FreeCFocusAction;
        public bool isFree;

        void Start()
        {

        }

        public void setFree()
        {
            FreeCFocusAction.gameObject.SetActive(false);
            cameraFocusable.gameObject.SetActive(true);

            dialog_id = dialogIDAfterFreed;
            OnFree();

            isFree = true;
        }

        public void setJailed()
        {
            cameraFocusable.gameObject.SetActive(false);
            FreeCFocusAction.gameObject.SetActive(true);

            isFree = false;
        }

        public virtual void OnFree() { }

    }
}
