using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameIsWin : MonoBehaviour
{
    public float CLICK_TIME = 1f;
    private float start_time;

    public TMPro.TextMeshProUGUI racetime_txt;
    public TMPro.TextMeshProUGUI pb_txt;

    // Start is called before the first frame update
    void Start()
    {
        start_time = Time.time;
        publishTime();
    }

    // Update is called once per frame
    void Update()
    {
        bool key_pressed = Input.anyKeyDown;
        double time_offset = Time.time - start_time;
        if (key_pressed && (time_offset>=CLICK_TIME) )
        {
            SceneManager.LoadScene(Constants.SN_TITLE, LoadSceneMode.Single);
            PlayerPrefs.SetString("racetime","0");
        }
    }

    public void publishTime()
    {
            int racetime = PlayerPrefs.GetInt("racetime");
            int pb = PlayerPrefs.GetInt("pb", 99999);
            int racetime_val_min = (int)(racetime / 60);
            int racetime_val_sec = (int)(racetime % 60);

            racetime_txt.SetText(racetime_val_min.ToString() + " m " + racetime_val_sec.ToString() + " s " );

            if (pb > racetime)
            {
                PlayerPrefs.SetInt("pb",racetime);
                PlayerPrefs.Save();
            }
            if (!!pb_txt)
            {
                pb = PlayerPrefs.GetInt("pb", 0);

                int pb_val_min = (int)(pb / 60);
                int pb_val_sec = (int)(pb % 60);
                pb_txt.SetText(pb_val_min.ToString() + " m " + pb_val_sec.ToString() + " s " );
            }
    }
}
