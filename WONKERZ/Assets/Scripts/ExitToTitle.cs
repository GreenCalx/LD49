using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitToTitle : MonoBehaviour
{
    public bool enabler;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (enabler)
        {
            //if ( Input.GetKey(KeyCode.Escape) )
            {
                SceneManager.LoadScene(Constants.SN_TITLE, LoadSceneMode.Single); 
            }
        }
    }
}
