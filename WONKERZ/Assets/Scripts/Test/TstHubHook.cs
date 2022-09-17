using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/**
*   Loads Hub Scene to retrieve Player/Managers then loads back current scene
*  before deleting itself.
*  Purpose : Test play scenes without going thru hub portal
*/
public class TstHubHook : MonoBehaviour
{
    private readonly string pivotScene = Constants.SN_HUB;
    private string hookedScene = "";
    private bool killMe = false;
    // Start is called before the first frame update
    void Awake()
    {
        killMe = false;

        GameObject[] gos = GameObject.FindGameObjectsWithTag("TstHubHook");
        if (gos.Length>1)
        {
            Destroy(gameObject);
            return;
        }

        hookedScene = SceneManager.GetActiveScene().name;
        // Keep this object alive until the end of the process
        DontDestroyOnLoad(gameObject);
        // Load hub
        SceneManager.LoadScene( pivotScene, LoadSceneMode.Single);
        // wait for scene loader object to exist then load back the hooked scene
        StartCoroutine(loadBack());
    }
    IEnumerator loadBack()
    {
        while(SceneManager.GetActiveScene().name!=pivotScene)
        { yield return null; }
        Access.SceneLoader().loadScene(hookedScene);

        killMe = true;
    }

    IEnumerator killWhenBackToHook()
    {
        while(!killMe)
        { yield return null; }

        Scene s = SceneManager.GetActiveScene();
        while(s.name!=hookedScene)
        { yield return null; }
        while(!s.isLoaded)
        { yield return null; }

        Destroy(gameObject);
    }

}
