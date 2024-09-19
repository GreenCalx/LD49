using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using TMPro;
using Mirror;

using Schnibble;
using Schnibble.UI;
using Schnibble.Managers;

using Wonkerz;

// TODO : Induces input buffering (ex start jump, pause, spam jump, unpause => boom rocket jump)
// THUS !! Player must be frozen and most likely any kind of User inputs beside this pause menu.
public class OnlineUIPauseMenu : UIControllableElement
{
    public GameObject UIHandle;
    public UIControllableElement panel;
    public UIControllableElement debugPanel;
    public UITab goToRoomTab;

    public enum EXITABLE_SCENES { SN_TITLE };
    public EXITABLE_SCENES sceneToLoadOnExit = EXITABLE_SCENES.SN_TITLE;

    [Header("Mandatory")]
    public UIOnlinePlayerInfoLine UILobbyPlayerPrefab;
    public Transform UILobbyPlayerInfoHandle;

    private PlayerController attachedPlayer;
    private List<UIOnlinePlayerInfoLine> playerInfoLines = new List<UIOnlinePlayerInfoLine>();

    override protected void OnEnable()
    {
        if (attachedPlayer == null) {
            attachedPlayer = Access.Player();
            if (attachedPlayer == null) {
                this.LogError("No player has been attached.");
                return;
            }
        }

        attachedPlayer.inputMgr.Attach(this);

        if (NetworkClient.activeHost) {
            var tabs = (panel as UIPanelTabbed).tabs;
            if (!tabs.Contains(goToRoomTab)) {
                tabs.Add(goToRoomTab);
                goToRoomTab.init();
                goToRoomTab.gameObject.SetActive(true);
            }
        }
    }

    override protected void OnDisable() {
        attachedPlayer.inputMgr.Detach(this);
    }

    override protected void ProcessInputs(InputManager currentMgr, GameController Entry)
    {
        if (!NetworkRoomManagerExt.singleton.onlineGameManager.gameLaunched)
        {
            return;
        }

        if ((Entry.Get((int)PlayerInputs.InputCode.UIStart) as GameInputButton).GetState().down)
        {
            updateTrackDetails();
            updateLobbyInfo();

            // NOTE: need to set input manager before calling SetActive as it will activate the UX.
            panel.inputMgr = attachedPlayer.inputMgr;
            //panel.init(); => not needed for now.
            UIHandle.SetActive(true);
        }
    }

    public void pauseGame(bool isPaused)
    {
        #if false
        if (isPaused)
        attachedPlayer.Freeze();
        else
        attachedPlayer.UnFreeze();
        #endif
    }

    public void GoBackToRoom() {
        if (NetworkRoomManagerExt.singleton != null) {
            if (NetworkClient.activeHost)
            {
                NetworkRoomManagerExt.singleton.ServerChangeScene(NetworkRoomManagerExt.singleton.RoomScene);
            }
        }
    }

    public void OnExitButton()
    {
        // save & exit here
        if (NetworkRoomManagerExt.singleton != null)
        {
            // make shure we dont go back to offline room.
            NetworkRoomManagerExt.singleton.offlineScene = "";
            if (NetworkServer.activeHost)
            {
                NetworkRoomManagerExt.singleton.StopHost();

                GameObject.Destroy(NetworkRoomManagerExt.singleton.gameObject);
                NetworkServer.Shutdown();
            }
            else if (NetworkClient.active)
            {
                NetworkRoomManagerExt.singleton.StopClient();

                GameObject.Destroy(NetworkRoomManagerExt.singleton.gameObject);
                NetworkClient.Shutdown();
            }
        }

        Access.SceneLoader().ResetDontDestroyOnLoad();
        Access.SceneLoader().loadScene(Constants.SN_TITLE, new SceneLoader.LoadParams{
            useTransitionOut = true,
            useTransitionIn  = true,
            sceneLoadingMode = LoadSceneMode.Single,
        });
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
