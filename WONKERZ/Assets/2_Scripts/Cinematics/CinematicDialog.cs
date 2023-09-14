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
        Utils.detachControllable<CinematicDialog>(this);
    }

    void IControllable.ProcessInputs(InputManager currentMgr, GameInput[] Entry)
    {
        if (!dialogIsOver && (Entry[(int) PlayerInputs.InputCode.Jump] as GameInputButton).GetState().down)
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
        Utils.attachControllable<CinematicDialog>(this);
        dialog.talk();
        while (dialog.dialogIsPlaying())
        {
            yield return null;
        }

        dialogIsOver = true;
        if (callbackOnDialogDone!=null)
            callbackOnDialogDone.Invoke();
        Utils.detachControllable<CinematicDialog>(this);

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
