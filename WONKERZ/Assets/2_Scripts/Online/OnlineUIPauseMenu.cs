using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic; 
using TMPro;
using Schnibble;
using Schnibble.UI;
using Schnibble.Managers;
using Wonkerz;
using Mirror;

// TODO : Induces input buffering (ex start jump, pause, spam jump, unpause => boom rocket jump)
// THUS !! Player must be frozen and most likely any kind of User inputs beside this pause menu.
public class OnlineUIPauseMenu : MonoBehaviour, IControllable
{
    public GameObject UIHandle;
    public UISelectableElement panel;
    public UISelectableElement debugPanel;

    public enum EXITABLE_SCENES { SN_TITLE };
    public EXITABLE_SCENES sceneToLoadOnExit = EXITABLE_SCENES.SN_TITLE;

    [Header("Mandatory")]
    public UIOnlinePlayerInfoLine UILobbyPlayerPrefab;
    public Transform UILobbyPlayerInfoHandle;

    private PlayerController attachedPlayer;
    private List<UIOnlinePlayerInfoLine> playerInfoLines = new List<UIOnlinePlayerInfoLine>();

    void Start()
    {
        StartCoroutine(WaitPlayerForInit());
    }

    IEnumerator WaitPlayerForInit()
    {
        while (attachedPlayer==null)
        {
            attachedPlayer = Access.Player();
            yield return null;
        }
        attachedPlayer.inputMgr.Attach(this as IControllable);
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

        if (!NetworkRoomManagerExt.singleton.onlineGameManager.gameLaunched)
        {
            return;
        }

        if ((Entry.Get((int)PlayerInputs.InputCode.UIStart) as GameInputButton).GetState().down)
        {
            updateTrackDetails();
            updateLobbyInfo();

            UIHandle.SetActive(true);
            panel.inputMgr = attachedPlayer.inputMgr;
            //panel.inputMgr = Access.PlayerInputsManager().all;
            panel.onActivate.Invoke();
            panel.activate();
        }
    }

    public void pauseGame(bool isPaused)
    {
        //Time.timeScale = (isPaused ? 0 : 1);
        if (isPaused)
            attachedPlayer.Freeze();
        else
            attachedPlayer.UnFreeze();
    }

    public void OnExitButton()
    {
        panel.onDeactivate.Invoke();
        // save & exit here

        Access.SceneLoader().loadScene(Constants.SN_TITLE);
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
        // if (!!wonkerzBar)
        // {
        //     foreach(CollectibleWONKERZ.LETTERS let in Enum.GetValues(typeof(CollectibleWONKERZ.LETTERS)))
        //     {
        //         wonkerzBar.updateLetter(let, cm.hasWONKERZLetter(let));
        //     }
        // }
        updateLobbyInfo();
    }

    public void updateLobbyInfo()
    {
        NetworkRoomManagerExt room = NetworkRoomManagerExt.singleton;

        foreach(UIOnlinePlayerInfoLine oldline in playerInfoLines)
        {
            Destroy(oldline.gameObject);
        }
        playerInfoLines = new List<UIOnlinePlayerInfoLine>();
        foreach( OnlinePlayerController opc in room.onlineGameManager.uniquePlayers)
        {
            UIOnlinePlayerInfoLine infoLine = Instantiate(UILobbyPlayerPrefab, UILobbyPlayerInfoHandle).GetComponent<UIOnlinePlayerInfoLine>();
            infoLine.Refresh(opc);
            playerInfoLines.Add(infoLine);

        }
    }

    public void displayDebugPanel()
    {
        //panel.onDeactivate.Invoke();
        debugPanel.onActivate.Invoke();
    }
}
