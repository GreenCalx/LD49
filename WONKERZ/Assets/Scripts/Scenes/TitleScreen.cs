using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreen : MonoBehaviour, IControllable
{
    public float CLICK_TIME = 0.8f;
    private float start_time;
    private float time_offset;

    // Start is called before the first frame update
    void Start()
    {
        start_time = Time.time;
        Utils.attachControllable<TitleScreen>(this);
    }

    void OnDestroy() {
        Utils.detachControllable<TitleScreen>(this);
    }

    // Update is called once per frame
    void Update()
    {
        time_offset = Time.time - start_time;
    }

    void IControllable.ProcessInputs(InputManager.InputData Entry)
    {
        if (Entry.Inputs["Cancel"].IsDown)
        {
            Application.Quit();
        }
        else if (Entry.Inputs["Jump"].IsDown && (time_offset>=CLICK_TIME) )
        {
            SceneManager.LoadScene(Constants.SN_HUB, LoadSceneMode.Single);
        }
    }
}
