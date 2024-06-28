using System.Collections;
using System.Collections.Generic;
using System.Linq; 
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

using Mirror;

using Wonkerz;

public class OnlineGameManager : NetworkBehaviour
{
    [SyncVar]
    public bool openCourseLoaded = false;
    [SyncVar]
    public bool trialLoaded = false;
    [SyncVar]
    public bool openCourseUnLoaded = false;

    [Header("References")]
    public OnlineUIPostGame uiPostGame_sceneObject;

    [Header("Tweaks")] // not the right place ?
    public uint countdown; // in seconds
    public uint gameDuration = 180; // in Seconds
    public uint postGameDuration = 30;
    public string selectedTrial = "RaceTrial01";
    [Header("INTERNALS")]
    public SyncList<OnlinePlayerController> uniquePlayers  = new SyncList<OnlinePlayerController>();
    public SyncDictionary<OnlinePlayerController, bool> PlayersReadyDict = new SyncDictionary<OnlinePlayerController, bool>();
    public SyncDictionary<OnlinePlayerController, bool> PlayersLoadedDict = new SyncDictionary<OnlinePlayerController, bool>();
    public int expectedPlayersFromLobby;

    [SyncVar]
    public float countdownElapsed = 0f;
    [SyncVar]
    public float gameTime;
    [SyncVar]
    public float postGameTime = 0f;
    [SyncVar]
    public bool gameLaunched;

    public OnlineTrialManager trialManager;

    // Start is called before the first frame update
    void Start()
    {

        NetworkRoomManagerExt.singleton.onlineGameManager = this;
        expectedPlayersFromLobby = NetworkRoomManagerExt.singleton.roomSlots.Count;
        if (isServer)
        {
            
            StartCoroutine(StartOnlineGame());
        }
            
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
        if (PlayersLoadedDict.Count == 0 )
            return false;

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

    [ServerCallback]
    public bool HasPlayerLoaded(OnlinePlayerController iOPC)
    {
        if (PlayersLoadedDict.ContainsKey(iOPC))
            return PlayersLoadedDict[iOPC];
        return false;
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
        yield return StartCoroutine(PostGame());

    }

    IEnumerator WaitTrialSessions()
    {
        AskPlayersToLoad();
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

            while (!AllPlayersLoaded())
            {
                yield return null;
            }

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
        trialManager.trialLaunched = true;

        while(!trialManager.trialIsOver)
        {
            yield return null;
        }

        trialManager.trialLaunched = false;
        // player ranks availables

    }

    IEnumerator PostGame()
    {
        gameLaunched = false;
        RpcDisplayPostGameUI(true);

        AskPlayersToReadyUp();
        postGameTime = postGameDuration;
        while (!ArePlayersReady())
        {
            postGameTime -= Time.deltaTime;
            if (postGameTime <= 0f )
            {
                break;
            }

            foreach(OnlinePlayerController opc in PlayersReadyDict.Keys.ToList())
            {
                if (PlayersReadyDict[opc])
                {
                    opc.connectionToClient.Disconnect();
                }
            }
            yield return null;
        }

        //unload
        NetworkRoomManagerExt.singleton.unloadSelectedTrial();

        // shutdown server
        RpcDisconnectPlayers();
        NetworkServer.Shutdown();
    }

    IEnumerator WaitSessions()
    {
        while(!openCourseLoaded)
        {
            openCourseLoaded = NetworkRoomManagerExt.singleton.subsceneLoaded;
            
            yield return null;
        }
        RpcDisplayPostGameUI(false);
        RpcFreezePlayers(true);

        while (uniquePlayers.Count != expectedPlayersFromLobby)
        {
            yield return null;
        }


        while(!AllPlayersLoaded())
        {
            yield return null;
        }
        RpcNotifyOfflineMgrAllPlayersLoaded();

        while (!ArePlayersReady())
        {
            yield return null;
        }

        countdownElapsed = 0f;
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
        if (isServer)
            RpcFreezePlayers(false);
        gameTime = gameDuration;
        gameLaunched = true;


    }

    [ClientRpc]
    public void RpcDisconnectPlayers()
    {
        NetworkClient.Disconnect();
        Access.SceneLoader().loadScene(Constants.SN_TITLE);
    }

    [ClientRpc]
    public void RpcDisplayPostGameUI(bool iState)
    {
        uiPostGame_sceneObject.gameObject.SetActive(iState);
        uiPostGame_sceneObject.updatePlayerRankingsLbl(this);
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

    }

    IEnumerator LoadTrialScene()
    {   

        // Unload open course on server and load trial
        NetworkRoomManagerExt.singleton.unloadOpenCourse();

        NetworkRoomManagerExt.singleton.selectedTrial = selectedTrial;
        NetworkRoomManagerExt.singleton.loadSelectedTrial();    
        // launch transition to trial on clients from server
        NetworkRoomManagerExt.singleton.clientLoadSelectedTrial();

        if (isServer)
            RpcRefreshOfflineGameMgr();

        yield break;
    }
}
