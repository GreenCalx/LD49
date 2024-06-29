using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Wonkerz;
using Mirror;

public class OnlineTrialManager : NetworkBehaviour
{
    [Header("Mands")]
    //public List<OnlineStartPortal> startPositions;
    public OnlineStartLine onlineStartLine;
    public OnlineStartDispatcher portals;

    [Header("Internals")]
    public SyncDictionary<OnlinePlayerController, int> dicPlayerTrialFinishPositions = new SyncDictionary<OnlinePlayerController, int>();

    public SyncDictionary<OnlinePlayerController, float> dicPlayerTrialToRaceTimes = new SyncDictionary<OnlinePlayerController, float>();
    [SyncVar]
    public bool trialIsOver = false;
    [SyncVar]
    public float trialTime = 0f;
    [SyncVar]
    public bool trialLaunched = false;

    // Start is called before the first frame update
    void Start()
    {
        NetworkRoomManagerExt.singleton.onlineGameManager.trialManager = this;
        trialLaunched = false;

        if (isServer)
            DispatchPlayersToPortals();
    }

    // Update is called once per frame
    void Update()
    {
        if (!trialIsOver && trialLaunched)
        {
            trialTime += Time.deltaTime;
        }
    }


    [Server]
    protected void DispatchPlayersToPortals()
    {
        if (portals==null)
        { portals = GetComponent<OnlineStartDispatcher>(); }

        if (portals!=null)
        { portals.DispatchPlayersToPortals(); }

        else
        { Debug.LogError("No Portal Dispatcher for OnlineTrialManager."); }
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
        dicPlayerTrialToRaceTimes.Add(iOPC, trialTime);

        trialIsOver = AllPlayersFinished();
    }

    [ServerCallback]
    public void NotifyPlayerIsDNF(OnlinePlayerController iOPC)
    {
        if (dicPlayerTrialFinishPositions.ContainsKey(iOPC))
            return;

        dicPlayerTrialFinishPositions.Add(iOPC, -1);
        dicPlayerTrialToRaceTimes.Add(iOPC, trialTime);

        trialIsOver = AllPlayersFinished();
    }

    [ServerCallback]
    public bool AllPlayersFinished()
    {
        return (dicPlayerTrialFinishPositions.Count == NetworkRoomManagerExt.singleton.onlineGameManager.uniquePlayers.Count);
    }
}
