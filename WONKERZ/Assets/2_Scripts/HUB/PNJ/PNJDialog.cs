using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Schnibble;
using Schnibble.Managers;

public class PNJDialog : WkzNPC
{
    [Header("PNJDialog")]
    // ID of the dialog to load ( cf. DialogBank )
    public int dialog_id;

    public List<int> next_dialog_ids = new List<int>();
    public bool cycleDialogs = false;

    // UI to load to show dialog
    public GameObject dialogUI;
    // SFX dialog to play when talking
    public AudioClip[] voices;
    private AudioSource __audio_source;

    private string[] dialog;
    private UIDialog __loaded_dialog_ui;
    private UIDialogController dialogController;

    private int dialog_id_index;

    // Start is called before the first frame update
    void Start()
    {
        dialog_id_index = 0;
        if (next_dialog_ids.Count>0)
        {
            next_dialog_ids.Insert(0, dialog_id);
        }

        dialog = DialogBank.load(dialog_id);

        __audio_source = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void setDialogID(int iDialogID)
    {
        dialog_id = iDialogID;
    }

    public void StartTalk()
    {
        if (dialogController!=null)
            return;

        GameObject ui_go = Instantiate(dialogUI);
        dialogController = ui_go.GetComponent<UIDialogController>();
        dialogController.headerText = npc_name;

        UnityEvent cb = new UnityEvent();
        cb.AddListener(EndTalk);

        dialogController.SetDialogCallback( cb );
        dialogController.LaunchDialog(dialog_id);
    }

    public void EndTalk()
    {    
        // if (!!dialogUI)
        //     Destroy(dialogUI.gameObject);
        
        if (next_dialog_ids.Count > 0)
        {
            UpdateDialogID();
        }
    }

    private void playVoice()
    {
        if ((voices != null) && (voices.Length > 0))
        {
            var rand = new System.Random();
            int voice_to_play = rand.Next(0, voices.Length);
            __audio_source.clip = voices[voice_to_play];
            __audio_source.Play();
        }
    }

    private void UpdateDialogID()
    {
        int n_ids = next_dialog_ids.Count;
        dialog_id_index++;
        if ( dialog_id_index > n_ids)
        { // At last dialog id defined
            if (cycleDialogs)
                dialog_id_index = 0;
            else
                return;
        }

        dialog_id = next_dialog_ids[dialog_id_index];
        dialog = DialogBank.load(dialog_id);
    }

}
