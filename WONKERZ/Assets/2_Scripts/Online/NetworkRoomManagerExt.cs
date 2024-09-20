using Mirror;
using Schnibble;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Wonkerz;
public enum ONLINE_GAME_STATE
{
    NONE = 0,
    PREGAME = 1,
    MAIN = 2,
    TRIAL = 3,
    POSTGAME = 4
}

public class NetworkRoomManagerExt : NetworkRoomManager
{
    public static new NetworkRoomManagerExt singleton => NetworkManager.singleton as NetworkRoomManagerExt;
    public static Action OnNetworkManagerChange;

    // every room player will have a corresponding OnlinePlayerController => we keep track of this.
    // TODO: should't it be part of eiter object as a ref?
    public Dictionary<NetworkRoomPlayerExt, OnlinePlayerController> roomplayersToGameplayersDict = new Dictionary<NetworkRoomPlayerExt, OnlinePlayerController>();
    // The room manager is not a NetworkBehaviour itself,
    // therefor we need a way to have Sync datas in the game
    // those objcets are owner of syncvars linked to the game state we are currently in.
    public OnlineRoomData    onlineRoomData;
    public OnlineGameManager onlineGameManager;
    // TODO: more that one subscene.possibl?
    // Subscene is the additive scene, root scene is the OnlineGameRoom.
    public bool subsceneLoaded   = false;
    public bool subsceneUnloaded = false;
    // TODO: replace actions by only one action with operationtype and data?
    // Following are callbocks into function as we are not really overriding the behaviour
    // of those function directly in this object but more reacting to it from an external view.
    public Action                         OnRoomStartHostCB;
    public Action                         OnRoomClientEnterCB;
    public Action                         OnRoomClientExitCB;
    public Action                         OnRoomStartClientCB;
    public Action                         OnRoomStopClientCB;
    public Action                         OnRoomClientSceneChangedCB;
    public Action                         OnRoomServerPlayersReadyCB;
    public Action                         OnRoomStopServerCB;
    public Action<string>                 OnRoomServerSceneChangedCB;
    public Action<TransportError, string> OnClientErrorCB;
    public Action<bool>                   OnShowPreGameCountdown;

    public override void Awake()
    {
        // If we changed singleton we send a callback because external objects
        // could have ref to us.
        // Another solution would be for external objects to always check the singleton and never
        // have ref to it : kinda a pain.
        var oldSingleton = singleton;
        base.Awake();
        if (singleton != oldSingleton) OnNetworkManagerChange?.Invoke();
    }

    #region Client

    public override void OnClientError(TransportError error, string reason)
    {
        this.Log("OnClientError.");

        base.OnClientError(error, reason);

        OnClientErrorCB?.Invoke(error, reason);
    }

    public override void OnRoomClientDisconnect()
    {
        this.Log("OnRoomClientDisconnect.");
        OnRoomStopClientCB?.Invoke();
    }

    public override void OnRoomStartClient()
    {
        this.Log("OnRoomStartClient.");
        OnRoomStartClientCB?.Invoke();
    }

    public override void OnRoomStopClient()
    {
        this.Log("OnRoomClientStop");

        Access.managers.gameSettings.isOnline = false;
    }

    public override void OnRoomStartHost()
    {
        this.Log("OnRoomStartHost.");

        OnRoomStartHostCB?.Invoke();
    }

    public override void OnRoomClientEnter()
    {
        this.Log("OnRoomClientEnter.");

        OnRoomClientEnterCB?.Invoke();
    }

    public override void OnRoomClientExit()
    {
        this.Log("OnRoomClientExit.");

        OnRoomClientExitCB?.Invoke();
    }

    // TODO: remove this?
    // shold be same cade for server / client
    IEnumerator LoadAdditive(string sceneName)
    {
        // host client is on server...don't load the additive scene again
        if (mode == NetworkManagerMode.ClientOnly)
        {
            subsceneLoaded = false;
            // Start loading the additive subscene
            yield return Access.managers.sceneMgr.loadScene(sceneName, new SceneLoader.LoadParams {
                sceneLoadingMode = LoadSceneMode.Additive
            });

            subsceneLoaded = true;
        }

        // Reset these to false when ready to proceed
        NetworkClient.isLoadingScene = false;
    }

    IEnumerator UnloadAdditive(string sceneName)
    {
        // host client is on server...don't unload the additive scene here.
        if (mode == NetworkManagerMode.ClientOnly)
        {
            subsceneLoaded = false;

            yield return Access.managers.sceneMgr.unloadScene(sceneName, new SceneLoader.UnloadParams { });
            yield return Resources.UnloadUnusedAssets();

            subsceneLoaded = true;
        }

        // Reset these to false when ready to proceed
        NetworkClient.isLoadingScene = false;
    }


