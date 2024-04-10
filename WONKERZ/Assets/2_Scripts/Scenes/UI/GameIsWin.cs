using UnityEngine;
using UnityEngine.SceneManagement;

namespace Wonkerz {
    public class GameIsWin : MonoBehaviour
    {
        public float CLICK_TIME = 1f;
        private float start_time;

        public TMPro.TextMeshProUGUI track_name;

        public TMPro.TextMeshProUGUI difficulty_txt;
        public TMPro.TextMeshProUGUI racetime_txt;
        public TMPro.TextMeshProUGUI pb_txt;
        public TMPro.TextMeshProUGUI trackscore_txt;
        public TMPro.TextMeshProUGUI highscore_txt;

        public TMPro.TextMeshProUGUI new_record;
        public TMPro.TextMeshProUGUI new_highscore;

        public Color easy_diff;
        public Color medium_diff;
        public Color hard_diff;
        public Color ironman_diff;

        // Start is called before the first frame update
        void Start()
        {
            start_time = Time.time;

            TrackManager tm = Access.TrackManager();

            difficulty_txt.SetText( tm.track_score.selected_diff.ToString() );

            switch ( tm.track_score.selected_diff )
            {
                case DIFFICULTIES.EASY :
                    difficulty_txt.color = easy_diff;
                    break;
                case DIFFICULTIES.MEDIUM :
                    difficulty_txt.color = medium_diff;
                    break;
                case DIFFICULTIES.HARD :
                    difficulty_txt.color = hard_diff;
                    break;
                case DIFFICULTIES.IRONMAN :
                    difficulty_txt.color = ironman_diff;
                    break;
            }
            track_name.SetText( tm.track_score.track_name );

            publishTime();
            publishScore();
        }

        // Update is called once per frame
        void Update()
        {
            bool key_pressed = Input.anyKeyDown;
            double time_offset = Time.time - start_time;
            if (key_pressed && (time_offset >= CLICK_TIME))
            {
                Access.SceneLoader().loadScene(Constants.SN_HUB);
                //PlayerPrefs.SetString("racetime", "0");
            }
        }

        public void publishTime()
        {
            double racetime     = Access.TrackManager().getRaceTime();
            double pb           = Access.TrackManager().getRacePB();
            int racetime_val_min = (int)(racetime / 60);
            int racetime_val_sec = (int)(racetime % 60);

            racetime_txt.SetText(racetime_val_min.ToString() + " m " + racetime_val_sec.ToString() + " s ");

            if (pb > racetime)
            {
                pb = racetime;
                if (!!new_record)
                {
                    new_record.gameObject.SetActive(true);
                }
                Access.TrackManager().saveRacePB();
            }
            if (!!pb_txt)
            {
                int pb_val_min = (int)(pb / 60);
                int pb_val_sec = (int)(pb % 60);
                pb_txt.SetText(pb_val_min.ToString() + " m " + pb_val_sec.ToString() + " s ");
            }
        }

        public void publishScore()
        {
            int trickscore = Access.TrackManager().getTrickScore();
            int highscore = Access.TrackManager().getRaceHighScore();

            trackscore_txt.SetText(trickscore.ToString());
            if ( trickscore > highscore )
            {
                highscore = trickscore;
                if (!!new_highscore)
                {
                    new_highscore.gameObject.SetActive(true);
                }
                Access.TrackManager().saveRaceHighScore();
            }
            highscore_txt.SetText(highscore.ToString());
        }
    }
}
