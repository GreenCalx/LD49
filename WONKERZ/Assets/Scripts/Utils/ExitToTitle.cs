using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitToTitle : MonoBehaviour, IControllable
{
    public bool enabler;
    // Start is called before the first frame update
    void Start()
    {
        Utils.attachControllable<ExitToTitle>(this);
    }

    void OnDestroy() {
        Utils.detachControllable<ExitToTitle>(this);
    }

    void IControllable.ProcessInputs(InputManager.InputData Entry){
        if (enabler)
        {
            if (Entry.Inputs["Cancel"].IsDown)
                SceneManager.LoadScene(Constants.SN_TITLE, LoadSceneMode.Single); 
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
