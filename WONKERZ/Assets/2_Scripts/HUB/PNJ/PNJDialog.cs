using UnityEngine;

public class PNJDialog : MonoBehaviour
{
    // ID of the dialog to load ( cf. DialogBank )
    public int dialog_id;
    // name to display in the dialog UI header
    public string npc_name;
    // UI to load to show dialog
    public GameObject dialogUI;
    // SFX dialog to play when talking
    public AudioClip[] voices;
    private AudioSource __audio_source;

    private bool is_talkable;
    public bool dialog_ongoing;
    private string[] dialog;
    private int curr_dialog_index;
    private UIDialog __loaded_dialog_ui;

    private Animator __animator;
    private const string __animator_talk_parm = "talk";

    // Start is called before the first frame update
    void Start()
    {
        is_talkable = false;
        dialog_ongoing = false;
        dialog = DialogBank.load(dialog_id);

        __audio_source = GetComponent<AudioSource>();

        __animator = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public bool talk()
    {
        if (!dialog_ongoing)
        {
            dialog_ongoing = true;
            GameObject ui_go = Instantiate(dialogUI);
            __loaded_dialog_ui = ui_go.GetComponent<UIDialog>();
            curr_dialog_index = 0;
            if (!!__animator)
            __animator.SetBool(__animator_talk_parm, true);
        }

        if (__loaded_dialog_ui == null)
        return false;

        if (!__loaded_dialog_ui.message_is_displayed() &&
            __loaded_dialog_ui.has_a_message_to_display())
        __loaded_dialog_ui.force_display();
        else
        {
            if (__loaded_dialog_ui.overflows)
            {
                __loaded_dialog_ui.display(npc_name, __loaded_dialog_ui.overflowing_text);
            }
            else
            {
                if (curr_dialog_index >= dialog.Length)
                {
                    end_dialog();
                    return false;
                }

                __loaded_dialog_ui.display(npc_name, dialog[curr_dialog_index]);
                playVoice();
                curr_dialog_index++;
            }

        }
        return true;
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

    public bool dialogIsPlaying()
    {
        return dialog_ongoing;
    }

    public void end_dialog()
    {
        dialog_ongoing = false;
        Destroy(__loaded_dialog_ui.gameObject);
        curr_dialog_index = 0;

        if (!!__animator)
        __animator.SetBool(__animator_talk_parm, false);
    }

}
