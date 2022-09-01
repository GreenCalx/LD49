using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

// TODO : Induces input buffering (ex start jump, pause, spam jump, unpause => boom rocket jump)
// THUS !! Player must be frozen and most likely any kind of User inputs beside this pause menu.
public class UIPauseMenu : MonoBehaviour, IControllable
{
    public enum EXITABLE_SCENES { SN_TITLE, SN_HUB };
    public EXITABLE_SCENES sceneToLoadOnExit = EXITABLE_SCENES.SN_TITLE;

    [Header("Mandatory")]
    /// These Buttons MUST have an outline component as well.
    public Button exitButton;
    public Button optionsButton;
    public Text   tracknameText;
    public Button toggleButton;
    public Toggle cameraToggle;

    [Header("Tweaks")]
    public float selector_latch = 0.2f;
    public Color outlineSelectedBtn;
    public Color outlineClickedBtn;

    private float elapsed_time = 0f;
    private bool menuIsOpened = false;
    private List<Button> selectables;
    private int index;
    

    // Start is called before the first frame update
    void Start()
    {
    }

    void Awake()
    {
        init();
        Utils.attachControllable<UIPauseMenu>(this);
    }

    private void init()
    {
        exitButton.onClick.AddListener(OnExitButton);
        toggleButton.onClick.AddListener(OnCameraToggleButton);
        cameraToggle.onValueChanged.AddListener ( delegate {OnCameraToggleChange(cameraToggle);} );

        selectables = new List<Button>();
        
        selectables.Add(toggleButton);
        selectables.Add(optionsButton);
        selectables.Add(exitButton);

        index = 0;
        elapsed_time = 0f;
        
        deactivateAll();
    }

    void OnDestroy()
    {
        Utils.detachControllable<UIPauseMenu>(this);
        pauseGame(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void IControllable.ProcessInputs(InputManager.InputData Entry) 
    {
        if (menuIsOpened)
        {
            if (elapsed_time > selector_latch)
            {
                float Y = Entry.Inputs[Constants.INPUT_UIUPDOWN].AxisValue;
                if (Y < -0.2f)
                {
                    
                    selectNext();
                    elapsed_time = 0f;
                }
                else if ( Y > 0.2f )
                {
                    selectPrevious();
                    elapsed_time = 0f;
                }
            }
            elapsed_time += Time.unscaledDeltaTime;

            if (Entry.Inputs["Jump"].IsDown)
            {
                pick();
            }
        }

        if (Entry.Inputs[Constants.INPUT_START].IsDown)
        {
            if (menuIsOpened)
            {
                deactivateAll();
                pauseGame(false);
                Access.Player().isFrozen = false;
            } else {
                tracknameText.text = SceneManager.GetActiveScene().name;
                activateAll();
                pauseGame(true);
                Access.Player().isFrozen = true;
            }
        } 
    }

    private void selectNext()
    {
        index++;
        if (index>=selectables.Count) 
            index = 0;
        refreshButtons();
    }
    private void selectPrevious()
    {
        index--;
        if ( index < 0 ) 
            index = selectables.Count-1;
        refreshButtons();
    }

    private void pick()
    {
        Button selected = selectables[index];
        selected.onClick.Invoke();
    }

    private void refreshButtons()
    {
        for (int i=0; i < selectables.Count; i++)
        {
            Outline outline = selectables[i].GetComponent<Outline>();
            if (outline==null)
            { Debug.LogWarning("Missing Outline component on a UIPauseMenu button : " + selectables[i].gameObject.name); continue; }
            if (i==index)
            {
                outline.enabled = true;
                outline.effectColor = outlineSelectedBtn; 
            }
            else
            {
                outline.enabled = false;
            }
        }
    }

    private void refreshCameraState()
    {
        cameraToggle.isOn = Access.CameraManager().active_camera ? 
            (Access.CameraManager().active_camera.camType == GameCamera.CAM_TYPE.HUB) :
            false
            ;
    }

    private void pauseGame(bool isPaused)
    {
        Time.timeScale = (isPaused ? 0 : 1);
    }

    private void deactivateAll()
    {
        foreach( Transform child in transform )
        {
            child.gameObject.SetActive(false);
        }
        menuIsOpened = false;
    }

    private void activateAll()
    {
        foreach( Transform child in transform )
        {
            child.gameObject.SetActive(true);
        }
        menuIsOpened = true;
        refreshButtons();
        refreshCameraState();
    }

    private void OnExitButton()
    {
        // save & exit here
        string sceneToLoad = "";
        switch (sceneToLoadOnExit)
        {
            case EXITABLE_SCENES.SN_TITLE :
                sceneToLoad = Constants.SN_TITLE;
                break;
            case EXITABLE_SCENES.SN_HUB:
                sceneToLoad = Constants.SN_HUB;
                break;
            default:
                sceneToLoad = Constants.SN_TITLE;
                break;
        }
        SceneManager.LoadScene( sceneToLoad, LoadSceneMode.Single);
    }

    private void OnCameraToggleButton()
    {
        cameraToggle.isOn = !cameraToggle.isOn;
    }

    private void OnCameraToggleChange(Toggle iToggle)
    {
        if (iToggle.isOn)
        {
            Access.CameraManager().changeCamera( GameCamera.CAM_TYPE.HUB );
        } else {
            Access.CameraManager().changeCamera( GameCamera.CAM_TYPE.TRACK );
        }
    }
}
