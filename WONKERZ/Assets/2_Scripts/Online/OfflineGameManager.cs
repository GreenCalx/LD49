using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using Mirror;

public class OfflineGameManager : MonoBehaviour
{
    [Header("Mand Refs")]
    public GameObject UIWaitForPlayers;
    public OnlineStartLine startLine;
    [Header("Internals")]
    public bool isServerOnly = false;
    public bool sessionIsReadyToGo = false;
    public bool allPlayersHaveLoaded = false;

    public OnlinePlayerController localPlayer;

    public List<OnlinePlayerController> remotePlayers;


    // Start is called before the first frame update
    void Start()
    {
        refreshSession();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void refreshSession()
    {
        sessionIsReadyToGo = false;
        allPlayersHaveLoaded = false;
        StartCoroutine(WaitForOtherPlayers());
    }

    IEnumerator WaitForOtherPlayers()
    {
        UIWaitForPlayers.SetActive(true);

        while (NetworkRoomManagerExt.singleton.onlineGameManager==null)
        {
            yield return null;
        }

        while (NetworkRoomManagerExt.singleton.AnySceneOperationOngoing())
        {
            yield return null;
        }

        // No players => server only
        if (isServerOnly)
        {
            allPlayersHaveLoaded = NetworkRoomManagerExt.singleton.onlineGameManager.AllPlayersLoaded();
            while (!allPlayersHaveLoaded)
            {
                // Information given by OnlineGameManager
                yield return null;
            }
            UIWaitForPlayers.SetActive(false);
            sessionIsReadyToGo = true;
            yield break;
        }

        
        while(localPlayer==null)
        {
            yield return null;
        }
        while (startLine==null)
        {
            yield return null;
        }
        
        // if (localPlayer.isServer)
        // {
        //     //NetworkRoomManagerExt.singleton.onlineGameManager.NotifyPlayerHasLoaded(localPlayer, true);
        //     allPlayersHaveLoaded = NetworkRoomManagerExt.singleton.onlineGameManager.AllPlayersLoaded();
        //     while (!allPlayersHaveLoaded)
        //     {
        //         // Information given by OnlineGameManager
        //         yield return null;
        //     }
        // }
        if (localPlayer.isClient)
        {
            while(!localPlayer.readyToPlay)
            {
                yield return null;
            }

            if (localPlayer.isClientOnly)
                localPlayer.CmdInformPlayerHasLoaded();
            else
                localPlayer.InformPlayerHasLoaded();

            while (!allPlayersHaveLoaded) // modified via callback from OnlineGameMgr
            { yield return null; }
        }
            
        sessionIsReadyToGo = true;

        UIWaitForPlayers.SetActive(false);
    }

    private void flush()
    {
        localPlayer = null;
        remotePlayers = new List<OnlinePlayerController>();
    }

    public void AddToRemotePlayers(OnlinePlayerController iOPC)
    {
        if (!remotePlayers.Contains(iOPC))
            remotePlayers.Add(iOPC);
    }

}
