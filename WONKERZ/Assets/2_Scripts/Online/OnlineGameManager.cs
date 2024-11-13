using Mirror;
using Schnibble;
using Schnibble.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wonkerz;


// NetworkManager is not a NetworkBehaviour! We therefore need an object for server-client synchronisation.
//
// Careful: this is a singleton, every new object will be destroyed if another one already exists.
//
// OnlineGameManager is the object use for such syncronisation when the gameplay scene is launched.
// It also act as a global UX Controller so that UX can be updated when the model changes.
//
// Sync direction : Server => client.
// Authority      : Server
//
// Client side:
//    - React to change of SyncVars and react accordingly.
//    - Set the spawnde/loaded/ready state on the server.
//
// Server side:
//    - keep track of connected player with a SyncList<OnlinePlayerController> uniquePlayers
//    - StateMachine for the game : waiting for players, launching game, post game, etc...
//

public class OnlineGameManager : NetworkBehaviour
{
    #region singleton
    [HideInInspector]
    public static OnlineGameManager _instance = null;
    public static OnlineGameManager singleton
    {
        get
        {
            // no instance yet, might have one on the NetworkManager singleton?
            if (_instance == null)
            {
                if (NetworkRoomManagerExt.singleton == null) return null;
                return NetworkRoomManagerExt.singleton.onlineGameManager;
            }
            return _instance;
        }
        private set { _instance = value; }
    }
    // Callback if the onlinegamemanager changes.
    // Should probably not be used that much?
    public static Action<OnlineGameManager> onOnlineGameManagerChanged;

    void Start()
    {
        if (_instance == null)
        {
            _instance = this;
            this.Log("OnlineGameManager : started.");
            onOnlineGameManagerChanged?.Invoke(this);
        }
        else
        {
            this.LogWarn("OnlineGameManager : an instance already exists, this one will be destroyed.");
            DestroyImmediate(this);
        }
    }

    void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
    #endregion

    [Header("UX")]
    [SerializeField]
    UIElement UIWaitForPlayers;
    [SerializeField]
    public UIPlayerOnline UIPlayer;
    [SerializeField]
    OnlineUIPostGame UIPostGame;
    [SerializeField]
    OnlineUIPauseMenu UIPauseMenu;

    [SyncVar]
    public bool openCourseLoaded = false;
    [SyncVar]
    public bool trialLoaded = false;
    [SyncVar]
    public bool openCourseUnLoaded = false;

    [Header("References")]
    public GameObject prefab_onlineTrialRoulette;

    [System.Serializable]
    public class GameSettings
    {
        public uint countdownDuration = 3; // in seconds
        public uint gameDuration = 180; // in Seconds
        public uint postGameDuration = 30;
        public uint trackEventTimeStep = 60;
        public List<string> trialPool = new List<string>() {
            "RaceTrial01",
            "MountainTrial01"
        };
        public string selectedTrial = "RaceTrial01";
    };
    public GameSettings settings;

    OnlinePlayerController _localPlayer;
    public OnlinePlayerController localPlayer
    {
        get => _localPlayer;
        set
        {
            _localPlayer = value;
            if (_localPlayer != null)
            {
                // TODO: shauld we have an event when the local player changes ?
                // sounds like we might want to listen to such a change.
                if (UIPauseMenu)
                {
                    UIPauseMenu.inputMgr = _localPlayer.self_PlayerController.inputMgr;
                }
            }
        }
    }

    public bool waitForPlayersToBeReady = false;

    public Action               onCountdownStart;
    public Action<float, float> onCountdownValue;
    public Action               onCountdownEnd;
    [SyncVar(hook=nameof(SyncCountdownElapsed))]
    float countdownElapsed      = 0f;

    void SyncCountdownElapsed(float oldValue, float newValue) {
        countdownElapsed = newValue;
        onCountdownValue?.Invoke(oldValue, newValue);
    }

