using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using Schnibble;
using Schnibble.Managers;

namespace Wonkerz {
    [RequireComponent(typeof(UIDialog))]
    public class UIDialogController : MonoBehaviour, IControllable
    {
        public bool autoTalk = false;
        public float auto_talk_time = 5f;
        private UnityEvent callbackOnDialogDone;
        private float internalTimer = 0f;
        public UIDialog dialog;
        private int curr_dialog_index;
        private string[] loadedDialog;
        private bool dialog_ongoing;
        public string headerText;

        void Awake()
        {
            dialog = GetComponent<UIDialog>();
            dialog.displayHeader(headerText);
            curr_dialog_index = 0;
        }
        void IControllable.ProcessInputs(InputManager currentMgr, GameController Entry)
        {
            if ((Entry.Get((int)PlayerInputs.InputCode.UIValidate) as GameInputButton).GetState().down)
            {
                PlayDialog();
            }
            if ((Entry.Get((int)PlayerInputs.InputCode.UICancel) as GameInputButton).GetState().down)
            {
                dialog_ongoing = false;
            }
        }

        public void SetDialogCallback(UnityEvent iCallback)
        {
            callbackOnDialogDone = iCallback;
        }

        public void LaunchDialog(string iNPCName, int iDialogID)
        {
            loadedDialog = DialogBank.load(iNPCName, Utils.GetCurrentSceneName(), iDialogID);

            Access.Player().inputMgr.Attach(this as IControllable);
            Access.Player().Freeze();
            dialog_ongoing = true;
            if (!autoTalk)
            StartCoroutine(PlayerTalkCo());
            else
            StartCoroutine(AutoTalkCo());
        }

        public bool PlayDialog()
        {
            if (!dialog.message_is_displayed() && dialog.has_a_message_to_display())
            dialog.force_display();
            else
            {
                if (dialog.overflows)
                {
                    dialog.display(headerText, dialog.overflowing_text);
                }
                else
                {
                    if (curr_dialog_index >= loadedDialog.Length)
                    {
                        dialog_ongoing = false;
                        return false;
                    }

                    dialog.display(headerText, loadedDialog[curr_dialog_index]);
                    //playVoice();
                    curr_dialog_index++;
                }

            }
            return true;
        }

        public void EndDialog()
        {
            Access.PlayerInputsManager().player1.Detach(this as IControllable);
            Access.Player().UnFreeze();
            callbackOnDialogDone.Invoke();
            Destroy(gameObject);
        }

        IEnumerator PlayerTalkCo()
        {
            UISpeedAndLifePool uiSpeedAndLifepoolRef    = Access.UISpeedAndLifePool();

            if (!!uiSpeedAndLifepoolRef)
            uiSpeedAndLifepoolRef.gameObject.SetActive(false);

            PlayDialog();
            while (dialog_ongoing)
            {
                yield return null;
            }
            EndDialog();

            if (!!uiSpeedAndLifepoolRef)
            uiSpeedAndLifepoolRef.gameObject.SetActive(true);
        }

        IEnumerator AutoTalkCo()
        {
            while(PlayDialog())
            {
                while (internalTimer < auto_talk_time)
                {
                    internalTimer += Time.deltaTime;
                    yield return null;
                }
                internalTimer = 0f;
            }
            EndDialog();

            if (callbackOnDialogDone!=null)
            callbackOnDialogDone.Invoke();
        }

    }}
