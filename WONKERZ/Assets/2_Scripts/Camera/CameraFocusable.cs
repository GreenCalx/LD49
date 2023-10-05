using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraFocusable : MonoBehaviour
{
    [Header("Tweaks")]
    public Color focusColor;
    [Header("Internals")]
    private bool susbscribedToCamMgr = false;
    private CameraManager cameraManager;

    void Start()
    {
        bool susbscribedToCamMgr = false;
        subToManager();
    }

    void Update()
    {
        if (!susbscribedToCamMgr)
            return;
        subToManager();
        
    }

    void OnDestroy()
    {
        if (!!cameraManager && susbscribedToCamMgr)
        {
            cameraManager.removeFocusable(this);
        }
    }

    private void subToManager()
    {
        cameraManager = Access.CameraManager();
        if (!!cameraManager)
        {
            cameraManager.addFocusable(this);
            susbscribedToCamMgr = true;
        }
    }

}
