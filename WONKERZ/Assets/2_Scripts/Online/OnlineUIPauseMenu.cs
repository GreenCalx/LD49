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

    public PlayerController attachedPlayer;
    private List<UIOnlinePlayerInfoLine> playerInfoLines = new List<UIOnlinePlayerInfoLine>();

    // Consider that it does not changed during the lifetime of this gameObject.
    NetworkRoomManagerExt room;

    // Should be init or select.
    override public void init()
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

        room = NetworkRoomManagerExt.singleton;
    }

    override protected void Update() {
        if (isActivated) {
            updateLobbyInfo();
        }
    }

    override public void deinit() {
        attachedPlayer.inputMgr.Detach(this);
    }

    override protected void ProcessInputs(InputManager currentMgr, GameController Entry)
    {
        if (!room.onlineGameManager.gameLaunched)
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
        if (room != null) {
            if (NetworkClient.activeHost)
            {
                room.ServerChangeScene(room.RoomScene);
            }
        }
    }

    public void OnExitButton()
    {
        // save & exit here
        if (room != null)
        {
            // make shure we dont go back to offline room.
            room.offlineScene = "";
            if (NetworkServer.activeHost)
            {
                room.StopHost();

                GameObject.Destroy(room.gameObject);
                NetworkServer.Shutdown();
            }
            else if (NetworkClient.active)
            {
                room.StopClient();

                GameObject.Destroy(room.gameObject);
                NetworkClient.Shutdown();
            }
        }

        Access.managers.sceneMgr.ResetDontDestroyOnLoad();
        Access.managers.sceneMgr.loadScene(Constants.SN_TITLE, new SceneLoader.LoadParams{
            useTransitionOut = true,
            useTransitionIn  = true,
            sceneLoadingMode = LoadSceneMode.Single,
        });
    }

    public void OnCameraToggleChange(bool value)
    {
        //Access.managers.cameraMgr.changeCamera(value ? GameCamera.CAM_TYPE.ORBIT : GameCamera.CAM_TYPE.OLD_TRACK);
        // Disable auto rot of manual camera
        ManualCamera mc = Access.managers.cameraMgr.active_camera.GetComponent<ManualCamera>();
        if (!!mc)
        {
            mc.autoAlign = value;
        }
    }

    public void GetCameraToggleValue(UICheckbox.UICheckboxValue value)
    {
        value.value = Access.managers.cameraMgr.active_camera ?
            (Access.managers.cameraMgr.active_camera.camType == GameCamera.CAM_TYPE.ORBIT) :
            false
            ;
    }

    public void updateTrackDetails()
    {
        // update collectibles
        CollectiblesManager cm = Access.managers.collectiblesMgr;

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

    void createInfoLines() {
        foreach(UIOnlinePlayerInfoLine oldline in playerInfoLines)
        {
            Destroy(oldline.gameObject);
        }
        playerInfoLines = new List<UIOnlinePlayerInfoLine>(room.roomSlots.Count);
        foreach (OnlinePlayerController opc in room.onlineGameManager.uniquePlayers)
        {
            UIOnlinePlayerInfoLine infoLine = Instantiate(UILobbyPlayerPrefab, UILobbyPlayerInfoHandle).GetComponent<UIOnlinePlayerInfoLine>();
            infoLine.player = opc;
            playerInfoLines.Add(infoLine);
        }
    }

    public void updateLobbyInfo()
    {
        // Consider that for now there is no change in player count and index
        // during the game.
        // TODO: subscribe to room changes to reflect the changes.
        if (playerInfoLines.Count != room.roomSlots.Count) {
            createInfoLines();
        }

        foreach(var infoLine in playerInfoLines) infoLine.Refresh();
    }

    public void displayDebugPanel()
    {
        //panel.onDeactivate.Invoke();
        debugPanel.onActivate.Invoke();
    }
}
