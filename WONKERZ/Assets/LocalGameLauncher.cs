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
    Queue<MainThreadCommand> pendingCommands = new Queue<MainThreadCommand>();
    // Start is called before the first frame update
    void Update()
    {
        while (pendingCommands.Count != 0) {
            var cmd = pendingCommands.Dequeue();
            if (cmd == null)
            {
                this.LogError("Command is null => very weird!");
            }
            else
            {
                cmd.Do();
            }
        }

        if (!isExecuted) {
            if (GameSettings.testMenuData.autoStartMode != GameSettings.AutoStartMode.None) {
                isExecuted = true;
                Access.managers.sceneMgr.SetDontDestroyOnLoad(this.gameObject);

                switch(GameSettings.testMenuData.autoStartMode){
                    case GameSettings.AutoStartMode.SoloHost:
                        {
                            // solo host means start a local host and launch the game
                            // without waiting.
                            Access.managers.gameSettings.isLocal = true;
                            Access.managers.sceneMgr.loadScene("OfflineRoom", new SceneLoader.LoadParams{
                                useLoadingScene = false,
                                useTransitionIn = false,
                                useTransitionOut = false,
                                onSceneLoaded = delegate(AsyncOperation op) {
                                    var uiOnline = GameObject.Find("UIOnline").GetComponent<UIOnline>();

                                    uiOnline.Deinit();
                                    uiOnline.Deactivate();
                                    uiOnline.Init();

                                    uiOnline.roomServer.OnRoomClientEnterCB += delegate() {
                                        uiOnline.roomServer.roomSlots[0].CmdChangeReadyState(true);
                                        uiOnline.roomServer.StartGameScene();
                                    };

                                    uiOnline.client.OnConnected += delegate (int clientID) {
                                        pendingCommands.Enqueue(new MainThreadCommand( delegate() {

                                            uiOnline.SetState(UIOnline.States.CreatingRoom);

                                            uiOnline.client.CreateLobby(new Lobby
                                            {
                                                maxPlayerCount = 4,
                                                //cf :hastName: SchLobbyServer
                                                hostName = Access.managers.gameProgressSaveMgr.activeProfile,
                                                name     = "generic debug name",
                                            });
                                        }));
                                    };

                                    uiOnline.Activate();
                                    uiOnline.SetState(UIOnline.States.Deactivated);
                                },
                            });
                        } break;

                    case GameSettings.AutoStartMode.Host:
                        {
                            // host means launch host and wait for clients.
                            Access.managers.gameSettings.isLocal = true;
                            Access.managers.sceneMgr.loadScene("OfflineRoom", new SceneLoader.LoadParams{
                                useLoadingScene = false,
                                useTransitionIn = false,
                                useTransitionOut = false,
                                onSceneLoaded = delegate(AsyncOperation op) {
                                    var uiOnline = GameObject.Find("UIOnline").GetComponent<UIOnline>();

                                    uiOnline.Deinit();
                                    uiOnline.Deactivate();

                                    uiOnline.Init();

                                    uiOnline.client.OnConnected += delegate (int clientID) {
                                        pendingCommands.Enqueue(new MainThreadCommand(delegate() {

                                            uiOnline.SetState(UIOnline.States.CreatingRoom);

                                            uiOnline.client.CreateLobby(new Lobby
                                            {
                                                maxPlayerCount = 4,
                                                //cf :hastName: SchLobbyServer
                                                hostName = "Host", // easier to debug.
                                                name     = "Generic debug name",
                                            });
                                        }));
                                    };

                                    uiOnline.Activate();
                                    uiOnline.SetState(UIOnline.States.Deactivated);
                                }
                            });
                        } break;

                    case GameSettings.AutoStartMode.Client:
                        {
                            // Client means connect to the first server available.
                            Access.managers.gameSettings.isLocal = true;
                            Access.managers.sceneMgr.loadScene("OfflineRoom", new SceneLoader.LoadParams{
                                useLoadingScene = false,
                                useTransitionIn = false,
                                useTransitionOut = false,
                                onSceneLoaded = delegate(AsyncOperation op) {
                                    var uiOnline = GameObject.Find("UIOnline").GetComponent<UIOnline>();
                                    // start again the local server from scratch.
                                    uiOnline.Deactivate();
                                    uiOnline.Deinit();
                                    uiOnline.Init();
                                    uiOnline.client.OnConnected += delegate (int clientID) {
                                        pendingCommands.Enqueue(new MainThreadCommand(
                                            delegate() {
                                                uiOnline.roomServer.OnRoomStartClientCB += delegate() {
                                                    uiOnline.Init();
                                                    uiOnline.SetState(UIOnline.States.InRoom);
                                                };

                                                uiOnline.client.OnStartNATPunch += delegate(IPEndPoint toNATPunch) {
                                                    pendingCommands.Enqueue(new MainThreadCommand(
                                                        delegate() {
                                                            uiOnline.ConnectToRoom(toNATPunch, 0);
                                                        }));
                                                };

                                                uiOnline.client.JoinLobby(0);
                                            }
                                        ));
                                    };

                                    uiOnline.Activate();
                                },
                            });
                        } break;
                }
            }
        }
    }
}