    [SyncVar]
    public float gameTime;
    [SyncVar]
    public float postGameTime = 0f;
    [SyncVar]
    public bool earlyGameEnd = false;
    [SyncVar]
    public bool gameLaunched;
    [Header("Manual mand refs")]
    public OnlineTrackEventManager trackEventManager;

    [Header("Auto Refs")]
    public OnlineStartLine startLine;
    public OnlineTrialManager trialManager;

    public Action<bool> onShowUITrackTime;

    #region Server

    public enum States {
        Loading,
        PreGame,
        Countdown,
        Game,
        PreTrial,
        Trial,
        PostGame,
    };
    [SyncVar(hook=nameof(SyncState))]
    States _state;
    public States state {get;}
    public Action<States> onStateValue;

    void SyncState(States oldState, States newState) {
        if (isServer) _state = newState;
        onStateValue?.Invoke(state);
    }

    bool AreAllClientsState(States state) {
        var uniquePlayers = NetworkRoomManagerExt.singleton.roomplayersToGameplayersDict.Values;
        foreach (var opc in uniquePlayers)  if (opc.gameManagerState != state) return false;
        return true;
    }

    IEnumerator WaitForAllClientsToSwitchState(States state) {
        this.Log("WaitForAllClientsToSwitchState : " + state);
        while (!AreAllClientsState(state)) yield return null;
        this.Log("End WaitForAllClientsToSwitchState : " + state);
    }

    public override void OnStartServer()
    {
        this.Log("Start onlineGameManager : server.");
        StartCoroutine(ServerRoutine());
    }

    [Server]
    IEnumerator ServerRoutine()
    {
        NetworkRoomManagerExt manager = NetworkRoomManagerExt.singleton;
        // We start because we loaded the server scene (OpenCourseScene) as of <2024-10-18 Fri>.
        // First thing is to wait for scene to be loaded on each client
        // this way we are sure that we can expect everything to be
        // same an clients/server
        // TODO: Show something on the UI?
        // NOTE: This is only waiting for a SERVER scene change, loading
        // additive scene will not change this, cf WaitForSubSceneToBeLoadedForAllPlayers.
        yield return manager.WaitForSceneToBeLoadedForAllPlayers();

        SyncState(state, States.Loading);
        yield return WaitForAllClientsToSwitchState(state);

        // NOTE: why do we need this bool?
        Access.managers.gameSettings.isOnline = true;
        // HACK: should probably not be here...
        Access.managers.fpsLimiter.LimitFPS(false);

        yield return StartCoroutine(WaitForAllClientToSpawn    ());
        yield return StartCoroutine(WaitForAllPlayersToBeLoaded());

        RpcAllPlayersLoaded();
        FreezeAllPlayers(true);

        SyncState(state, States.PreGame);
        yield return WaitForAllClientsToSwitchState(state);

        // All players have been loaded inside the OnlineGameRoom,
        // now we load the track.
        yield return StartCoroutine(LoadScene(Constants.SN_OPENCOURSE));
        yield return StartCoroutine(WaitForSubSceneToBeLoadedForAllPlayers(Constants.SN_OPENCOURSE));
        // TODO: should we wait for all players to load the scene?
        // arg for => we are sure that everyone has the same state
        // arg against => one player might have a very slow PC and slow everyone dawn or make the server crash.

        // Finally show the scene.
        Access.managers.sceneMgr.unloadLoadingScene();
        // Do we wait for players to input Start to be ready or directly start the game.
        if (waitForPlayersToBeReady)
        {
            yield return StartCoroutine(WaitForAllPlayersToBeReady());
        }
        RpcAllPlayersLockAndLoaded();

        if (!Wonkerz.GameSettings.testMenuData.byPassTrialWheel)
        {
            yield return StartCoroutine(SpinTrialRoulette());
        }
        else
        {
            if (!string.IsNullOrEmpty(Wonkerz.GameSettings.testMenuData.trialName))
            {
                settings.selectedTrial = Wonkerz.GameSettings.testMenuData.trialName;
            }
        }

        SyncState(state, States.Countdown);
        yield return WaitForAllClientsToSwitchState(state);

        yield return StartCoroutine(StartGame());

        if (Wonkerz.GameSettings.testMenuData.byPassCourse)
        {
            gameTime = 0.0f;
        }


        SyncState(state, States.Game);
        yield return WaitForAllClientsToSwitchState(state);

        yield return StartCoroutine(GameLoop());

        if ( !OneOrLessPlayerAlive() )
        {   // TRIAL
            SyncState(state, States.PreTrial);
            yield return WaitForAllClientsToSwitchState(state);

            yield return StartCoroutine(WaitTrialSessions());

            RpcAllPlayersLockAndLoaded();

            SyncState(state, States.Countdown);
            yield return WaitForAllClientsToSwitchState(state);

            yield return StartCoroutine(Countdown());

            if (!Wonkerz.GameSettings.testMenuData.byPassTrial)
            {
                SyncState(state, States.Trial);
                yield return WaitForAllClientsToSwitchState(state);

                yield return StartCoroutine(TrialLoop());
            }
        }

        SyncState(state, States.PostGame);
        yield return WaitForAllClientsToSwitchState(state);

        yield return StartCoroutine(PostGame());
    }

