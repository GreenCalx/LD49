using UnityEngine;
//using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using TMPro;
using Schnibble;

// TODO : Induces input buffering (ex start jump, pause, spam jump, unpause => boom rocket jump)
// THUS !! Player must be frozen and most likely any kind of User inputs beside this pause menu.
public class UIPauseMenu : MonoBehaviour, IControllable
{
    public UISelectableElement panel;
    public UISelectableElement debugPanel;

    public enum EXITABLE_SCENES { SN_TITLE, SN_HUB };
    public EXITABLE_SCENES sceneToLoadOnExit = EXITABLE_SCENES.SN_TITLE;

    [Header("Mandatory")]
    public Text tracknameText;
    public Text collectedNuts;
    public UIWonkerzBar wonkerzBar;
    public TextMeshProUGUI TMP_keyObtained;
    public TextMeshProUGUI TMP_cageOpened;

    void Awake()
    {
        Utils.attachControllable(this);
    }

    void OnDestroy()
    {
        Utils.detachControllable(this);
    }

    void IControllable.ProcessInputs(InputManager currentMgr, GameInput[] Entry)
    {
        if ((Entry[(int) PlayerInputs.InputCode.UIStart] as GameInputButton).GetState().down)
        {
            tracknameText.text = SceneManager.GetActiveScene().name;
            updateTrackDetails();

            panel.inputMgr = Access.InputManager();
            panel.onActivate.Invoke();
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
        Access.CameraManager().changeCamera(value ? GameCamera.CAM_TYPE.ORBIT : GameCamera.CAM_TYPE.OLD_TRACK);
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
        foreach(CollectibleWONKERZ.LETTERS let in Enum.GetValues(typeof(CollectibleWONKERZ.LETTERS)))
        {
            wonkerzBar.updateLetter(let, cm.hasWONKERZLetter(let));
        }

        // key + cage status
        string sceneName = SceneManager.GetActiveScene().name;
        TMP_keyObtained.color = (cm.hasCageKey(sceneName) ) ? Color.green : Color.red;
        TMP_cageOpened.color  = (cm.hasGaragist(sceneName)) ? Color.green : Color.red;
    }

    public void displayDebugPanel()
    {
        //panel.onDeactivate.Invoke();
        debugPanel.onActivate.Invoke();
    }
}
