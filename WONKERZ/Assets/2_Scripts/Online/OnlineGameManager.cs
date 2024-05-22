using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using Mirror;

public class OnlineGameManager : NetworkBehaviour
{

    [Header("Tweaks")]
    public uint countdown; // in seconds
    public uint gameDuration = 180; // in Seconds
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

    IEnumerator StartOnlineGame()
    {
        yield return StartCoroutine(WaitSessions());
        yield return StartCoroutine(Countdown());
        yield return StartCoroutine(GameLoop());
    }

    IEnumerator WaitSessions()
    {
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
        gameTime = 0f;
        gameLaunched = true;
    }

    [ClientRpc]
    public void RpcLaunchOnlineStartLine()
    {
        OfflineGameManager offgm = Access.OfflineGameManager();
        offgm.startLine.launchCountdown();
    }

    IEnumerator GameLoop()
    {
        while (gameTime < gameDuration)
        {
            gameTime += Time.deltaTime;
            yield return null;
        }

        // Post Game

    }
}
