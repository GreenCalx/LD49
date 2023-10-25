using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Schnibble;

[RequireComponent(typeof(PNJDialog))]
public class CinematicDialog : MonoBehaviour, IControllable
{
    public float auto_talk_time = 5f;

    private float internalTimer = 0f;

    private PNJDialog dialog;

    public bool autoDialog = false;
    public bool dialogIsOver = false;

    public UnityEvent callbackOnDialogDone;

    public void playPNJDialog()
    {
        if (autoDialog)
        StartCoroutine(autoTalk());
        else
        StartCoroutine(playerTalk());
    }

    public bool isDialogOver()
    {
        return !!dialogIsOver;
    }

    void OnDestroy()
    {
        if (!dialogIsOver && !!dialog)
            dialog.end_dialog();
        
        try{
            Access.PlayerInputsManager().player1.Detach(this as IControllable);
        } catch (NullReferenceException e) {
            this.Log(gameObject.name + " OnDestroy : NULL ref on detachable");
        }
    }

    void IControllable.ProcessInputs(InputManager currentMgr, GameInput[] Entry)
    {
        if (!dialogIsOver && (Entry[(int) PlayerInputs.InputCode.UIValidate] as GameInputButton).GetState().down)
        {
            dialog.talk();
        }
        else if ((Entry[(int) PlayerInputs.InputCode.UICancel] as GameInputButton).GetState().down)
        {
            dialog.end_dialog();
        }
    }

    // -------------------------------------
    // COROUTINES

    IEnumerator playerTalk()
    {
        UITurboAndSaves uiTurboAndSaveRef           = Access.UITurboAndSaves();
        UISpeedAndLifePool uiSpeedAndLifepoolRef    = Access.UISpeedAndLifePool();

        if (!!uiTurboAndSaveRef)
            uiTurboAndSaveRef.gameObject.SetActive(false);
        if (!!uiSpeedAndLifepoolRef)
            uiSpeedAndLifepoolRef.gameObject.SetActive(false);

        dialogIsOver = false;
        Access.Player().inputMgr.Attach(this as IControllable);
        dialog.talk();
        while (dialog.dialogIsPlaying())
        {
            yield return null;
        }

        dialogIsOver = true;
        if (callbackOnDialogDone!=null)
            callbackOnDialogDone.Invoke();
        Access.Player().inputMgr.Detach(this as IControllable);

        if (!!uiTurboAndSaveRef)
            uiTurboAndSaveRef.gameObject.SetActive(true);
        if (!!uiTurboAndSaveRef)
            uiSpeedAndLifepoolRef.gameObject.SetActive(true);
    }

    IEnumerator autoTalk()
    {
        dialogIsOver = false;
        while(dialog.talk())
        {
            while (internalTimer < auto_talk_time)
            {
                internalTimer += Time.deltaTime;
                yield return null;
                }
            internalTimer = 0f;
                }

        dialogIsOver = true;
        if (callbackOnDialogDone!=null)
            callbackOnDialogDone.Invoke();
        }

    // -------------------------------------
    // UNITY
    void Start()
            {
        dialog = GetComponent<PNJDialog>();
    }
}
