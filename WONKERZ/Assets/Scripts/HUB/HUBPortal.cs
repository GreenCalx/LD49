using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HUBPortal : MonoBehaviour
{
    public string PORTAL_SCENE_TARGET = "main";
    private AsyncOperation sceneAsync;
    private GameObject currPlayer;

    private bool is_loading = false;

    // Start is called before the first frame update
    void Start()
    {
        is_loading = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerStay(Collider iCol)
    {
        CarController player = iCol.GetComponent<CarController>();
        if (!!player && !is_loading)
        {
            //SceneManager.LoadScene(PORTAL_SCENE_TARGET, LoadSceneMode.Single);
            is_loading = true;
            currPlayer = player.transform.root.gameObject;
            StartCoroutine(loading());
        }
    }

    IEnumerator loading()
    {
        AsyncOperation scene = SceneManager.LoadSceneAsync(PORTAL_SCENE_TARGET, LoadSceneMode.Additive);
        scene.allowSceneActivation = false;
        sceneAsync = scene;

        while (!scene.isDone)
        {
            Debug.Log("Loading scene " + " [][] Progress: " + scene.progress);
            if (scene.progress >= 0.9f)
            {
                scene.allowSceneActivation = true;
            }
            yield return null;
        }
        
        OnFinishedLoadingAllScene();
    }

    void enableScene()
    {
        //Activate the Scene
        //sceneAsync.allowSceneActivation = true;
        Scene currentScene = SceneManager.GetActiveScene();
        Scene sceneToLoad = SceneManager.GetSceneByName(PORTAL_SCENE_TARGET);
        if (sceneToLoad.IsValid())
        {
            Debug.Log("Scene is Valid");
            SceneManager.MoveGameObjectToScene(currPlayer, sceneToLoad);
            SceneManager.SetActiveScene(sceneToLoad);
            
            SceneManager.UnloadSceneAsync(currentScene);
            
        }
    }

    private void OnFinishedLoadingAllScene()
    {
        Debug.Log( PORTAL_SCENE_TARGET + " Scene loaded.");
        enableScene();
    }
}
