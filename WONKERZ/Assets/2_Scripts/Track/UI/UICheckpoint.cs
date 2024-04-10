using UnityEngine;
using UnityEngine.UI;

namespace Wonkerz
{

    public class UICheckpoint : MonoBehaviour
    {
        public GameObject globalShowHideHandle;
        public GameObject cpNotifShowHideHandle;

        public TMPro.TextMeshProUGUI cp_text;
        public TMPro.TextMeshProUGUI cp_name_textval;
        public TMPro.TextMeshProUGUI cp_time_textval;

        public TMPro.TextMeshProUGUI convertTxt;

        public Image pompistImage;
        public TMPro.TextMeshProUGUI idOfLastCPTriggered;

        public float FULL_DURATION = 3f;
        public float CP_NOTIF_DURATION = 1.5f;

        private bool is_enabled = false;
        private float display_start_time;


        void Start()
        {
            disable();
        }

        public void disable()
        {
            globalShowHideHandle.SetActive(false);
            is_enabled = false;
        }

        public void enable()
        {
            globalShowHideHandle.SetActive(true);
            cpNotifShowHideHandle.SetActive(true);
            is_enabled = true;
        }

        public void disable_cpinfo()
        {
            cpNotifShowHideHandle.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {
            if (is_enabled)
            {
                if ((Time.time - display_start_time) >= CP_NOTIF_DURATION)
                disable_cpinfo();
                if ((Time.time - display_start_time) >= FULL_DURATION)
                disable();
            }
        }

        public void displayCP(CheckPoint iCP)
        {
            idOfLastCPTriggered.text = iCP.id.ToString();
            int racetime_val_min = (int)(Access.TrackManager().track_score.track_time / 60);
            int racetime_val_sec = (int)(Access.TrackManager().track_score.track_time % 60);
            cp_name_textval.SetText(iCP.checkpoint_name);
            cp_time_textval.SetText(racetime_val_min.ToString() + " m " + racetime_val_sec.ToString() + " s");
            enable();
            display_start_time = Time.time;
        }
    }
}
