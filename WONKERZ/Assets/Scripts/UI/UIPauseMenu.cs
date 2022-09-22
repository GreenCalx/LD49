using UnityEngine;
//using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// TODO : Induces input buffering (ex start jump, pause, spam jump, unpause => boom rocket jump)
// THUS !! Player must be frozen and most likely any kind of User inputs beside this pause menu.
public class UIPauseMenu : MonoBehaviour, IControllable
{
    public UISelectableElement panel;

    public enum EXITABLE_SCENES { SN_TITLE, SN_HUB };
    public EXITABLE_SCENES sceneToLoadOnExit = EXITABLE_SCENES.SN_TITLE;

    [Header("Mandatory")]
    public Text tracknameText;
    public Text collectedNuts;

    [Header("Track Handles")]
    public GameObject collectiblesHandle;

    void Awake()
    {
        Utils.attachControllable(this);
    }

    void OnDestroy()
    {
        Utils.detachControllable(this);
    }

    void IControllable.ProcessInputs(InputManager.InputData Entry)
    {
        if (Entry.Inputs[Constants.INPUT_START].IsDown)
        {
            tracknameText.text = SceneManager.GetActiveScene().name;
            updateTrackDetails();

            panel.onActivate.Invoke();
        }
    }

    public void pauseGame(bool isPaused)
    {
        Time.timeScale = (isPaused ? 0 : 1);
        Access.Player().isFrozen = isPaused;
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
        SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Single);
    }

    public void OnCameraToggleChange(bool value)
    {
        Access.CameraManager().changeCamera(value ? GameCamera.CAM_TYPE.HUB : GameCamera.CAM_TYPE.TRACK);
    }

    public void GetCameraToggleValue(UICheckbox.UICheckboxValue value)
    {
        value.value = Access.CameraManager().active_camera ?
            (Access.CameraManager().active_camera.camType == GameCamera.CAM_TYPE.HUB) :
            false
            ;
    }

    public void updateTrackDetails()
    {
        // update collectibles
        if (!!collectiblesHandle)
        {
            CollectiblesManager cm = Access.CollectiblesManager();
            // collected nuts
            string n_nuts = cm.getCollectedNuts(SceneManager.GetActiveScene().name).ToString();
            string tot_nuts = cm.getCollectableCollectible<CollectibleNut>(collectiblesHandle).ToString();
            collectedNuts.text = n_nuts + "/" + tot_nuts ; // TODO : get total nuts per track
        }

    }
}
