using Schnibble;
using UnityEngine;
using UnityEngine.Events;

namespace Wonkerz
{

    public class CameraFocusable : MonoBehaviour
    {
        [Header("Tweaks")]
        public bool enableFocusActions = true;
        public Color focusColor;
        public float focusFindRange = 50f;
        public bool forceFocus = false;

        [Header("Optional - Action On Focus")]
        public string initActionName = "action";
        private string loc_actionName = "";
        public string actionName
        {
            get { return loc_actionName; }
            set { 
                loc_actionName = value; 
                if (UIFocusAction_Inst!=null)
                UIFocusAction_Inst.actionName = value;
                }
        }

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
            actionName = initActionName;

        }

        void OnEnable()
        {

        }

        void OnDisable()
        {

        }

        void Update()
        {

            if (forceFocus && !isFocus)
            {
                OnPlayerFocus();
                PlayerCamera pCam = Access.CameraManager().active_camera.GetComponent<PlayerCamera>();
                pCam?.SetSecondaryFocus(this, true);
            }
        }

        void OnDestroy()
        {

        }


        public void OnPlayerFocus()
        {
            if (!enableFocusActions)
            return;

            if (isFocus)
            return;

            callbackOnFocus?.Invoke();
            isFocus = true;

            if (UIFocusAction_Ref != null)
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
}
