using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class CameraFocusable : MonoBehaviour
{
    [Header("Tweaks")]
    public bool enableFocusActions = true;
    public Color focusColor;
    public float focusFindRange = 50f;
    [Header("Optional - Action On Focus")]
    public string actionName = "";
    public UIFocusAction UIFocusAction_Ref;
    public Vector3 screenSpaceUIOffset;
    public UnityEvent callbackOnFocus;
    public UnityEvent callbackOnAction;
    public UnityEvent callbackOnUnFocus;

    [Header("Internals")]
    public bool isFocus = false;
    private bool susbscribedToCamMgr = false;
    private CameraManager cameraManager;

    private UIFocusAction UIFocusAction_Inst;

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

    public void OnPlayerFocus()
    {
        if (!enableFocusActions)
            return;

        if (isFocus)
            return;

        callbackOnFocus?.Invoke();
        isFocus = true;
        
        if (UIFocusAction_Ref!=null)
        {
            UIFocusAction_Inst = Instantiate(UIFocusAction_Ref, Access.UISecondaryFocus().transform);

            //UIFocusAction_Inst.transform.position = transform.position;
            UIFocusAction_Inst.transform.position += screenSpaceUIOffset;

            UIFocusAction_Inst.action = callbackOnAction;
            UIFocusAction_Inst.actionName = actionName;
        }
    }
    
    public void OnPlayerUnfocus()
    {
        if (!enableFocusActions)
            return;

        if (!isFocus)
            return;

        callbackOnUnFocus?.Invoke();
        isFocus = false;
        Destroy(UIFocusAction_Inst.gameObject);
    }

}
