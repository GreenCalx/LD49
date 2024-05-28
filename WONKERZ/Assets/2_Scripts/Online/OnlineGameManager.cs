using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

using Mirror;

public class OnlineGameManager : NetworkBehaviour
{
    [SyncVar]
    public bool openCourseLoaded = false;
    [SyncVar]
    public bool trialLoaded = false;
    [SyncVar]
    public bool openCourseUnLoaded = false;

    [Header("Tweaks")] // not the right place ?
    public uint countdown; // in seconds
    public uint gameDuration = 180; // in Seconds
    public string selectedTrial = "RaceTrial01";
    [Header("INTERNALS")]
    public SyncList<OnlinePlayerController> uniquePlayers  = new SyncList<OnlinePlayerController>();
    public int expectedPlayersFromLobby;
    [SyncVar]
    public bool allPlayersLoadedInLobby = false;
    [SyncVar]
    public float countdownElapsed = 0f;
    [SyncVar]
    public float gameTime;
    [SyncVar]
    public bool gameLaunched;

    public OnlineTrialManager trialManager;

    // Start is called before the first frame update
    void Start()
    {
        allPlayersLoadedInLobby = false;

        NetworkRoomManagerExt.singleton.onlineGameManager = this;
        expectedPlayersFromLobby = NetworkRoomManagerExt.singleton.roomSlots.Count;
        if (isServer)
            StartCoroutine(StartOnlineGame());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddPlayer(OnlinePlayerController iOPC)
    {
        if (!uniquePlayers.Contains(iOPC))
        {
            uniquePlayers.Add(iOPC);
        }
    }

    [Command]
    public void CmdFreezePlayers(bool iState)
    {
        if (iState)
            Access.Player().Freeze();
        else
            Access.Player().UnFreeze();
    }

    IEnumerator StartOnlineGame()
    {
        yield return StartCoroutine(WaitSessions());
        yield return StartCoroutine(Countdown());
        yield return StartCoroutine(GameLoop());
        yield return StartCoroutine(WaitTrialSessions());
        yield return StartCoroutine(Countdown());
        yield return StartCoroutine(TrialLoop());

    }

    IEnumerator WaitTrialSessions()
    {
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

        while (trialManager==null)
        {
            yield return null;
        }

        Access.OfflineGameManager().startLine = trialManager.onlineStartLine;
        //Access.CameraManager().changeCamera(GameCamera.CAM_TYPE.ORBIT, false);
    }

    IEnumerator TrialLoop()
    {
        while(!trialManager.trialIsOver)
        {
            yield return null;
        }
    }

    IEnumerator WaitSessions()
    {
        while(!openCourseLoaded)
        {
            openCourseLoaded = NetworkRoomManagerExt.singleton.subsceneLoaded;
            yield return null;
        }
        while (uniquePlayers.Count != expectedPlayersFromLobby)
        {
            yield return null;
        }
        allPlayersLoadedInLobby = true;


    }

    IEnumerator Countdown()
    {
        countdownElapsed = 0f;
        RpcLaunchOnlineStartLine();
        while (countdownElapsed < countdown)
        {
            countdownElapsed += Time.deltaTime;
            yield return null;
        }

        // track is on !
        gameTime = gameDuration;
        gameLaunched = true;
    }

    [ClientRpc]
    public void RpcRefreshOfflineGameMgr()
    {
        OfflineGameManager offgm = Access.OfflineGameManager();
        offgm.refreshSession();
    }

    [ClientRpc]
    public void RpcLaunchOnlineStartLine()
    {
        OfflineGameManager offgm = Access.OfflineGameManager();
        offgm.startLine.init(Access.Player());
        offgm.startLine.launchCountdown();
    }

    [ClientRpc]
    public void RpcShowUITrackTime(bool iState)
    {
        Access.UIPlayerOnline().showTrackTime = iState;
    }

    IEnumerator GameLoop()
    {
        RpcShowUITrackTime(true);
        while (gameTime > 0)
        {
            gameTime -= Time.deltaTime;
            yield return null;
        }

        // Post Game
        RpcShowUITrackTime(false);

        yield return LoadTrialScene();
        //yield return SendPlayersToTrial();

    }

    IEnumerator LoadTrialScene()
    {   
        //CmdFreezePlayers(true);

        // Unload open course on server and load trial
        NetworkRoomManagerExt.singleton.unloadOpenCourse();

        NetworkRoomManagerExt.singleton.selectedTrial = selectedTrial;
        NetworkRoomManagerExt.singleton.loadSelectedTrial();    
        // launch transition to trial on clients from server
        NetworkRoomManagerExt.singleton.clientLoadSelectedTrial();

        if (isServer)
            RpcRefreshOfflineGameMgr();

        // while ( !trialLoaded || !openCourseUnLoaded )
        // {
        //     yield return null;
        // }   
        // Access.CameraManager().changeCamera(GameCamera.CAM_TYPE.INIT, false);
        yield break;
    }
}
