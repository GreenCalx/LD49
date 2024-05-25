using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schnibble;
using Schnibble.Managers;


namespace Wonkerz {
    public class UITutorialStopWindow : MonoBehaviour, IControllable
    {
        // Start is called before the first frame update
        void Start()
        {
            Access.Player().inputMgr.Attach(this as IControllable);
            pauseGame(true);
        }

        // Update is called once per frame
        void Update()
        {

        }

        void IControllable.ProcessInputs(InputManager currentMgr, GameController Entry)
        {
            if ((Entry.Get((int)PlayerInputs.InputCode.UIValidate) as GameInputButton).GetState().down)
            {
                pauseGame(false);
                foreach (Transform child in transform)
                {
                    Destroy(child);
                }
                Destroy(gameObject);
            }
        }

        void OnDestroy()
        {
            try{
                Access.PlayerInputsManager().player1.Detach(this as IControllable);
            } catch (NullReferenceException e) {
                this.Log(gameObject.name + " OnDestroy : NULL ref on detachable");
            }
        }

        public void pauseGame(bool isPaused)
        {
            Time.timeScale = (isPaused ? 0 : 1);
            var player = Access.Player();
            if (isPaused)
            player.Freeze();
            else
            player.UnFreeze();
        }
    }
}