    [Server]
    IEnumerator WaitForSubSceneToBeUnloadedForAllPlayers(string sceneName) {
        this.Log("WaitForSubSceneToBeLoadedForAllPlayers :" + sceneName);

        var uniquePlayers = NetworkRoomManagerExt.singleton.roomplayersToGameplayersDict.Values;
        var stop = false;
        while (!stop) {
            stop = true;
            foreach (var v in uniquePlayers)
            {
                if (v.loadedScenes.Contains(sceneName)) stop = false;
            }
            yield return null;
        }

        this.Log("End WaitForSubSceneToBeUnloadedForAllPlayers :" + sceneName);
        yield break;
    }

    [Server]
    IEnumerator WaitForSubSceneToBeLoadedForAllPlayers(string sceneName) {
        this.Log("WaitForSubSceneToBeLoadedForAllPlayers :" + sceneName);

        var uniquePlayers = NetworkRoomManagerExt.singleton.roomplayersToGameplayersDict.Values;
        var stop = false;
        while (!stop) {
            stop = true;
            foreach (var v in uniquePlayers)
            {
                if (!v.loadedScenes.Contains(sceneName)) stop = false;
            }
            yield return null;
        }

        this.Log("End WaitForSubSceneToBeLoadedForAllPlayers :" + sceneName);
        yield break;
    }

    [Server]
    IEnumerator WaitForAllClientToSpawn()
    {
        // When a client spawns, it will add itself to the uniquePlayers with AddPlayer.
        // Therefor if we dont have the same number of players in the room/lobby and spawned players there is a problem.
        // TODO:
        // Add a timeout if we cannot have all players spawed for 10s or something.
        this.Log("Waiting for clients to spawn.");
        while (!AreAllPlayersSpawned()) yield return null;
        this.Log("End waiting for clients to spawn.");
    }

    [Server]
    IEnumerator WaitForAllPlayersToBeReady()
    {
        // When a client press start to ready up it will set its isReady flag on its OnlinePlayerController
        this.Log("Waiting for clients to be ready.");
        while (!AreAllPlayersReady()) { yield return null; }
        this.Log("End waiting for clients to be ready.");
    }

    [Server]
    IEnumerator WaitForAllPlayersToBeLoaded()
    {
        // TODO:
        // We probably dant need this state anymore.
        // After a client has spawned we wait for it to be "loaded" meaning every dependencies are resolved client-side.
        // This is to avoid having Rpc sent when the client is not fully loaded.
        // This should not happen, but for now it happens sometimes because Mirror does not spown the localPlayer as if it was a real NetworkServer.Spawn I guess.
        this.Log("WaitForAllPlayersToBeLoaded.");

        while (!AreAllPlayersLoaded()) yield return null;

        this.Log("End WaitForAllPlayersToBeLoaded.");
    }

