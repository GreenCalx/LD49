using UnityEngine;
using Schnibble;
using Schnibble.Managers;
namespace Wonkerz {

    public class TalkBox : MonoBehaviour, IControllable
    {
        [Header("MANDATORY")]
        public string npc_name;
        public int dialog_id;
        public GameObject dialogUI_ref;
        [Header("OPTIONALS")]
        public AudioClip[] voices;
        public Animator animator;
        public CinematicCamera dialogCamera;
        //////////////////////////////////
        private AudioSource audio_source;
        private UIDialog dialogUI_inst;
        private bool is_talkable;
        private bool is_in_dialog;
        private string[] loaded_dialog;
        private int curr_dialog_index;
        private const string c_animator_talk_parm = "talk";

        //////////////////////////////////

        void Start()
        {
            is_talkable = false;
            is_in_dialog = false;
            loaded_dialog = DialogBank.load(npc_name, Utils.GetCurrentSceneName() ,dialog_id);

            audio_source = GetComponent<AudioSource>();
            Access.Player().inputMgr.Attach(this as IControllable);
        }

        void OnDestroy()
        {
            Access.Player().inputMgr.Detach(this as IControllable);
        }

        void IControllable.ProcessInputs(InputManager currentMgr, GameController Entry)
        {

            if (is_talkable && (Entry.Get((int) PlayerInputs.InputCode.UIValidate) as GameInputButton).GetState().down)
            {
                talk();
            }
            else if (is_in_dialog && (Entry.Get((int) PlayerInputs.InputCode.UICancel) as GameInputButton).GetState().down)
            {
                end_dialog();
            }
        }
        void Update()
        {
        }

        void OnTriggerEnter(Collider iCol)
        {
            if (iCol.GetComponent<SchCarController>())
            {
                is_talkable = true;
            }
        }

        void OnTriggerExit(Collider iCol)
        {
            if (iCol.GetComponent<SchCarController>())
            {
                end_dialog();
                is_talkable = false;
            }
        }

        private void talk()
        {
            if (!is_in_dialog) // Start Dialog
            {
                is_in_dialog = true;
                GameObject ui_go = Instantiate(dialogUI_ref);
                dialogUI_inst = ui_go.GetComponent<UIDialog>();
                curr_dialog_index = 0;

                if (!!animator)
                animator.SetBool(c_animator_talk_parm, true);

                if (!!dialogCamera)
                dialogCamera.launch();
            }

            if (dialogUI_inst == null)
            return;

            if (!dialogUI_inst.message_is_displayed() &&
                dialogUI_inst.has_a_message_to_display())
            dialogUI_inst.force_display();
            else
            {
                if (dialogUI_inst.overflows)
                {
                    dialogUI_inst.display(npc_name, dialogUI_inst.overflowing_text);
                }
                else
                {
                    if (curr_dialog_index >= loaded_dialog.Length)
                    {
                        end_dialog();
                        return;
                    }

                    dialogUI_inst.display(npc_name, loaded_dialog[curr_dialog_index]);
                    playVoice();
                    curr_dialog_index++;
                }

            }

        }

        private void playVoice()
        {
            if ((voices != null) && (voices.Length > 0))
            {
                var rand = new System.Random();
                int voice_to_play = rand.Next(0, voices.Length);
                audio_source.clip = voices[voice_to_play];
                audio_source.Play();
            }
        }

        private void end_dialog()
        {
            is_in_dialog = false;
            if (!!dialogUI_inst)
            Destroy(dialogUI_inst.gameObject);
            curr_dialog_index = 0;

            if (!!animator)
            animator.SetBool(c_animator_talk_parm, false);

            if (!!dialogCamera)
            dialogCamera.end();
        }
    }
}
