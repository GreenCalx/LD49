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

    public Dictionary<NetworkRoomPlayerExt, OnlinePlayerController> roomplayersToGameplayersDict;
    public OnlineGameManager onlineGameManager;

    public ONLINE_GAME_STATE gameState = ONLINE_GAME_STATE.NONE;

    // Must be inited to true for correct AnySceneOperationOngoing()
    public bool subsceneLoaded = true;
    public bool subsceneUnloaded = true;

    public string selectedTrial = "";

    public Action OnRoomStartHostCB;
    public Action OnRoomClientEnterCB;
    public Action OnRoomClientExitCB;
    public Action OnRoomStartClientCB;
    public Action OnRoomStopClientCB;
    public Action OnRoomClientSceneChangedCB;
    public Action OnRoomServerPlayersReadyCB;
    public Action OnRoomStopServerCB;

    public Action<TransportError, string> OnClientErrorCB;

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

    public override void OnRoomClientSceneChanged()
    {
        // HACK:Check if we loaded the open course.
        // TODO: this is very bad please fix asap!
        var openCourseScene = SceneManager.GetSceneByName(Constants.SN_OPENCOURSE);
        if (openCourseScene.IsValid()) {
            SceneManager.SetActiveScene(openCourseScene);
        }
        OnRoomClientSceneChangedCB?.Invoke();
    }

    IEnumerator SeekOnlineGameManager()
    {
        while (onlineGameManager == null)
        {
            yield return null;
        }

    }

    public void StartGameScene()
    {
        ServerChangeScene(GameplayScene);
    }

    /// <summary>
    /// This is called on the server when a networked scene finishes loading.
    /// </summary>
    /// <param name="sceneName">Name of the new scene.</param>
    public override void OnRoomServerSceneChanged(string sceneName)
    {
        // spawn the initial batch of Rewards
        if (sceneName == GameplayScene)
        {
            StartCoroutine(ServerLoadOpenCourse());

            // do stuff
            Access.GameSettings().isOnline = true;
            roomplayersToGameplayersDict = new Dictionary<NetworkRoomPlayerExt, OnlinePlayerController>();
            StartCoroutine(SeekOnlineGameManager());
        }
    }

    public IEnumerator ServerLoadOpenCourse()
    {
        subsceneLoaded = false;
        yield return SceneManager.LoadSceneAsync(Constants.SN_OPENCOURSE, new LoadSceneParameters
        {
            loadSceneMode = LoadSceneMode.Additive
        });

        SceneManager.SetActiveScene(SceneManager.GetSceneByName(Constants.SN_OPENCOURSE));
        subsceneLoaded = true;
    }

    public bool AnySceneOperationOngoing()
    {
        return !NetworkRoomManagerExt.singleton.subsceneLoaded ||
                !NetworkRoomManagerExt.singleton.subsceneUnloaded;
    }

    public void unloadOpenCourse()
    {
        StartCoroutine(ServerUnloadOpenCourse());
    }

    IEnumerator ServerUnloadOpenCourse()
    {
        subsceneUnloaded = false;
        yield return SceneManager.UnloadSceneAsync(Constants.SN_OPENCOURSE);
        subsceneUnloaded = true;
    }

    public void loadSelectedTrial()
    {
        StartCoroutine(ServerLoadTrial());
    }

    IEnumerator ServerLoadTrial()
    {
        subsceneLoaded = false;
        yield return SceneManager.LoadSceneAsync(selectedTrial, new LoadSceneParameters
        {
            loadSceneMode = LoadSceneMode.Additive
        });

        subsceneLoaded = true;
    }

    public void unloadSelectedTrial()
    {
        StartCoroutine(ServerUnloadSelectedTrial());
    }

    IEnumerator ServerUnloadSelectedTrial()
    {
        subsceneUnloaded = false;
        yield return SceneManager.UnloadSceneAsync(selectedTrial);
        subsceneUnloaded = true;
    }

    public override void OnClientChangeScene(string sceneName, SceneOperation sceneOperation, bool customHandling)
    {
        if (sceneOperation == SceneOperation.UnloadAdditive)
            StartCoroutine(UnloadAdditive(sceneName));

        if (sceneOperation == SceneOperation.LoadAdditive)
            StartCoroutine(LoadAdditive(sceneName));
    }

    IEnumerator LoadAdditive(string sceneName)
    {
        // host client is on server...don't load the additive scene again
        if (mode == NetworkManagerMode.ClientOnly)
        {
            subsceneLoaded = false;
            // Start loading the additive subscene
            loadingSceneAsync = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

            while (loadingSceneAsync != null && !loadingSceneAsync.isDone)
                yield return null;
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

            yield return SceneManager.UnloadSceneAsync(sceneName);
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
        if (Mirror.Utils.IsSceneActive(GameplayScene))
        {
            SceneManager.UnloadSceneAsync(Constants.SN_OPENCOURSE);
            if (selectedTrial != "") SceneManager.UnloadSceneAsync(selectedTrial);
        }

        Access.GameSettings().isOnline = false;
    }

    public override void OnRoomStopServer()
    {
        this.Log("OnRoomServerStop");

        if (Mirror.Utils.IsSceneActive(GameplayScene))
        {
            SceneManager.UnloadSceneAsync(Constants.SN_OPENCOURSE);
            if (selectedTrial != "") SceneManager.UnloadSceneAsync(selectedTrial);
        }

        Access.GameSettings().isOnline = false;

        OnRoomStopServerCB?.Invoke();
    }

    public override bool OnRoomServerSceneLoadedForPlayer(NetworkConnectionToClient conn, GameObject roomPlayer, GameObject gamePlayer)
    {
        OnlinePlayerController OPC = gamePlayer.GetComponent<OnlinePlayerController>();
        NetworkRoomPlayerExt nrp = roomPlayer.GetComponent<NetworkRoomPlayerExt>();
        roomplayersToGameplayersDict.Add(nrp, OPC);

        conn.Send(new SceneMessage { sceneName = Constants.SN_OPENCOURSE, sceneOperation = SceneOperation.LoadAdditive, customHandling = true });

        return true;
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

    // public override GameObject OnRoomServerCreateGamePlayer(NetworkConnectionToClient conn, GameObject roomPlayer)
    // {

    // }

    bool showStartButton;

    public override void OnRoomServerPlayersReady()
    {
        OnRoomServerPlayersReadyCB?.Invoke();
        // calling the base method calls ServerChangeScene as soon as all players are in Ready state.
        //if (Mirror.Utils.IsHeadless())
        {
            //base.OnRoomServerPlayersReady();
        }
        //else
        //{
        //    showStartButton = true;
        // }
    }

    public override void OnGUI()
    {
        base.OnGUI();

        if (allPlayersReady && showStartButton && GUI.Button(new Rect(150, 300, 120, 20), "START GAME"))
        {
            // set to false to hide it in the game scene
            showStartButton = false;

            // initialize scene load status
            subsceneLoaded = true;
            subsceneUnloaded = true;
            ServerChangeScene(GameplayScene);
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        for (int i = 0; i < SceneManager.sceneCount; ++i)
        {
            var scene = SceneManager.GetSceneAt(i);
            if (scene.name == RoomScene || scene.path == RoomScene ||
                scene.name == GameplayScene || scene.path == GameplayScene ||
                scene.name == offlineScene || scene.path == offlineScene ||
                scene.name == onlineScene || scene.path == onlineScene)
            {
                Access.SceneLoader().unloadScene(scene.name);
            }
        }
    }
}