    [Server]
    IEnumerator LoadScene(string sceneName)
    {
        NetworkRoomManagerExt manager = NetworkRoomManagerExt.singleton;
        // listen for loadOpenCourse callback;
        bool sceneLoaded = false;
        Action<string> callback = delegate (string scene)
        {
            if (scene == Constants.SN_OPENCOURSE)
            {
                sceneLoaded = true;
            }
        };

        manager.OnRoomServerSceneChangedCB += callback;
        manager.ServerSceneOperation(sceneName, SceneOperation.LoadAdditive);
        while (!sceneLoaded) yield return null;
        manager.OnRoomServerSceneChangedCB -= callback;

        yield break;
    }


    [Server]
    IEnumerator StartGame()
    {
        countdownElapsed = settings.countdownDuration;

        UIPlayer.Show();

        yield return StartCoroutine(Countdown());
    }

    [Server]
    void FreezeAllPlayers(bool state)
    {
        this.Log("server: FreezeAllPlayers " + state.ToString());
        var uniquePlayers = NetworkRoomManagerExt.singleton.roomplayersToGameplayersDict.Values;
        foreach (var opc in uniquePlayers) opc.FreezePlayer(state);
    }

    [Server]
    IEnumerator WaitTrialSessions()
    {
        AskPlayersToLoad();

        FreezeAllPlayers(true);

        yield return WaitForSubSceneToBeUnloadedForAllPlayers(Constants.SN_OPENCOURSE);
        yield return WaitForSubSceneToBeLoadedForAllPlayers(settings.selectedTrial);

        while (!AreAllPlayersLoaded())
        {
            yield return null;
        }

        AskPlayersToReadyUp();
        RpcWaitForOtherPlayersToBeReady();

        while (!AreAllPlayersReady())
        {
            yield return null;
        }

        countdownElapsed = settings.countdownDuration;
    }

    IEnumerator TrialLoop()
    {
        this.Log("Start trial loop.");

        trialManager.trialLaunched = true;
        trialManager.trialIsOver = false;

        while (!trialManager.trialIsOver)
        {
            yield return null;
        }

        trialManager.trialLaunched = false;
        // player ranks availables
        this.Log("End trial loop.");
    }

    IEnumerator PostGame()
    {
        this.Log("Start post game.");
        gameLaunched = false;

        RpcPostGame();
        AskPlayersToReadyUp();
        postGameTime = settings.postGameDuration;

        // Force disconnection after postGameTime
        // If a players press starts he disconnects on his own
        while (postGameTime > 0.0f)
        {
            postGameTime -= Time.deltaTime;
            yield return null;
        }

        // shutdown server
        RpcDisconnectPlayers();

        NetworkServer.Shutdown();
    }

    [Server]
    IEnumerator Countdown()
    {
        UIWaitForPlayers.Hide();
        //FreezeAllPlayers(true);
        RpcStartCountdown();
        SyncCountdownElapsed(countdownElapsed, settings.countdownDuration);
        while (countdownElapsed > 0.0f)
        {
            SyncCountdownElapsed(countdownElapsed, countdownElapsed - Time.deltaTime);
            yield return null;
        }
        RpcEndCountdown();
    }

    [Server]
    IEnumerator GameLoop()
    {
        FreezeAllPlayers(false);

        gameTime = settings.gameDuration;
        gameLaunched = true;

        RpcShowUITrackTime(true);

        trackEventManager.nextEventTime = gameTime - settings.trackEventTimeStep;

        while (gameTime > 0)
        {
            gameTime -= Time.deltaTime;

            if (gameTime < trackEventManager.nextEventTime)
            {
                trackEventManager.SpawnEvent();
                trackEventManager.nextEventTime = gameTime - settings.trackEventTimeStep;
            }

            yield return null;
        }

        // Post Game
        if (OneOrLessPlayerAlive())
        {
            yield break; // Post Game
        }
        FreezeAllPlayers(true);
        RpcShowItsTrialTime(true);
        RpcShowUITrackTime(false);
        yield return new WaitForSeconds(2f);

        FreezeAllPlayers(false);
        UnequipAllPowers();
        RpcShowItsTrialTime(false);
        gameLaunched = false;
        yield return LoadTrialScene();
    }

