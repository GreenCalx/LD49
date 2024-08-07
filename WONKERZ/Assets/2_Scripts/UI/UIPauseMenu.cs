using UnityEngine;
//using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using TMPro;
using Schnibble;
using Schnibble.UI;
using Schnibble.Managers;

namespace Wonkerz {
    // TODO : Induces input buffering (ex start jump, pause, spam jump, unpause => boom rocket jump)
    // THUS !! Player must be frozen and most likely any kind of User inputs beside this pause menu.
    public class UIPauseMenu : MonoBehaviour, IControllable
    {
        public GameObject UIHandle;
        public UISelectableElement panel;
        public UISelectableElement debugPanel;

        public enum EXITABLE_SCENES { SN_TITLE, SN_HUB };
        public EXITABLE_SCENES sceneToLoadOnExit = EXITABLE_SCENES.SN_TITLE;

        [Header("Mandatory")]
        public TextMeshProUGUI TMP_trackname;
        public UIWonkerzBar wonkerzBar;
        public TextMeshProUGUI TMP_keyObtained;
        public TextMeshProUGUI TMP_cageOpened;

        [Header("Tweaks")]
        public Color trackDetailValidated = Color.green;
        public Color trackDetailPending = Color.red;

        void Start()
        {
            Access.Player().inputMgr.Attach(this as IControllable);
        }

        void OnDestroy()
        {
            try{
                Access.PlayerInputsManager().player1.Detach(this as IControllable);
            } catch (NullReferenceException e) {
                this.Log(gameObject.name + " OnDestroy : NULL ref on detachable");
            }
        }

        void IControllable.ProcessInputs(InputManager currentMgr, GameController Entry)
        {
            if ((Entry.Get((int)PlayerInputs.InputCode.UIStart) as GameInputButton).GetState().down)
            {
                if (!!TMP_trackname)
                TMP_trackname.text = SceneManager.GetActiveScene().name;
                updateTrackDetails();

                UIHandle.SetActive(true);
                panel.inputMgr = Access.Player().inputMgr;
                //panel.inputMgr = Access.PlayerInputsManager().all;
                panel.onActivate.Invoke();
                panel.activate();
            }
        }

        public void pauseGame(bool isPaused)
        {
            Time.timeScale = (isPaused ? 0 : 1);
            var player = Access.Player();
            if (isPaused)
            player.Freeze();
            else
            player.UnFreeze();
        }

        public void OnExitButton()
        {
            panel.onDeactivate.Invoke();
            // save & exit here
            string sceneToLoad = "";
            switch (sceneToLoadOnExit)
            {
                case EXITABLE_SCENES.SN_TITLE:
                    sceneToLoad = Constants.SN_TITLE;
                    break;
                case EXITABLE_SCENES.SN_HUB:
                    sceneToLoad = Constants.SN_HUB;
                    break;
                default:
                    sceneToLoad = Constants.SN_TITLE;
                    break;
            }
            Access.SceneLoader().loadScene(sceneToLoad);
        }

        public void OnCameraToggleChange(bool value)
        {
            //Access.CameraManager().changeCamera(value ? GameCamera.CAM_TYPE.ORBIT : GameCamera.CAM_TYPE.OLD_TRACK);
            // Disable auto rot of manual camera
            ManualCamera mc = Access.CameraManager().active_camera.GetComponent<ManualCamera>();
            if (!!mc)
            {
                mc.autoAlign = value;
            }
        }

        public void GetCameraToggleValue(UICheckbox.UICheckboxValue value)
        {
            value.value = Access.CameraManager().active_camera ?
                (Access.CameraManager().active_camera.camType == GameCamera.CAM_TYPE.ORBIT) :
                false
            ;
        }

        public void updateTrackDetails()
        {
            // update collectibles
            CollectiblesManager cm = Access.CollectiblesManager();

            //collected wonkerz
            if (!!wonkerzBar)
            {
                foreach(CollectibleWONKERZ.LETTERS let in Enum.GetValues(typeof(CollectibleWONKERZ.LETTERS)))
                {
                    wonkerzBar.updateLetter(let, cm.hasWONKERZLetter(let));
                }
            }


            // key + cage status
            if (!!TMP_keyObtained && !!TMP_cageOpened)
            {
                string sceneName = SceneManager.GetActiveScene().name;
                TMP_keyObtained.color = (cm.hasCageKey(sceneName) ) ? trackDetailValidated : trackDetailPending;
                TMP_cageOpened.color  = (cm.hasGaragist(sceneName)) ? trackDetailValidated : trackDetailPending;
            }

        }

        public void displayDebugPanel()
        {
            //panel.onDeactivate.Invoke();
            debugPanel.onActivate.Invoke();
        }
    }
}
