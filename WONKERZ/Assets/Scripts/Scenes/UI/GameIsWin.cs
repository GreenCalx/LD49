using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameIsWin : MonoBehaviour
{
    public float CLICK_TIME = 1f;
    private float start_time;

    public Text racetime_txt;
    public Text pb_txt;

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
            string racetime = PlayerPrefs.GetString("racetime");
            racetime_txt.text = racetime;

            string pb = PlayerPrefs.GetString("pb", "999999");
            double pb_val = double.Parse(pb, System.Globalization.CultureInfo.InvariantCulture);
            double racetime_val = double.Parse(racetime, System.Globalization.CultureInfo.InvariantCulture);
            if (pb_val > racetime_val)
                PlayerPrefs.SetString("pb",racetime_val.ToString());
            if (!!pb_txt)
                pb_txt.text = PlayerPrefs.GetString("pb", "999999");
    }
}
