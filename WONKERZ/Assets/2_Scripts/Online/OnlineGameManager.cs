using Mirror;
using System.Collections;
using System;
using UnityEngine;
using Wonkerz;

using Schnibble;

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
    public static OnlineGameManager Get() {
        if (NetworkRoomManagerExt.singleton == null) return null;
        return NetworkRoomManagerExt.singleton.onlineGameManager;
    }

    [Header("UX")]
    [SerializeField]
    GameObject     UIWaitForPlayers;
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

    [Header("Tweaks")] // not the right place ?
    public uint countdown; // in seconds
    public uint gameDuration = 180; // in Seconds
    public uint postGameDuration = 30;
    public uint trackEventTimeStep = 60;
    public string selectedTrial = "RaceTrial01";
    [Header("INTERNALS")]
    public readonly SyncList<OnlinePlayerController> uniquePlayers = new SyncList<OnlinePlayerController>();
    public OnlinePlayerController localPlayer;
    public int expectedPlayersFromLobby;

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
    IEnumerator ServerRoutine() {
        yield return StartCoroutine(WaitForAllClientToSpawn());
        yield return StartCoroutine(WaitForAllPlayersToBeLoaded());
        
        RpcAllPlayersLoaded();
        FreezeAllPlayers(true);

        yield return StartCoroutine(WaitForAllPlayersToBeReady());

        RpcAllPlayersLockAndLoaded();

        yield return StartCoroutine(StartGame());
        yield return StartCoroutine(GameLoop());
        yield return StartCoroutine(WaitTrialSessions());
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
        while (!AreAllPlayersReady()) {yield return null;}
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
        while (!AreAllPlayersLoaded()) {yield return null;}
        this.Log("AllClientLoaded.");
    }

    [Server]
    IEnumerator StartGame() {
        countdownElapsed = 0f;

        UIPlayer.gameObject.SetActive(true);

        yield return StartCoroutine(Countdown());
    }

    [Server]
    void FreezeAllPlayers(bool state) {
        this.Log("server: FreezeAllPlayers " + state.ToString());
        foreach(var opc in uniquePlayers) opc.FreezePlayer(state);
    }

    [Server]
    IEnumerator WaitTrialSessions()
    {
        AskPlayersToLoad();
        foreach(OnlinePlayerController opc in uniquePlayers) { opc.RpcLoadSubScene(); }

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

        //RpcNotifyOfflineMgrAllPlayersLoaded();

        AskPlayersToReadyUp();
        while (!AreAllPlayersReady())
        {
            yield return null;
        }

        countdownElapsed = 0f;
    }

    IEnumerator TrialLoop()
    {
        trialManager.trialLaunched = true;

        while (!trialManager.trialIsOver)
        {
            yield return null;
        }

        trialManager.trialLaunched = false;
        // player ranks availables

    }

    IEnumerator PostGame()
    {
        gameLaunched = false;

        RpcPostGame();

        AskPlayersToReadyUp();
        postGameTime = postGameDuration;
        while (!AreAllPlayersReady())
        {
            postGameTime -= Time.deltaTime;
            if (postGameTime <= 0f)
            {
                break;
            }

            foreach (var opc in uniquePlayers)
            if (opc != null && opc.connectionToClient != null) opc.connectionToClient.Disconnect();
            yield return null;
        }

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
    public void AddPlayer(OnlinePlayerController iOPC)
    {
        // When a localPlayer is Spawned an the client at will call this function.
        if (!uniquePlayers.Contains(iOPC)) {uniquePlayers.Add(iOPC);}
    }

    [Server]
    public void AskPlayersToReadyUp()
    {
        foreach (var opc in uniquePlayers) opc.CmdModifyReadyState(false);
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

    override public void OnStartClient() {
        NetworkRoomManagerExt.singleton.onlineGameManager = this;

        UIPlayer.gameObject.SetActive(false);
        UIPostGame.gameObject.SetActive(false);
        UIWaitForPlayers.gameObject.SetActive(true);
        // Do we really want the menu to be activable during waiting of players?
        UIPauseMenu.gameObject.SetActive(true);
    }

    [ClientRpc]
    void RpcAllPlayersLoaded() {
        UIPostGame.gameObject.SetActive(false);
        UIPostGame.updatePlayerRankingsLbl(this);
    }

    [ClientRpc]
    void RpcAllPlayersLockAndLoaded() {
        UIWaitForPlayers.SetActive(false);
        UIPlayer.gameObject.SetActive(true);
    }

    [ClientRpc]
    void RpcPostGame() { 
        UIPostGame.gameObject.SetActive(true);
        UIWaitForPlayers.gameObject.SetActive(false);
    }

    [ClientRpc]
    void WaitForOtherPlayersToBeReady() {
        UIWaitForPlayers.SetActive(true);
        UIPlayer.gameObject.SetActive(false);
    }

    [ClientRpc]
    public void RpcDisconnectPlayers()
    {
        NetworkClient.Disconnect();
        Access.SceneLoader().loadScene(Constants.SN_TITLE);
    }

    [ClientRpc]
    public void RpcLaunchOnlineStartLine()
    {
        if (startLine == null) {
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
