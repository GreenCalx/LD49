using System.Collections;
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
        if (gos.Length > 1)
        { // duplicate when back to hooked scene
            Destroy(gameObject);
            return;
        }

        hookedScene = SceneManager.GetActiveScene().name;
        // Keep this object alive until the end of the process
        DontDestroyOnLoad(gameObject);
        // Load hub
        SceneManager.LoadScene(pivotScene, LoadSceneMode.Single);
        // wait for the hub to be loaded before loading back the hook
        StartCoroutine(loadPivot());
        // kill self when hook is loaded back
        StartCoroutine(killWhenBackToHook());
    }
    IEnumerator loadPivot()
    {
        while (SceneManager.GetActiveScene().name != pivotScene)
        { yield return null; }
        Access.SceneLoader().loadScene(hookedScene);

        killMe = true;
    }

    IEnumerator killWhenBackToHook()
    {
        while (!killMe)
        { yield return null; }

        while (SceneManager.GetActiveScene().name != hookedScene)
        { yield return null; }

        while (!SceneManager.GetActiveScene().isLoaded)
        { yield return null; }

        Destroy(gameObject);
    }

}
