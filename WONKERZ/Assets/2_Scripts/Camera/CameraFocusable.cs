using Schnibble;
using UnityEngine;
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
        subToManager();
    }

    void OnEnable()
    {
        subToManager();        
    }

    void OnDisable()
    {
        unsubToManager();
    }

    void Update()
    {
        // if (!susbscribedToCamMgr)
        //     subToManager();
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
        if (cameraManager==null)
            cameraManager = Access.CameraManager();
        if (!!cameraManager)
        {
            cameraManager.addFocusable(this);
            susbscribedToCamMgr = true;
        }
    }

    private void unsubToManager()
    {
        // IMPORTANT toffa: unsubToManager is called OnDisable,
        // but we cannot use GameObject.Find in this function.
        // As Access will try to find the object, simply report that it is
        // weird to unsub with a cameraManager == null as it means we never
        // successfully made the subToManager.
        //if (cameraManager == null)
        //    cameraManager = Access.CameraManager();
        if (cameraManager == null)
            this.LogError("Trying to unsub from a null CameraManager. For now (03/25/24) it is only happenning in debug builds, using TSTHook.\n If it happens in release build this is a bug!");

        if (!!cameraManager)
        {
            cameraManager.removeFocusable(this);
            susbscribedToCamMgr = false;
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
        if (!!UIFocusAction_Inst)
            Destroy(UIFocusAction_Inst.gameObject);
    }

}
