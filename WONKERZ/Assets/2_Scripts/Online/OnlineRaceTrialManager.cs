using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mirror;

public class OnlineRaceTrialManager : OnlineTrialManager
{
    [Header("ORTM Manual Refs")]
    public UIOnlineRaceTrial uiORT;

    [Header("Race Trial Specs")]
    public int n_laps = 3;
    public Transform CheckpointsHandle;
    
    [Header("Race Trial Internals")]
    public OnlineRaceCheckPoint RootCP;
    public readonly SyncDictionary<OnlinePlayerController, int> dicPlayersToLaps = new SyncDictionary<OnlinePlayerController, int>();
    public readonly SyncDictionary<OnlinePlayerController, OnlineRaceCheckPoint> dicPlayersToCP = new SyncDictionary<OnlinePlayerController, OnlineRaceCheckPoint>();
    //public SyncDictionary<OnlinePlayerController, float> dicPlayersToRacePosScore = new SyncDictionary<OnlinePlayerController, float>();
    public LinkedList<OnlineRaceCheckPoint> checkpoints = new LinkedList<OnlineRaceCheckPoint>();

    void Start()
    {
        StartCoroutine(Init());
    }

    void Update()
    {
        if (!trialIsOver && trialLaunched)
        {
            trialTime += Time.deltaTime;
            uiORT.RpcUpdateRaceTime(GetTrialTime());
            RefreshPlayersRacePosition();
        }
    }

    IEnumerator Init()
    {
        yield return StartCoroutine(GenericTrialInit());

        // init check points
        checkpoints = new LinkedList<OnlineRaceCheckPoint>(CheckpointsHandle.GetComponentsInChildren<OnlineRaceCheckPoint>());
        foreach (OnlineRaceCheckPoint cp in checkpoints)
        {
            if (cp.id == 0)
            { RootCP = cp; }
            cp.ORTM = this;
        }

        // init players
        // retrieved from OGM.uniquePlayers
        foreach (OnlinePlayerController opc in OnlineGameManager.singleton.uniquePlayers)
        {
            dicPlayersToLaps.Add(opc, 1);
            dicPlayersToCP.Add(opc, RootCP);
            //dicPlayersToRacePosScore.Add(opc,0f);

            if (opc.isClientOnly)
                uiORT.RpcUpdateLap(1);
            else
                uiORT.updateLap(1);
        }
    
        uiORT.RpcUpdateNLapsValue(n_laps);
        uiORT.RpcUpdateRaceTime(GetTrialTime());

        RefreshPlayersRacePosition();
    }

    [Server]
    public void RefreshPlayersRacePosition()
    {
        List<OnlinePlayerController> rankedPlayers = new List<OnlinePlayerController>();

        for (int i=0; i < OnlineGameManager.singleton.uniquePlayers.Count; i++)
        {
            OnlinePlayerController polledOPC = OnlineGameManager.singleton.uniquePlayers[i];
            if (i==0)
            {   
                rankedPlayers.Add(polledOPC);
                continue;
            }

            float polledOPCScore = GetScore(polledOPC);
            for (int j=0; j < rankedPlayers.Count; j++)
            { 
                if (GetScore(rankedPlayers[j]) < polledOPCScore)
                {
                    rankedPlayers.Insert(j, polledOPC);
                    break;
                }
            }
            if (!rankedPlayers.Contains(polledOPC))
            {
                rankedPlayers.Add(polledOPC); // is in last position
            }
        }

        foreach (OnlinePlayerController opc in rankedPlayers)
        {
            if (opc.isClientOnly)
            {
                uiORT.RpcLocalPlayerUpdatePosition(rankedPlayers.IndexOf(opc)+1);
            } else {
                uiORT.UpdateLocalPlayerPosition(rankedPlayers.IndexOf(opc)+1);
            }
        }
    }

    [Server]
    public float GetScore(OnlinePlayerController iOPC)
    {
        return dicPlayersToLaps[iOPC] + (0.1f * dicPlayersToCP[iOPC].id) + (0.01f * DistToNextCP(iOPC));
    }

    [Server]
    public float DistToNextCP(OnlinePlayerController iOPC)
    {
        Vector3 pPos = iOPC.self_PlayerController.GetTransform().transform.position;
        Vector3 nextCPPos = Vector3.zero;
        if (dicPlayersToCP[iOPC].IsPredecessorOf(RootCP) )
        {
            nextCPPos = RootCP.transform.position;
        } else {
            nextCPPos = dicPlayersToCP[iOPC].GetFirstSuccessor().transform.position;
        }
        return Vector3.Distance(pPos, nextCPPos);
    }

    [Server]
    public void NotifyPlayerPassedCP(OnlinePlayerController iOPC, OnlineRaceCheckPoint iCP)
    {
        // check normal scenario : triggered cp is a successor of last triggered CP
        if (dicPlayersToCP[iOPC].IsPredecessorOf(iCP))
        {
            // all good, update CP
            dicPlayersToCP[iOPC] = iCP;

            // check if a lap has been done
            if (iCP.id == 0)
            {
                // lap over !
                dicPlayersToLaps[iOPC]++;
            
                if (dicPlayersToLaps[iOPC] > n_laps)
                {
                    NotifyPlayerHasFinished(iOPC);
                } else {

                    if (iOPC.isClientOnly)
                    { uiORT.RpcUpdateLap(dicPlayersToLaps[iOPC]); }
                    else
                    { uiORT.updateLap(dicPlayersToLaps[iOPC]); }
                }
            }
            return;
        }

        // check if player is going backward
        else if (dicPlayersToCP[iOPC].id==iCP.id)
        {
            // TODO : Notify to the player that he is going backward
            return;
        }

        // a skip occured
        else
        {
            return;
        }
    }


}