    [Server]
    IEnumerator LoadTrialScene()
    {
        var manager = NetworkRoomManagerExt.singleton;
        manager.ServerSceneOperation(Constants.SN_OPENCOURSE, SceneOperation.UnloadAdditive);
        manager.ServerSceneOperation(settings.selectedTrial, SceneOperation.LoadAdditive);

        yield break;
    }

    [Server]
    IEnumerator SpinTrialRoulette()
    {
        GameObject inst = Instantiate(prefab_onlineTrialRoulette);
        inst.transform.parent = null;
        NetworkServer.Spawn(inst);

        OnlineTrialRoulette asRoulette = inst.GetComponent<OnlineTrialRoulette>();

        yield return new WaitForFixedUpdate();
        asRoulette.init(settings.trialPool);

        yield return new WaitForFixedUpdate();
        asRoulette.Spin();

        yield return new WaitForFixedUpdate();
        while (asRoulette.IsSpinning())
        {
            yield return null;
        }

        yield return new WaitForFixedUpdate();
        asRoulette.StopSpin();

        while (!asRoulette.HasSnapped)
        {
            yield return null;
        }

        yield return new WaitForFixedUpdate();
        settings.selectedTrial = asRoulette.RetrieveSelectedTrial();

        yield return new WaitForSeconds(2f);


        NetworkServer.Destroy(inst);
        Destroy(inst);
    }

    [Server]
    public void AskPlayersToReadyUp()
    {
        var uniquePlayers = NetworkRoomManagerExt.singleton.roomplayersToGameplayersDict.Values;
        // modifying the ready state is not sufficient because it is client authored.
        // we need to specifically ask players to ready up.
        foreach (var opc in uniquePlayers)
        {
            opc.SetReadyState(false);
            RpcAskToReadyUp();
        }
    }

    [Server]
    public void AskPlayersToLoad()
    {
        var uniquePlayers = NetworkRoomManagerExt.singleton.roomplayersToGameplayersDict.Values;
        foreach (var opc in uniquePlayers) opc.CmdModifyLoadedState(false);
    }

    [Server]
    public bool AreAllPlayersReady()
    {
        var uniquePlayers = NetworkRoomManagerExt.singleton.roomplayersToGameplayersDict.Values;
        foreach (var opc in uniquePlayers) if (!opc.isReady) return false;
        return true;
    }

    [Server]
    public bool AreAllPlayersLoaded()
    {
        var uniquePlayers = NetworkRoomManagerExt.singleton.roomplayersToGameplayersDict.Values;
        foreach (var opc in uniquePlayers) if (!opc.isLoaded) return false;
        return true;
    }

    [Server]
    public bool AreAllPlayersSpawned() {
        var uniquePlayers = NetworkRoomManagerExt.singleton.roomplayersToGameplayersDict.Values;
        foreach (var opc in uniquePlayers)  if (!opc.isSpawned) return false;
        return true;
    }

    [Server]
    public bool OneOrLessPlayerAlive() {
        var uniquePlayers = NetworkRoomManagerExt.singleton.roomplayersToGameplayersDict.Values;
        
        int count = 0;
        foreach (var opc in uniquePlayers) count += Convert.ToInt32(opc.IsAlive);
        return count <= 1;
    }

    [Server]
    public void UnequipAllPowers()
    {
        var uniquePlayers = NetworkRoomManagerExt.singleton.roomplayersToGameplayersDict.Values;
        foreach (var opc in uniquePlayers)
        {
            opc.self_PlayerController.self_PowerController.UnequipPower();
        }
    }

    [ServerCallback]
    public void NotifyPlayerDeath(OnlinePlayerController iOPC)
    {
        if (!NetworkRoomManagerExt.singleton.roomplayersToGameplayersDict.ContainsValue(iOPC))
            return;
        
        
        iOPC.Kill();


        Vector3 deathClonePos = iOPC.self_PlayerController.GetTransform().position;
        RpcSpawnDeathPlayerClone(deathClonePos, (iOPC.self_PlayerController.car.car as WkzCar).carType);

        if (OneOrLessPlayerAlive())
        {
            gameTime = 0f; // end course
            earlyGameEnd = true;
        } else {
            // TODO : update map bounds
            // TODO : Set dead as observer
        }
    }

