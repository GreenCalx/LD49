using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(PNJDialog))]
public class CinematicDialog : MonoBehaviour
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

    // -------------------------------------
    // COROUTINES

    IEnumerator playerTalk()
    {
        dialogIsOver = false;

        while (dialog.dialogIsPlaying())
        {
            yield return null;
        }

        dialogIsOver = true;
        if (callbackOnDialogDone!=null)
            callbackOnDialogDone.Invoke();
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

        while (dialog.dialogIsPlaying())
        {
            yield return null;
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
