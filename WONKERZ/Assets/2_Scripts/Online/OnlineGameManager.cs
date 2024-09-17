using Mirror;
using Schnibble;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wonkerz;

// Network object specs
//
// description : This is the main manager. It is effectively a singleton that will manage
//               the syncronisation of every game state, like who is connected, loaded, countdown, track start, track change, etc.
//
// authority : server
//
// Client side:
//    - Manage UX related states: when all player are loaded, ready, etc...
//
// Server side:
//    - keep track of connected player with a SyncList<OnlinePlayerController> uniquePlayers
//    - Start the ServerRoutine which will wait for players state and act accordingly => start game, start trial, etc. 
//

public class OnlineGameManager : NetworkBehaviour
{
    // TODO:
    // Remove this and create a real singleton as we shauld have only one OnlineGameManager.
    public static OnlineGameManager Get()
    {
        if (NetworkRoomManagerExt.singleton == null) return null;
        return NetworkRoomManagerExt.singleton.onlineGameManager;
    }

    [Header("UX")]
    [SerializeField]
    GameObject UIWaitForPlayers;
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

    [Header("Tweaks")] // not the right place ?
    public uint countdown; // in seconds
    public uint gameDuration = 180; // in Seconds
    public uint postGameDuration = 30;
    public uint trackEventTimeStep = 60;
    public List<string> trialPool = new List<string>() { "RaceTrial01", "MountainTrial01" };
    public string selectedTrial = "RaceTrial01";

    [Header("INTERNALS")]
    public readonly SyncList<OnlinePlayerController> uniquePlayers = new SyncList<OnlinePlayerController>();
    public OnlinePlayerController localPlayer;
    public int expectedPlayersFromLobby;

    public bool waitForPlayersToBeReady = false;

    [SyncVar]
    public float countdownElapsed = 0f;
    [SyncVar]
    public float gameTime;
    [SyncVar]
    public float postGameTime = 0f;
    [SyncVar]
    public bool gameLaunched;
    [Header("Manual mand refs")]
    public OnlineTrackEventManager trackEventManager;
    [Header("Auto Refs")]
    public OnlineStartLine startLine;
    public OnlineTrialManager trialManager;

    public Action<bool> onShowUITrackTime;

    /* ----------------------------------
    Server
    ------------------------------------ */

    public override void OnStartServer()
    {
        this.Log("Start onlineGameManager : server.");
        // TODO: remove this as we are already kinda a singleton ourselves.
        expectedPlayersFromLobby = NetworkRoomManagerExt.singleton.roomSlots.Count;
        NetworkRoomManagerExt.singleton.onlineGameManager = this;

        StartCoroutine(ServerRoutine());
    }

    [Server]
    IEnumerator ServerRoutine()
    {
        yield return StartCoroutine(WaitForAllClientToSpawn    ());
        yield return StartCoroutine(WaitForAllPlayersToBeLoaded());
        // All players have been loaded inside the OnlineGameRoom,
        // now we load the track.
        NetworkRoomManagerExt.singleton.loadOpenCourse();
        // wait for the track to be loaded.
        while(!NetworkRoomManagerExt.singleton.subsceneLoaded) yield return null;
        // do stuff
        Access.GameSettings().isOnline = true;

        RpcAllPlayersLoaded();
        FreezeAllPlayers(true);

        // Show the scene.
        Access.SceneLoader().unloadLoadingScene();

        if (waitForPlayersToBeReady)
        {
            yield return StartCoroutine(WaitForAllPlayersToBeReady());
        }

        RpcAllPlayersLockAndLoaded();

        yield return StartCoroutine(SpinTrialRoulette());
        yield return StartCoroutine(StartGame());
        yield return StartCoroutine(GameLoop());
        yield return StartCoroutine(WaitTrialSessions());

        RpcAllPlayersLockAndLoaded();

        yield return StartCoroutine(Countdown());
        yield return StartCoroutine(TrialLoop());
        yield return StartCoroutine(PostGame());
    }

    [Server]
    IEnumerator WaitForAllClientToSpawn()
    {
        // When a client spawns, it will add itself to the uniquePlayers with AddPlayer.
        // Therefor if we dont have the same number of players in the room/lobby and spawned players there is a problem.
        // TODO:
        // Add a timeout if we cannot have all players spawed for 10s or something.
        this.Log("Waiting for client to spawn.");
        while (uniquePlayers.Count != expectedPlayersFromLobby)
        {
            this.Log("Number of client spawned: " + uniquePlayers.Count);
            yield return null;
        }
        this.Log("AllClientSpawned." + uniquePlayers.Count);
    }