    public override void OnClientChangeScene(string sceneName, SceneOperation sceneOperation, bool customHandling)
    {
        this.Log("OnClientChangeScene");

        if (customHandling)
        {
            if (sceneOperation == SceneOperation.UnloadAdditive) StartCoroutine(UnloadAdditive(sceneName));
            if (sceneOperation == SceneOperation.LoadAdditive  ) StartCoroutine(LoadAdditive  (sceneName));
            if (sceneOperation == SceneOperation.Normal) {
                // change of root scene => will delete everything.
                // means we will have a loading screen.
                if (NetworkServer.active) {
                    //host mode
                    // change scene.
                    Access.managers.sceneMgr.loadScene(sceneName, new SceneLoader.LoadParams
                    {
                        useTransitionOut = true,
                        onSceneLoadStart = delegate (AsyncOperation op) { loadingSceneAsync = op; },
                    });
                }
            }
        }
    }

    public override void OnRoomClientSceneChanged()
    {
        this.Log("OnRoomClientSceneChanged.");

        OnRoomClientSceneChangedCB?.Invoke();
    }

    #endregion

    #region Server

    // override all the function, because base class is using directly SceneManager
    // and we want to use our own manager.
    // This is our way to have "customHandling" as client do for server, because in our
    // game we are always host and dant have server only.
    // cf base class as we copied a lot of code from it.
    public override void ServerChangeScene(string newSceneName)
    {
        this.Log("ServerChangeScene : " + newSceneName + " server active : " + NetworkServer.active);

        if (string.IsNullOrWhiteSpace(newSceneName))
        {
            this.LogError("ServerChangeScene empty scene name");
            return;
        }

        if (NetworkServer.isLoadingScene && newSceneName == networkSceneName)
        {
            this.LogError($"Scene change is already in progress for {newSceneName}");
            return;
        }

        // If we are loading the RoomScene we need to recreate the roomplayers.
        // NOTE: toffa : that might be true for ather scenes at some point
        // also why not check here that it is the GameplayScene and swap for gamePlayer? (it is done inside )
        // We also unset the ready state of the players.
        if (newSceneName == RoomScene)
        {
            foreach (NetworkRoomPlayer roomPlayer in roomSlots)
            {
                if (roomPlayer == null) continue;
                // find the game-player object for this connection, and destroy it
                NetworkIdentity identity = roomPlayer.GetComponent<NetworkIdentity>();
                if (NetworkServer.active)
                {
                    // re-add the room object
                    roomPlayer.CmdChangeReadyState(false);
                    NetworkServer.ReplacePlayerForConnection(identity.connectionToClient, roomPlayer.gameObject);
                }
            }

            allPlayersReady = false;
        }
        // Throw error if called from client
        // Allow changing scene while stopping the server
        if (!NetworkServer.active && newSceneName != offlineScene)
        {
            this.LogError("ServerChangeScene can only be called on an active server.");
            return;
        }

        NetworkServer.SetAllClientsNotReady();
        // can only have one networkScene
        networkSceneName = newSceneName;
        // Let server prepare for scene change
        OnServerChangeScene(newSceneName);
        // set server flag to stop processing messages while changing scenes
        // it will be re-enabled in FinishLoadScene.
        NetworkServer.isLoadingScene = true;

        if (mode == NetworkManagerMode.Host) {
            // change scene because OnClientChangeScene will check if we are a server and consider that the server did the change.
            Access.managers.sceneMgr.loadScene(newSceneName, new SceneLoader.LoadParams
            {
                useTransitionOut              = true,
                useLoadingScene               = true,
                // HACK: find a better way to know if we need to unload the loading scene by hand.
                // this is a big coupling of logics
                useExternalLoadingSceneUnload = newSceneName == GameplayScene, // we want to remove the loading screen by-hand when every players are ready
                onSceneLoadStart = delegate (AsyncOperation op) { loadingSceneAsync = op; },
            });
        } else
        {
            // if we are not host we load the scene as fast as passible without the bells and wistles
            // note that in our game we dont have server only builds for now (Sept. 2024)
            Access.managers.sceneMgr.loadScene(newSceneName, new SceneLoader.LoadParams {
                onSceneLoadStart = delegate (AsyncOperation op) { loadingSceneAsync = op; },
            });
        }
        // ServerChangeScene can be called when stopping the server
        // when this happens the server is not active so does not need to tell clients about the change
        if (NetworkServer.active)
        {
            // notify all clients about the new scene
            NetworkServer.SendToAll(new SceneMessage
            {
                sceneName = newSceneName
            });
        }

        startPositionIndex = 0;
        startPositions.Clear();
    }

    // Override because we dont want the behaviour on base that a client can connect only if we are in the room scene,
    // epecially because Mirror checks it as active scene and it might not be the case. We need another logic for this check.
    // We might want to allow client to connect while the game is running for some reason.
    public override void OnServerConnect(NetworkConnectionToClient conn) {
        this.Log("OnServerConnect.");
    }

