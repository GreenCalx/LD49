using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitToTitle : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if ( Input.GetKey(KeyCode.Escape) )
        {
            SceneManager.LoadScene(Constants.SN_TITLE, LoadSceneMode.Single); 
        }
    }
}
