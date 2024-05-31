using System.Collections;
using System.Collections.Generic;
using System.Linq; 
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
    public SyncDictionary<OnlinePlayerController, bool> PlayersReadyDict = new SyncDictionary<OnlinePlayerController, bool>();
    public SyncDictionary<OnlinePlayerController, bool> PlayersLoadedDict = new SyncDictionary<OnlinePlayerController, bool>();
    public int expectedPlayersFromLobby;
    // [SyncVar]
    // public bool allPlayersLoadedInLobby = false;
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
        //allPlayersLoadedInLobby = false;

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
        if (!PlayersReadyDict.ContainsKey(iOPC))
        {
            PlayersReadyDict.Add(iOPC, false);
        }
        if (!PlayersLoadedDict.ContainsKey(iOPC))
        {
            PlayersLoadedDict.Add(iOPC, false);
        }
    }

    [ServerCallback]
    public void AskPlayersToReadyUp()
    {
        foreach ( OnlinePlayerController opc in PlayersReadyDict.Keys.ToList())
        {
            PlayersReadyDict[opc] = false;
        }
        //PlayersReadyDict.Keys.ForEach(e => PlayersReadyDict[e] = false);
    }

    [ServerCallback]
    public void AskPlayersToLoad()
    {
        foreach (OnlinePlayerController opc in PlayersLoadedDict.Keys.ToList())
        {
            PlayersLoadedDict[opc] = false;
        }
    }

    [ServerCallback]
    public bool ArePlayersReady()
    {
        bool retval = true;
        foreach ( OnlinePlayerController opc in PlayersReadyDict.Keys)
        {
            retval &= PlayersReadyDict[opc];
            if (!retval)
                return retval;
        }
        return retval;
    }

    [ServerCallback]
    public bool AllPlayersLoaded()
    {
        bool retval = true;
        foreach (OnlinePlayerController opc in PlayersLoadedDict.Keys)
        {
            retval &= PlayersLoadedDict[opc];
            if (!retval)
                return retval;
        }
        return retval;
    }

    [ServerCallback]
    public void NotifyPlayerIsReady(OnlinePlayerController iOPC, bool iState)
    {
        if (!PlayersReadyDict.ContainsKey(iOPC))
        {
            Debug.Log("OnlineGameManager::CmdNotifyPlayerIsReady() | Player is not registered");
            AddPlayer(iOPC);
            //return;
        }
        PlayersReadyDict[iOPC] = iState;
    }

    [ServerCallback]
    public void NotifyPlayerHasLoaded(OnlinePlayerController iOPC, bool iState)
    {
        if (!PlayersLoadedDict.ContainsKey(iOPC))
        {
            Debug.Log("OnlineGameManager::NotifyPlayerHasLoaded | Player is not registered");
            //return;
            AddPlayer(iOPC);
        }
        PlayersLoadedDict[iOPC] = iState;
    }

    [ClientRpc]
    public void RpcFreezePlayers(bool iState)
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
        //allPlayersLoadedInLobby = false;

        // if (isServer)
            AskPlayersToLoad();

        // if (isServer)
            RpcFreezePlayers(true);

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

        // DEAD CODE : Coroutine is server only
        // if (isClient)
        // {
        //     OfflineGameManager OGM = Access.OfflineGameManager();
        //     while (OGM == null)
        //     {
        //         yield return null;
        //     }
        //     while (trialManager == null)
        //     {
        //         yield return null;
        //     }
        //     OGM.startLine = trialManager.onlineStartLine;

        //     while (!OGM.sessionIsReadyToGo)
        //     {
        //         yield return null;
        //     }

        //     NotifyPlayerHasLoaded(OGM.localPlayer, true);
        // }

        //Access.CameraManager().changeCamera(GameCamera.CAM_TYPE.ORBIT, false);
        //allPlayersLoadedInLobby = true;
        // if (isServer)
        // {
            while (!AllPlayersLoaded())
            {
                yield return null;
            }
        // }

        //if (isServer)
        RpcNotifyOfflineMgrAllPlayersLoaded();

        AskPlayersToReadyUp();
        while (!ArePlayersReady())
        {
            yield return null;
        }

        countdownElapsed = 0f;
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

        // DEAD CODE - Coroutinne is server only
        // if (isClient)
        // {
        //     OfflineGameManager OGM = Access.OfflineGameManager();
        //     while (OGM == null)
        //     {
        //         yield return null;
        //     }
        //     while ( !OGM.sessionIsReadyToGo)
        //     {
        //         yield return null;
        //     }
        //     if (isServer)
        //         NotifyPlayerHasLoaded(OGM.localPlayer, true);
        //     else if (isClientOnly)
        //         CmdNotifyPlayerHasLoaded(OGM.localPlayer, true);
        // }

        //allPlayersLoadedInLobby = true;
        while(!AllPlayersLoaded())
        {
            yield return null;
        }
        RpcNotifyOfflineMgrAllPlayersLoaded();

        while (!ArePlayersReady())
        {
            yield return null;
        }
    }

    IEnumerator Countdown()
    {
        countdownElapsed = 0f;

        // while (Access.OfflineGameManager().startLine==null)
        // { yield return null; }
        RpcLaunchOnlineStartLine();
        
        while (countdownElapsed < countdown)
        {
            countdownElapsed += Time.deltaTime;
            yield return null;
        }

        // track is on !
        if (isServer)
            RpcFreezePlayers(false);
        gameTime = gameDuration;
        gameLaunched = true;


    }

    [ClientRpc]
    public void RpcNotifyOfflineMgrAllPlayersLoaded()
    {
        OfflineGameManager offgm = Access.OfflineGameManager();
        offgm.allPlayersHaveLoaded = true;
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
        //offgm.startLine.init(offgm.localPlayer.self_PlayerController);
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

        gameLaunched = false;
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