    #endregion

    #region Client

    override public void OnStartClient()
    {
        var manager = NetworkRoomManagerExt.singleton;
        manager.onlineGameManager = this;

        // listen for scene loading on client, to send back to server when we
        // finished loading.
        //manager.OnRoomClientSceneChangedCB += OnClientSceneChanged;
        UIPlayer.Hide();
        UIPostGame.Hide();
        UIWaitForPlayers.Hide();
        UIPauseMenu.Hide();
    }

    [ClientRpc]
    void RpcAllPlayersLoaded()
    {
        this.Log("RpcAllPlayersLoaded.");
    }

    [ClientRpc]
    void RpcAllPlayersLockAndLoaded()
    {
        this.Log("RpcAllPlayersLockAndLoaded.");

        UIPlayer.Show();
        UIPauseMenu.Init();
    }

    [ClientRpc]
    void RpcPostGame()
    {
        this.Log("RpcPostGame");

        UIPostGame.earlyGameEnd = earlyGameEnd;
        UIPostGame.Show();
        UIWaitForPlayers.Hide();
    }

    [ClientRpc]
    void RpcWaitForOtherPlayersToBeReady()
    {
        this.Log("RpcWaitForOtherPlayersToBeReady.");

        UIWaitForPlayers.Show();
        UIPlayer.Hide();
    }

    [ClientRpc]
    void RpcAskToReadyUp()
    {
        this.Log("RpcAskToReadyUp.");

        // HACK:
        // Activate the wait for players input.
        // should probably be set on startline?
        waitForPlayersToBeReady = true;
        if (startLine == null)
        {
            this.LogError("Cannot find start line");
        }
        else
        {
            // HACK: remove and change for a real callable init function.
            startLine.gameObject.SetActive(true);
            startLine.OnStartClient();
        }
    }

    [ClientRpc]
    public void RpcDisconnectPlayers()
    {
        this.Log("RpcDisconnectPlayers.");

        NetworkClient.Disconnect();
        Access.managers.sceneMgr.loadScene(Constants.SN_TITLE, new SceneLoader.LoadParams
        {
            useTransitionOut = true,
            useTransitionIn = true,
        });
    }

    [ClientRpc]
    public void RpcStartCountdown()
    {
        this.Log("RpcStartCountdown");
        onCountdownStart?.Invoke();
    }

    [ClientRpc]
    public void RpcEndCountdown()
    {
        this.Log("RpcEndCountdown");
        onCountdownEnd?.Invoke();
    }

    #if false
    [ClientRpc]
    public void RpcUpdateStartLineCountdown(float iCooldownElapsed)
    {
        if (startLine == null)
        {
            this.LogError("Starting OnlineStartLine but startLine is null.");
            return;
        }
        startLine.CountdownUpdate(iCooldownElapsed);
    }
    #endif

    [ClientRpc]
    public void RpcShowUITrackTime(bool iState)
    {
        this.Log("RpcShowUITrackTime.");

        UIPlayer.showTrackTime = iState;
    }

    [ClientRpc]
    public void RpcShowItsTrialTime(bool iState)
    {
        if (iState)
        UIPlayer.ItsTrialTimeHandle.Show();
        else
        UIPlayer.ItsTrialTimeHandle.Hide();
    }

    [ClientRpc]
    public void RpcSpawnDeathPlayerClone(Vector3 iSpawnPos, CAR_TYPE iCarType)
    {
        GameObject deathClonePrefab = Access.managers.carBanks.GetDeathClonePrefab(iCarType);
        GameObject deathClone = Instantiate(deathClonePrefab);
        deathClone.transform.parent = null;
        deathClone.transform.position = iSpawnPos;
    }

    #endregion
}
