using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using Mirror;

public class OnlineTrialManager : NetworkBehaviour
{
    [Header("Mands")]
    public List<OnlineStartPortal> startPositions;
    public OnlineStartLine onlineStartLine;
    [Header("Internals")]
    public SyncDictionary<OnlinePlayerController, int> dicPlayerTrialFinishPositions = new SyncDictionary<OnlinePlayerController, int>();
    [SyncVar]
    public bool trialIsOver = false;

    // Start is called before the first frame update
    void Start()
    {
        NetworkRoomManagerExt.singleton.onlineGameManager.trialManager = this;
        
        if (isServer)
            DispatchPlayersToPortals();
    
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [Server]
    protected void DispatchPlayersToPortals()
    {
        foreach( OnlinePlayerController opc in NetworkRoomManagerExt.singleton.onlineGameManager.uniquePlayers)
        {
            foreach(OnlineStartPortal sp in startPositions)
            {
                if (sp.attachedPlayer==null)
                {
                    sp.attachedPlayer = opc;
                    break;
                }
            }
        }
    }

    public void NotifyLocalPlayerFinished()
    {
        OnlinePlayerController localPC = Access.OfflineGameManager().localPlayer;
        if (localPC.isClientOnly)
        {
            localPC.CmdNotifyPlayerFinishedTrial();
        } else { // isServer
            NotifyPlayerHasFinished(localPC);
        }
        localPC.self_PlayerController.Freeze();
    }
    
    [Command]
    public void CmdNotifyPlayerHasFinished(OnlinePlayerController iOPC)
    {
        NotifyPlayerHasFinished(iOPC);
        
    }
    
    [ServerCallback]
    public void NotifyPlayerHasFinished(OnlinePlayerController iOPC)
    {
        if (dicPlayerTrialFinishPositions.ContainsKey(iOPC))
            return;

        dicPlayerTrialFinishPositions.Add(iOPC, dicPlayerTrialFinishPositions.Count+1);

        trialIsOver = AllPlayersFinished();
    }

    [ServerCallback]
    public bool AllPlayersFinished()
    {
        return (dicPlayerTrialFinishPositions.Count == NetworkRoomManagerExt.singleton.onlineGameManager.uniquePlayers.Count);
    }
}