    [Server]
    IEnumerator WaitForAllPlayersToBeReady()
    {
        // When a client press start to ready up it will set its isReady flag on its OnlinePlayerController
        this.Log("Waiting for clients to be ready.");
        while (!AreAllPlayersReady()) { yield return null; }
        this.Log("AllClientReady.");
    }

    [Server]
    IEnumerator WaitForAllPlayersToBeLoaded()
    {
        // TODO:
        // We probably dant need this state anymore.
        // After a client has spawned we wait for it to be "loaded" meaning every dependencies are resolved client-side.
        // This is to avoid having Rpc sent when the client is not fully loaded.
        // This should not happen, but for now it happens sometimes because Mirror does not spown the localPlayer as if it was a real NetworkServer.Spawn I guess.
        this.Log("Waiting for clients to be loaded.");
        while (!AreAllPlayersLoaded()) { yield return null; }
        this.Log("AllClientLoaded.");
    }

    [Server]
    IEnumerator StartGame()
    {
        countdownElapsed = 0f;

        UIPlayer.gameObject.SetActive(true);

        yield return StartCoroutine(Countdown());
    }

    [Server]
    void FreezeAllPlayers(bool state)
    {
        this.Log("server: FreezeAllPlayers " + state.ToString());
        foreach (var opc in uniquePlayers) opc.FreezePlayer(state);
    }

    [Server]
    IEnumerator WaitTrialSessions()
    {
        AskPlayersToLoad();
        foreach (OnlinePlayerController opc in uniquePlayers) { opc.RpcLoadSubScene(); }

        FreezeAllPlayers(true);

        while (!openCourseUnLoaded)
        {
            openCourseUnLoaded = NetworkRoomManagerExt.singleton.subsceneUnloaded;
            yield return null;
        }
        while (!trialLoaded)
        {
            trialLoaded = NetworkRoomManagerExt.singleton.subsceneLoaded;
            yield return null;
        }

        while (!AreAllPlayersLoaded())
        {
            yield return null;
        }

        AskPlayersToReadyUp();
        WaitForOtherPlayersToBeReady();

        while (!AreAllPlayersReady())
        {
            yield return null;
        }

        countdownElapsed = 0f;
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
        postGameTime = postGameDuration;

        // Wait until either postGameTime is elapsed, or all players want to quit.
        while (!AreAllPlayersReady() || postGameTime > 0.0f)
        {
            postGameTime -= Time.deltaTime;
            yield return null;
        }

        this.Log("End post game.");

        //unload
        NetworkRoomManagerExt.singleton.unloadSelectedTrial();
        // shutdown server
        RpcDisconnectPlayers();
        NetworkServer.Shutdown();
    }


    [Server]
    IEnumerator WaitSessions()
    {
        // Wait to load all scene.
        while (!openCourseLoaded)
        {
            openCourseLoaded = NetworkRoomManagerExt.singleton.subsceneLoaded;

            yield return null;
        }

        FreezeAllPlayers(true);

        while (uniquePlayers.Count != expectedPlayersFromLobby)
        {
            yield return null;
        }


        while (!AreAllPlayersLoaded())
        {
            yield return null;
        }

        //RpcNotifyOfflineMgrAllPlayersLoaded();

        while (!AreAllPlayersReady())
        {
            yield return null;
        }

        countdownElapsed = 0f;
    }

    [Server]
    IEnumerator Countdown()
    {
        countdownElapsed = 0f;
        //FreezeAllPlayers(true);

        RpcLaunchOnlineStartLine();

        while (countdownElapsed < countdown)
        {
            countdownElapsed += Time.deltaTime;
            yield return null;
        }

        FreezeAllPlayers(false);

        gameTime = gameDuration;
        gameLaunched = true;
    }

    [Server]
    IEnumerator GameLoop()
    {
        RpcShowUITrackTime(true);

        trackEventManager.nextEventTime = gameTime - trackEventTimeStep;

        while (gameTime > 0)
        {
            gameTime -= Time.deltaTime;

            if (gameTime < trackEventManager.nextEventTime)
            {
                trackEventManager.SpawnEvent();
                trackEventManager.nextEventTime = gameTime - trackEventTimeStep;
            }

            yield return null;
        }

        // Post Game
        RpcShowUITrackTime(false);

        gameLaunched = false;
        yield return LoadTrialScene();

    }

