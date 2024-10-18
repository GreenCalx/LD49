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
    [Header("Graphx")]
    public OnlineRenderingUpdater RenderingUpdater;

    [Header("Internals")]
    public readonly SyncDictionary<OnlinePlayerController, int> dicPlayerTrialFinishPositions = new SyncDictionary<OnlinePlayerController, int>();

    public readonly SyncDictionary<OnlinePlayerController, float> dicPlayerTrialToRaceTimes = new SyncDictionary<OnlinePlayerController, float>();
    [SyncVar]
    public bool trialIsOver = false;
    [SyncVar]
    public float trialTime = 0f;
    [SyncVar]
    public bool trialLaunched = false;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(GenericTrialInit());
    }

    IEnumerator WaitForDependencies()
    {
        while (OnlineGameManager.singleton             == null) {yield return null;}
    }

    protected IEnumerator GenericTrialInit()
    {
        yield return StartCoroutine(WaitForDependencies());
        OnlineGameManager.singleton.trialManager = this;
        trialLaunched = false;

        if (RenderingUpdater!=null)
        {
            RenderingUpdater.UpdateRenderSettings();
            if (isServer)
                RenderingUpdater.RpcUpdateRenderSettings();
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (!trialIsOver && trialLaunched)
        {
            trialTime += Time.deltaTime;
        }
    }

    // Note :
    // If still needed with new online scene workflow , move to OGM
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
        var localPC = OnlineGameManager.singleton.localPlayer;
        //OnlinePlayerController localPC = Access.OfflineGameManager().localPlayer;
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
        var uniquePlayers = NetworkRoomManagerExt.singleton.roomplayersToGameplayersDict.Values;
        return (dicPlayerTrialFinishPositions.Count == uniquePlayers.Count);
    }

    public string GetTrialTime()
    {
        int trackTime_val_min = (int)(trialTime / 60);
        if (trackTime_val_min<0)
        {
            trackTime_val_min = 0;
        }
        string trackTime_str_min = trackTime_val_min.ToString();
        if (trackTime_str_min.Length<=1)
        {
            trackTime_str_min = "0"+trackTime_str_min;
        }

        int trackTime_val_sec = (int)(trialTime % 60);
        if (trackTime_val_sec<0)
        {
            trackTime_val_min = 0;
        }
        string trackTime_str_sec = trackTime_val_sec.ToString();
        if (trackTime_str_sec.Length<=1)
        {
            trackTime_str_sec = "0"+trackTime_str_sec;
        }

        return trackTime_str_min + ":" + trackTime_str_sec;
    }
}
