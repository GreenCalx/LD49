using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

using Schnibble;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[ExecuteInEditMode]
public class TSTHookMgr : MonoBehaviour
{
    public TstHubHook hook;

    void Start()
    {
        SendCallbacks();
    }

    private void SendCallbacks()
    {
        UnityEditor.SceneManagement.EditorSceneManager.sceneClosed += OnSceneClose;
        UnityEditor.SceneManagement.EditorSceneManager.sceneOpened += OnSceneOpen;
    }

    void OnSceneClose(Scene scene)
    {
        this.Log("Hook deactive");
        if (hook==null)
            return;
        if (scene == hook.gameObject.scene)
        {
            hook.gameObject.SetActive(false);
            
        }
    }

    void OnSceneOpen(Scene scene, OpenSceneMode mode)
    {
        this.Log("Hook active");
        if (hook==null)
            return;
        if (scene == hook.gameObject.scene)
        {
            hook.gameObject.SetActive(true);
            
        }
    }
}
