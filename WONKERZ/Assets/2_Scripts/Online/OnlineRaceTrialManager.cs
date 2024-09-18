using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mirror;

public class OnlineRaceTrialManager : OnlineTrialManager
{
    [Header("Race Trial Specs")]
    public int n_laps = 3;
    public Transform CheckpointsHandle;
    
    [Header("Race Trial Internals")]
    public OnlineRaceCheckPoint RootCP;
    public SyncDictionary<OnlinePlayerController, int> dicPlayersToLaps = new SyncDictionary<OnlinePlayerController, int>();
    public SyncDictionary<OnlinePlayerController, OnlineRaceCheckPoint> dicPlayersToCP = new SyncDictionary<OnlinePlayerController, OnlineRaceCheckPoint>();
    public LinkedList<OnlineRaceCheckPoint> checkpoints = new LinkedList<OnlineRaceCheckPoint>();

    void Start()
    {
        StartCoroutine(Init());
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
        
        // init dic players to laps
        dicPlayersToLaps = new SyncDictionary<OnlinePlayerController, int>();
        // init dic players to CP
        dicPlayersToCP = new SyncDictionary<OnlinePlayerController, OnlineRaceCheckPoint>();
        foreach (OnlinePlayerController opc in OnlineGameManager.singleton.uniquePlayers)
        {
            dicPlayersToLaps.Add(opc, 0);
            dicPlayersToCP.Add(opc, RootCP);
        }
        
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
                if (dicPlayersToLaps[iOPC] >= n_laps)
                {
                    NotifyPlayerHasFinished(iOPC);
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
