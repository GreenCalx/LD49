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
        StartCoroutine(WaitForOtherPlayers());
    }

    IEnumerator WaitForOtherPlayers()
    {
        UIWaitForPlayers.SetActive(true);

        while (NetworkRoomManagerExt.singleton.onlineGameManager==null)
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
        startLine.init(localPlayer.self_PlayerController);
        sessionIsReadyToGo = true;

        while (!NetworkRoomManagerExt.singleton.onlineGameManager.allPlayersLoadedInLobby)
        {
            // Information given by OnlineGameManager
            yield return null;
        }   
        
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