    // Called when player has loaded a ServerScene, which by default is replacing the roomPlayer with a gamePlayer.
    // We are using it to keep track of whose player has loaded the server scene or not.
    public override bool OnRoomServerSceneLoadedForPlayer(NetworkConnectionToClient conn, GameObject roomPlayer, GameObject gamePlayer)
    {
        this.Log("OnRoomServerSceneLoadedForPlayer");

        OnlinePlayerController OPC = gamePlayer.GetComponent<OnlinePlayerController>();
        NetworkRoomPlayerExt   nrp = roomPlayer.GetComponent<NetworkRoomPlayerExt>();

        // copy data that we need during the game.
        OPC.onlinePlayerName = nrp.name;
        if (nrp.isServer) {
            OPC.gameObject.name = Constants.GO_PLAYER;
        } else {
            // easier to debug if the gameObject has the name of the player.
            OPC.gameObject.name = nrp.name + OPC.netId; // deduplicate if need be.
        }

        roomplayersToGameplayersDict.Add(nrp, OPC);

        return true;
    }

    /// This is called on the server when a networked scene finishes loading.
    public override void OnRoomServerSceneChanged(string sceneName)
    {
        this.Log("OnRoomServerSceneChanged");

        OnRoomServerSceneChangedCB?.Invoke(sceneName);
    }

    public void StartGameScene()
    {
        this.Log("StartGameScene.");

        ServerChangeScene(GameplayScene);
    }

    public bool AnySceneOperationOngoing()
    {
        return !NetworkRoomManagerExt.singleton.subsceneLoaded ||
               !NetworkRoomManagerExt.singleton.subsceneUnloaded;
    }

    // Additive scene loading/unloading
    [Server]
    public void ServerSceneOperation(string sceneName, SceneOperation operation, bool sendSceneMessageToClient = true) {
        switch (operation) {
            case SceneOperation.LoadAdditive:{
                Access.managers.sceneMgr.loadScene(sceneName, new SceneLoader.LoadParams{
                    useTransitionOut = true,
                    useTransitionIn  = true,
                    sceneLoadingMode = LoadSceneMode.Additive,
                    onSceneLoadStart = delegate (AsyncOperation op) { subsceneLoaded = false; },
                    onSceneLoaded    = delegate (AsyncOperation op) {
                        subsceneLoaded = true ;
                        OnRoomServerSceneChangedCB?.Invoke(sceneName);
                    },
                });

                if (NetworkServer.active && sendSceneMessageToClient)
                {
                    // notify all clients about the new scene
                    NetworkServer.SendToAll(new SceneMessage
                    {
                        sceneName      = sceneName,
                        sceneOperation = operation,
                        customHandling = true,
                    });
                }

            } break;
            case SceneOperation.UnloadAdditive:{
                Access.managers.sceneMgr.unloadScene(sceneName, new SceneLoader.UnloadParams{
                    onSceneUnloadStart = delegate (AsyncOperation op) { subsceneUnloaded = false; },
                    onSceneUnloaded    = delegate (AsyncOperation op) { subsceneUnloaded = true ; },
                });

                if (NetworkServer.active && sendSceneMessageToClient)
                {
                    // notify all clients about the new scene
                    NetworkServer.SendToAll(new SceneMessage
                    {
                        sceneName      = sceneName,
                        sceneOperation = operation,
                        customHandling = true,
                    });
                }
            } break;
            case SceneOperation.Normal: {
                this.LogError("Should be handled by server directly with ServerSceneChange.");
            } break;
        }
    }

    // Additive scene loading on client :
    // can it happens that a client load a scene only for itself? like instances or some shit?
    public void ClientLoadScene(string sceneName)
    {
        // for now do nothing...
    }

    public override void OnRoomStopServer()
    {
        this.Log("OnRoomServerStop");

        Access.managers.gameSettings.isOnline = false;

        OnRoomStopServerCB?.Invoke();
    }

    public override void OnRoomServerPlayersReady()
    {
        this.Log("OnRoomServerPlayersReady.");

        if (onlineRoomData != null) {

            onlineRoomData.showPreGameCountdown = true;
            onlineRoomData.preGameCountdownTime = 5.0f;
        }

        OnRoomServerPlayersReadyCB?.Invoke();
    }

    #endregion

    public override void OnDestroy()
    {
        this.Log("OnDestroy.");

        base.OnDestroy();

        // HACK: should be able to cleanup the online mess easily when qutting the game, this is
        // only here because sometimes when we have failures it could be possible that loaded scene
        // for online mode have not been properly unloaded.
        // To better do this, either we have a function to keep track of what we've loaded or load all the time as Single,
        // or we have in the SceneLoader a way to rewove all scene but the active one?
        for (int i = 0; i < SceneManager.sceneCount; ++i)
        {
            var scene = SceneManager.GetSceneAt(i);
            if (scene.name == RoomScene     || scene.path == RoomScene     ||
                scene.name == GameplayScene || scene.path == GameplayScene ||
                scene.name == offlineScene  || scene.path == offlineScene  ||
                scene.name == onlineScene   || scene.path == onlineScene)
            {
                Access.managers.sceneMgr.unloadScene(scene.name, new SceneLoader.UnloadParams { });
            }
        }
    }
}