    [Server]
    IEnumerator LoadTrialScene()
    {

        // Unload open course on server and load trial
        NetworkRoomManagerExt.singleton.unloadOpenCourse();

        NetworkRoomManagerExt.singleton.selectedTrial = selectedTrial;
        NetworkRoomManagerExt.singleton.loadSelectedTrial();
        // launch transition to trial on clients from server
        NetworkRoomManagerExt.singleton.clientLoadSelectedTrial();

        //if (isServer)
        //RpcRefreshOfflineGameMgr();

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
        asRoulette.init(trialPool);

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
        selectedTrial = asRoulette.RetrieveSelectedTrial();

        yield return new WaitForSeconds(2f);


        NetworkServer.Destroy(inst);
        Destroy(inst);
    }

    [Server]
    public void AddPlayer(OnlinePlayerController iOPC)
    {
        // When a localPlayer is Spawned an the client at will call this function.
        if (!uniquePlayers.Contains(iOPC)) { uniquePlayers.Add(iOPC); }
    }

    [Server]
    public void AskPlayersToReadyUp()
    {
        // modifying the ready state is not sufficient because it is client authored.
        // we need to specifically ask players to ready up.
        foreach (var opc in uniquePlayers)
        {
            opc.SetReadyState(false);
            ServerAskToReadyUp();
        }
    }

    [Server]
    public void AskPlayersToLoad()
    {
        foreach (var opc in uniquePlayers) opc.CmdModifyLoadedState(false);
    }

    [Server]
    public bool AreAllPlayersReady()
    {
        if (uniquePlayers.Count < expectedPlayersFromLobby)
            return false;

        foreach (var opc in uniquePlayers) if (!opc.IsReady()) return false;

        return true;
    }

    [Server]
    public bool AreAllPlayersLoaded()
    {
        if (uniquePlayers.Count < expectedPlayersFromLobby)
            return false;

        foreach (var opc in uniquePlayers) if (!opc.IsLoaded()) return false;
        return true;
    }


    /* ----------------------------------
    Client
    ------------------------------------ */

    override public void OnStartClient()
    {
        NetworkRoomManagerExt.singleton.onlineGameManager = this;

        UIPlayer.gameObject.SetActive(false);
        UIPostGame.gameObject.SetActive(false);
        UIWaitForPlayers.gameObject.SetActive(true);
        // Do we really want the menu to be activable during waiting of players?
        UIPauseMenu.gameObject.SetActive(true);
    }

    [ClientRpc]
    void RpcAllPlayersLoaded()
    {
        UIPostGame.gameObject.SetActive(false);
        UIPostGame.updatePlayerRankingsLbl(this);
    }

    [ClientRpc]
    void RpcAllPlayersLockAndLoaded()
    {
        this.Log("RpcAllPlayersLockAndLoaded.");
        UIWaitForPlayers.SetActive(false);
        UIPlayer.gameObject.SetActive(true);
    }

    [ClientRpc]
    void RpcPostGame()
    {
        UIPostGame.gameObject.SetActive(true);
        UIWaitForPlayers.gameObject.SetActive(false);
    }

    [ClientRpc]
    void WaitForOtherPlayersToBeReady()
    {
        UIWaitForPlayers.SetActive(true);
        UIPlayer.gameObject.SetActive(false);
    }

    [ClientRpc]
    void ServerAskToReadyUp()
    {
        this.Log("ServerAskToReadyUp.");
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
        NetworkClient.Disconnect();
        Access.SceneLoader().loadScene(Constants.SN_TITLE, new SceneLoader.LoadParams
        {
            useTransitionOut = true,
            useTransitionIn = true,
        });
    }

    [ClientRpc]
    public void RpcLaunchOnlineStartLine()
    {
        if (startLine == null)
        {
            this.LogError("Starting OnlineStartLine but object is null.");
            return;
        }
        startLine.LaunchCountdown();
    }

    [ClientRpc]
    public void RpcShowUITrackTime(bool iState)
    {
        UIPlayer.showTrackTime = iState;
    }
}
