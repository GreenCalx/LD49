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

        while(localPlayer==null)
        {
            yield return null;
        }
        while (startLine==null)
        {
            yield return null;
        }
        //startLine.init(localPlayer.self_PlayerController);
        if (localPlayer.isServer)
        {
            NetworkRoomManagerExt.singleton.onlineGameManager.NotifyPlayerHasLoaded(localPlayer, true);
            allPlayersHaveLoaded = NetworkRoomManagerExt.singleton.onlineGameManager.AllPlayersLoaded();
            while (!allPlayersHaveLoaded)
            {
                // Information given by OnlineGameManager
                yield return null;
            }
        }
        else if (localPlayer.isClientOnly)
        {
            localPlayer.CmdInformPlayerHasLoaded();

            while (!allPlayersHaveLoaded) // modified via callback from OnlineGameMgr
            {
                // Information given by OnlineGameManager
                yield return null;
            }
        }
            
        sessionIsReadyToGo = true;

        
        UIWaitForPlayers.SetActive(false);
        //localPlayer.self_PlayerController.UnFreeze();
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
