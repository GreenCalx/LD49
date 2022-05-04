using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreen : MonoBehaviour
{
    public float CLICK_TIME = 0.8f;
    private float start_time;

    // Start is called before the first frame update
    void Start()
    {
        start_time = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        bool key_pressed = Input.anyKeyDown;
        double time_offset = Time.time - start_time;
        //if ( Input.GetKey(KeyCode.Escape) )
        {
            Application.Quit();
        }
        //else if (key_pressed && (time_offset>=CLICK_TIME) )
            SceneManager.LoadScene(Constants.SN_HUB, LoadSceneMode.Single);

    }
}
