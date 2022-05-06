using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/**
* Handles objects that are not destroyed on scene change and remove duplicates of them
* > To attach on object carrying unique managers that we want to carry from scene to scene
*/
public class ManagerLifecycle : MonoBehaviour
{
    private int previousSceneIndex = -1; // no previous scene
    void Awake()
    {
        int n_manager_handler = FindObjectsOfType<ManagerLifecycle>().Length;
        if ( n_manager_handler > 1 )
        {
            Destroy(gameObject);
        } else {
            DontDestroyOnLoad(gameObject);
        }
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy ()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (previousSceneIndex==-1)
        {
            previousSceneIndex = SceneManager.GetActiveScene().buildIndex;
            return; // first scene loaded, if we detach we lost inputz
        }
        previousSceneIndex = SceneManager.GetActiveScene().buildIndex;
        
        // Do stuff on new scene load
        // ....
    }
}
