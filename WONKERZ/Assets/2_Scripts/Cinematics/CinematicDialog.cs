using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Schnibble;
using Schnibble.Managers;

public class CinematicDialog : MonoBehaviour
{
    public string npc_name = "";
    public int dialog_id;
    public string dialogHeaderString = "";
    public GameObject dialogUI_Ref;
    private UIDialogController dialogController;
    public bool autoDialog = false;
    public float auto_talk_time = 5f;
    public UnityEvent callbackOnDialogDone;

    public void Launch()
    {
        if (dialogController!=null)
            return;

        GameObject ui_go = Instantiate(dialogUI_Ref);
        dialogController = ui_go.GetComponent<UIDialogController>();

        dialogController.headerText = dialogHeaderString;
        dialogController.SetDialogCallback( callbackOnDialogDone );
        dialogController.autoTalk = autoDialog;

        dialogController.LaunchDialog(npc_name, dialog_id);
    }

    void OnDestroy()
    {
        // if (!dialogIsOver && !!dialog)
        //     dialog.end_dialog();
    }

    // -------------------------------------
    // UNITY
    void Start()
    {
        //dialog = GetComponent<PNJDialog>();
    }
}
