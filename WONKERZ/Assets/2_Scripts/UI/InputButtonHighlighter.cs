using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Schnibble;
using Schnibble.Managers;
using System;

namespace Wonkerz {
    public class InputButtonHighlighter : MonoBehaviour, IControllable
    {
        [Header("MAND")]
        public Image self_imgRef;
        [Header("Tweaks")]
        public PlayerInputs.InputCode key;
        public Color baseColor;
        public Color highlightColor;

        [Header("Internals")]
        public bool highlight = false;

        void Start()
        {
            Access.Player().inputMgr.Attach(this as IControllable);
        }

        void OnDestroy()
        {
            try{
                Access.PlayerInputsManager().player1.Detach(this as IControllable);
                #pragma warning disable CS0168
            } catch (NullReferenceException e) {
                this.Log(gameObject.name + " OnDestroy : NULL ref on detachable");
            }
            #pragma warning restore CS0168
        }

        void Update()
        {
            if (highlight)
            self_imgRef.color = highlightColor;
            else
            self_imgRef.color = baseColor;
        }


        void IControllable.ProcessInputs(InputManager currentMgr, GameController Entry)
        {
            GameInputButton inputButton = Entry.Get((int)key) as GameInputButton;
            if (null!=inputButton)
            {
                if (inputButton.GetState().down)
                {
                    highlight = true;
                }
                else if (inputButton.GetState().heldDown)
                {
                    highlight = true;
                }
                else if (inputButton.GetState().up)
                {
                    highlight = false;
                }
            }
        }
    }
}
