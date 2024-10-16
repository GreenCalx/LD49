using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

using Schnibble;
using Schnibble.Managers;

namespace Wonkerz {
    public class UIFocusAction : MonoBehaviour, IControllable
    {
        [Header("MAND : Self References")]
        public TextMeshProUGUI labelTxt;
        [Header("Internals - transmitted by CameraFocusable")]
        public UnityEvent action;
    

        private string loc_actionName = "";
        public string actionName
        {
            get { return loc_actionName; }
            set { loc_actionName = value; refreshLabel(); }
        }

        // Start is called before the first frame update
        void Start()
        {
            Access.Player().inputMgr.Attach(this as IControllable);
        }
        void OnDestroy()
        {
            try{
                Access.managers.playerInputsMgr.player1.Detach(this as IControllable);
                #pragma warning disable CS0168
            } catch (NullReferenceException e) {
                this.Log(gameObject.name + " OnDestroy : NULL ref on detachable");
            }
            #pragma warning restore CS0168
        }

        void IControllable.ProcessInputs(InputManager currentMgr, GameController Entry)
        {
            if ((Entry.Get((int)PlayerInputs.InputCode.UIValidate) as GameInputButton).GetState().down)
            {
                // DO ACTION
                action.Invoke();
            }
        }

        private void refreshLabel()
        {
            labelTxt.text = loc_actionName;
        }
    }
}
