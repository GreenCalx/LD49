using System.Collections;
using System.Collections.Generic;
using System.Net;
using System;

using UnityEngine;

using Mirror;

using Schnibble;
using Schnibble.Online;

using Wonkerz;

public class LocalGameLauncher : MonoBehaviour
{
    bool isExecuted = false;

    // What happens when connected to LobbyServer
    void OnConnected(int clientID) {
        this.Log("OnConnected");
        switch(GameSettings.testMenuData.autoStartMode){
            case GameSettings.AutoStartMode.Host:
                case GameSettings.AutoStartMode.SoloHost: {
                    MainThreadCommand.pendingCommands.Enqueue(new MainThreadCommand( delegate() {

                        var profileName = Access.managers.gameProgressSaveMgr.activeProfile;
                        if (string.IsNullOrEmpty(profileName)) profileName = "Host";

                        NetworkRoomManagerExt.singleton.CreateRoom(new Lobby
                        {
                            maxPlayerCount = 4,
                            //cf :hastName: SchLobbyServer
                            hostName = profileName,
                            name     = "generic debug name",
                        });

                        NetworkRoomManagerExt.singleton.lobbyClient.OnConnected -= OnConnected;
                    }));
                } break;
            case GameSettings.AutoStartMode.Client:
                {
                    MainThreadCommand.pendingCommands.Enqueue(new MainThreadCommand(
                        delegate() {
                            NetworkRoomManagerExt.singleton.JoinLobby(0);
                            NetworkRoomManagerExt.singleton.lobbyClient.OnConnected -= OnConnected;
                        }
                    ));
                } break;
        }
    }

    void OnRoomEnter() {
        switch(GameSettings.testMenuData.autoStartMode){
            case GameSettings.AutoStartMode.SoloHost: {
                var netMgr = NetworkRoomManagerExt.singleton;
                netMgr.roomSlots[0].CmdChangeReadyState(true);
                netMgr.StartGameScene();
                netMgr.OnRoomClientEnterCB -= OnRoomEnter;
            }break;
        }

    }

    // Start is called before the first frame update
    void Update()
    {
        if (!isExecuted) {
            if (GameSettings.testMenuData.autoStartMode != GameSettings.AutoStartMode.None) {
                isExecuted = true;
                Access.managers.sceneMgr.SetDontDestroyOnLoad(this.gameObject);
                // Start server scene
                Access.managers.gameSettings.isLocal = true;
                Access.managers.sceneMgr.loadScene("OfflineRoom", new SceneLoader.LoadParams{
                    useLoadingScene = false,
                    useTransitionIn = false,
                    useTransitionOut = false,
                    onSceneLoaded = delegate(AsyncOperation op) {
                        var netMgr = NetworkRoomManagerExt.singleton;
                        // register callbacks
                        netMgr.lobbyClient.OnConnected += OnConnected;
                        netMgr.OnRoomClientEnterCB     += OnRoomEnter;
                        // reconnect now that we have setup callbacks;
                        netMgr.JoinLocalLobbyServer();
                    },
                });
            }
        }
    }
}
