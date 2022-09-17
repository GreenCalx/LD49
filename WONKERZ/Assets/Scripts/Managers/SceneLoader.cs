using System.Collections.Generic;
using System.Collections; // IEnumerator
using UnityEngine;
using UnityEngine.SceneManagement;

/**
*
* SceneManager's role :
*   - Async Load of scenes
*   - Automatically loads an intermediate loading screen inbetween scenes
*
*/
public class SceneLoader : MonoBehaviour
{
    public bool operationFinished { get; private set; }
    public float operationProgress { get; private set; }

    private Queue<IEnumerator> q;
    private int n_tasks;

    IEnumerator runningCoroutine = null;

    void Start()
    {
        operationFinished = false;
        operationProgress = 0f;
        n_tasks = 1;

        q = new Queue<IEnumerator>();
        runningCoroutine = null;
    }


    private void updateProgress(float iTaskProgress)
    {
        if (n_tasks==0)
        { Debug.LogWarning("No tasks running to update progress."); return; }

        int nCompletedTasks = n_tasks - q.Count;
        operationProgress = (nCompletedTasks * (1f/n_tasks)) + iTaskProgress/n_tasks;
    }

    public void loadScene(string iSceneName)
    {
        Debug.Log("Loading scene : " + iSceneName);
        Scene currentScene = SceneManager.GetActiveScene();
        ///
        q.Enqueue(loading(Constants.SN_LOADING));
        q.Enqueue(unloading(currentScene.name));
        q.Enqueue(loading(iSceneName));
        q.Enqueue(unloading(Constants.SN_LOADING));
        
        n_tasks = q.Count;
        //
        operationFinished = false;
        operationProgress = 0f;
        StartCoroutine(coordinator());
    }

    IEnumerator coordinator()
    {
        while(q.Count > 0)
        {
            if (runningCoroutine!=null)
            {
                yield return null;
            }
            runningCoroutine = q.Dequeue();
            yield return StartCoroutine(runningCoroutine);
        }

        operationFinished = true;
        operationProgress = 1f;

        Access.invalidate();
    }


    IEnumerator unloading(string iSceneName)
    {
        AsyncOperation old_scene = SceneManager.UnloadSceneAsync(iSceneName);
        while (!old_scene.isDone)
        {
            updateProgress(old_scene.progress);
            yield return null;
        }
        runningCoroutine = null;
    }

    IEnumerator loading(string iSceneName)
    {
        AsyncOperation scene = SceneManager.LoadSceneAsync( iSceneName, LoadSceneMode.Additive);
        if (scene==null)
        {
            Debug.LogError(">>>> Missing scene in Build Settings <<<<");
        }
        scene.allowSceneActivation = false;

        while (!scene.isDone)
        {
            updateProgress(scene.progress);
            if (scene.progress >= 0.9f)
            {
                scene.allowSceneActivation = true;
            }
            yield return null;
        }
        
        // Finished loading new scene
        Debug.Log( iSceneName + " Scene loaded.");
        enableScene(iSceneName);

        runningCoroutine = null;
    }
    
    ///////////////////////////////////////////////////////////
    private void enableScene(string iSceneName)
    {
        //Activate the Scene
        Scene sceneToLoad = SceneManager.GetSceneByName(iSceneName);
        if (sceneToLoad.IsValid())
        {
            SceneManager.MoveGameObjectToScene( Access.Player().transform.root.gameObject, 
                                                sceneToLoad );
            bool activeSceneChanged = SceneManager.SetActiveScene(sceneToLoad);
            if (!activeSceneChanged)
                Debug.LogError("Failed to changed active scene for : " + sceneToLoad.name);
            
        }
    }

}