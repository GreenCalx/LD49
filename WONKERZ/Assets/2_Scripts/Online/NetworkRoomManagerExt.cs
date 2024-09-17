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

    public Dictionary<NetworkRoomPlayerExt, OnlinePlayerController> roomplayersToGameplayersDict = new Dictionary<NetworkRoomPlayerExt, OnlinePlayerController>();

    // data for syncvars
    public OnlineRoomData    onlineRoomData;
    public OnlineGameManager onlineGameManager;

    public ONLINE_GAME_STATE gameState = ONLINE_GAME_STATE.NONE;

    // TODO: more that one subscene.
    public bool subsceneLoaded   = false;
    public bool subsceneUnloaded = false;

    public string selectedTrial = "";

    // TODO: replace actions by only one action with operationtype and data?
    public Action OnRoomStartHostCB;
    public Action OnRoomClientEnterCB;
    public Action OnRoomClientExitCB;
    public Action OnRoomStartClientCB;
    public Action OnRoomStopClientCB;
    public Action OnRoomClientSceneChangedCB;
    public Action OnRoomServerPlayersReadyCB;
    public Action OnRoomStopServerCB;

    public Action<TransportError, string> OnClientErrorCB;

    public Action<bool> OnShowPreGameCountdown;

    public override void Awake()
    {
        var oldSingleton = singleton;

        base.Awake();

        if (singleton != oldSingleton)
        {
            OnNetworkManagerChange?.Invoke();
        }
    }

    public override void OnClientError(TransportError error, string reason)
    {
        base.OnClientError(error, reason);

        OnClientErrorCB?.Invoke(error, reason);
    }

    public override void OnRoomClientDisconnect()
    {
        OnRoomStopClientCB?.Invoke();
    }

    public override void OnRoomStartClient()
    {
        OnRoomStartClientCB?.Invoke();
    }

    public override void OnRoomStartHost()
    {
        OnRoomStartHostCB?.Invoke();
    }

    public override void OnRoomClientEnter()
    {
        OnRoomClientEnterCB?.Invoke();
    }

    public override void OnRoomClientExit()
    {
        OnRoomClientExitCB?.Invoke();
    }

    public override void ServerChangeScene(string newSceneName)
    {
        this.Log("ServerChangeScene : " + newSceneName + " server active : " + NetworkServer.active);
        // override all the function, because base class is using directly SceneManager
        // and we want to use our own manager.
        // cf base class as we copied a lot of code from it.

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
        networkSceneName = newSceneName;

        // Let server prepare for scene change
        OnServerChangeScene(newSceneName);

        // set server flag to stop processing messages while changing scenes
        // it will be re-enabled in FinishLoadScene.
        NetworkServer.isLoadingScene = true;

        if (mode == NetworkManagerMode.Host) {
            // change scene.
            Access.SceneLoader().loadScene(newSceneName, new SceneLoader.LoadParams
            {
                useTransitionOut              = true,
                useLoadingScene               = true,
                useExternalLoadingSceneUnload = newSceneName == GameplayScene, // we want to remove the loading screen by-hand when every players are ready
                onSceneLoadStart = OnSceneLoadingStart,
                onSceneLoaded    = OnSceneLoadingStop
            });
        } else
        {
            Access.SceneLoader().loadScene(newSceneName, new SceneLoader.LoadParams {
                onSceneLoadStart = OnSceneLoadingStart,
                onSceneLoaded = OnSceneLoadingStop
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

    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        OnRoomServerConnect(conn);
    }

    void OnSceneLoadingStart(AsyncOperation operation) {
        loadingSceneAsync = operation;
    }

    void OnSceneLoadingStop(AsyncOperation operation) {
        this.Log("Finish loading : allowActivation = " + operation.allowSceneActivation + " isValid=" + SceneManager.GetSceneByName(networkSceneName));
    }

    public override bool OnRoomServerSceneLoadedForPlayer(NetworkConnectionToClient conn, GameObject roomPlayer, GameObject gamePlayer)
    {
        this.Log("OnRoomServerSceneLoadedForPlayer");

        OnlinePlayerController OPC = gamePlayer.GetComponent<OnlinePlayerController>();
        NetworkRoomPlayerExt nrp = roomPlayer.GetComponent<NetworkRoomPlayerExt>();
        roomplayersToGameplayersDict.Add(nrp, OPC);

        conn.Send(new SceneMessage { sceneName = Constants.SN_OPENCOURSE, sceneOperation = SceneOperation.LoadAdditive, customHandling = true });

        return true;
    }

    /// This is called on the server when a networked scene finishes loading.
    /// </summary>
    /// <param name="sceneName">Name of the new scene.</param>
    public override void OnRoomServerSceneChanged(string sceneName)
    {
        this.Log("OnRoomServerSceneChanged");


        var scene = SceneManager.GetSceneByName(sceneName);
        if (!scene.IsValid())
        {
            scene = SceneManager.GetSceneByPath(sceneName);
            if (!scene.IsValid())
            {
                this.LogError("OnRoomServerSceneChanged : cannot find scene " + sceneName);
            }
        }
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
                    Access.SceneLoader().loadScene(sceneName, new SceneLoader.LoadParams
                    {
                        useTransitionOut = true,
                        onSceneLoadStart = OnSceneLoadingStart,
                        onSceneLoaded    = OnSceneLoadingStop
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

    public void StartGameScene()
    {
        // custom loading scene.
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
                Access.SceneLoader().loadScene(sceneName, new SceneLoader.LoadParams{
                    useTransitionOut = true,
                    useTransitionIn  = true,
                    sceneLoadingMode = LoadSceneMode.Additive,
                    onSceneLoadStart = delegate (AsyncOperation op) { subsceneLoaded = false; },
                    onSceneLoaded    = delegate (AsyncOperation op) { subsceneLoaded = true ; },
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
                Access.SceneLoader().unloadScene(sceneName, new SceneLoader.UnloadParams{
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
    public void ClientLoadScene(string sceneName) {
        // for now do nothing...
    }

    [Server]
    public void loadOpenCourse() {
        ServerSceneOperation(Constants.SN_OPENCOURSE, SceneOperation.LoadAdditive);
    }

    [Server]
    public void unloadOpenCourse()
    {
        ServerSceneOperation(Constants.SN_OPENCOURSE, SceneOperation.UnloadAdditive);
    }

    [Server]
    public void loadSelectedTrial()
    {
        ServerSceneOperation(selectedTrial, SceneOperation.LoadAdditive);
    }

    [Server]
    public void unloadSelectedTrial()
    {
        ServerSceneOperation(selectedTrial, SceneOperation.UnloadAdditive);
    }

    IEnumerator LoadAdditive(string sceneName)
    {
        // host client is on server...don't load the additive scene again
        if (mode == NetworkManagerMode.ClientOnly)
        {
            subsceneLoaded = false;
            // Start loading the additive subscene
            yield return Access.SceneLoader().loadScene(sceneName, new SceneLoader.LoadParams {
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

            yield return Access.SceneLoader().unloadScene(sceneName, new SceneLoader.UnloadParams { });
            yield return Resources.UnloadUnusedAssets();

            subsceneLoaded = true;
        }

        // Reset these to false when ready to proceed
        NetworkClient.isLoadingScene = false;
    }

    public override void OnRoomStopClient()
    {
        this.Log("OnRoomClientStop");

        // TODO: make this more robust... list of current active scenes of some sort?
        // => simply do a Single load of loading scene?
        if (Mirror.Utils.IsSceneActive(GameplayScene))
        {
            Access.SceneLoader().unloadScene(Constants.SN_OPENCOURSE, new SceneLoader.UnloadParams { });
            if (selectedTrial != "") Access.SceneLoader().unloadScene(selectedTrial, new SceneLoader.UnloadParams { });
        }

        Access.GameSettings().isOnline = false;
    }

    public override void OnRoomStopServer()
    {
        this.Log("OnRoomServerStop");

        if (Mirror.Utils.IsSceneActive(GameplayScene))
        {
            Access.SceneLoader().unloadScene(Constants.SN_OPENCOURSE, new SceneLoader.UnloadParams { });
            if (selectedTrial != "") Access.SceneLoader().unloadScene(selectedTrial, new SceneLoader.UnloadParams { });
        }

        Access.GameSettings().isOnline = false;

        OnRoomStopServerCB?.Invoke();
    }


    public void clientLoadSelectedTrial()
    {

        foreach (OnlinePlayerController opc in roomplayersToGameplayersDict.Values)
        {
            NetworkConnectionToClient conn = opc.netIdentity.connectionToClient;
            if (conn == null)
                return;

            // Tell client to unload previous subscene with custom handling (see NetworkManager::OnClientChangeScene).
            conn.Send(new SceneMessage { sceneName = Constants.SN_OPENCOURSE, sceneOperation = SceneOperation.UnloadAdditive, customHandling = true });
            conn.Send(new SceneMessage { sceneName = selectedTrial, sceneOperation = SceneOperation.LoadAdditive, customHandling = true });
        }

    }

    public override void OnRoomServerPlayersReady()
    {
        if (onlineRoomData != null) {

            onlineRoomData.showPreGameCountdown = true;
            onlineRoomData.preGameCountdownTime = 5.0f;
        }

        OnRoomServerPlayersReadyCB?.Invoke();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        // HACK: should be able to cleanup the online mess easily when qutting the game.
        for (int i = 0; i < SceneManager.sceneCount; ++i)
        {
            var scene = SceneManager.GetSceneAt(i);
            if (scene.name == RoomScene || scene.path == RoomScene ||
                scene.name == GameplayScene || scene.path == GameplayScene ||
                scene.name == offlineScene || scene.path == offlineScene ||
                scene.name == onlineScene || scene.path == onlineScene)
            {
                Access.SceneLoader().unloadScene(scene.name, new SceneLoader.UnloadParams { });
            }
        }
    }
}
