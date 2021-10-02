using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreen : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        bool key_pressed = Input.anyKeyDown;
        if (key_pressed)
            SceneManager.LoadScene(Constants.SN_MAINGAME, LoadSceneMode.Single);     
    }
}
