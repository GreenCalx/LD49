using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace Wonkerz {
    /**
     *   Loads Hub Scene to retrieve Player/Managers then loads back current scene
     *  before deleting itself.
     *  Purpose : Test play scenes without going thru hub portal
     */

    public class TstHubHook : MonoBehaviour
    {
        [Header("Reset game progress unique events")]
        public bool resetGameProgressEvents = false;
        public bool loadDebugGameProgress = true;
        public string debugProfileName = "";

        private readonly string pivotScene = Constants.SN_TITLE;
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

            if (resetGameProgressEvents)
            resetUniqueEvents();
            if (loadDebugGameProgress)
            loadGameProgress();

            Access.managers.sceneMgr.loadScene(hookedScene, new SceneLoader.LoadParams{});

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

        private void resetUniqueEvents()
        {
            Access.managers.gameProgressSaveMgr.ResetAndSave();
        }

        private void loadGameProgress()
        {
            Access.managers.gameProgressSaveMgr.activeProfile = debugProfileName;
            Access.managers.gameProgressSaveMgr.Load();

            Access.managers.collectiblesMgr.loadJars();
        }

    }
}
