using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class OnlineArenaTrialManager : OnlineTrialManager
{
    [Header("OATM Manual Refs")]
    public OnlineArenaTrialUI uiOAT;
    [Header("Arena Trial Internals")]
    public List<OnlinePlayerController> alivePlayers;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Init());
    }

    // Update is called once per frame
    void Update()
    {
        if (!trialIsOver && trialLaunched)
        {
            trialTime += Time.deltaTime;

            // refresh living players
            alivePlayers = alivePlayers.Where(p => p.IsAlive).ToList();

            if (alivePlayers.Count==1)
            {
                //end trial
                NotifyPlayerHasFinished(alivePlayers[0]);
            } else if (alivePlayers.Count==0)
            {
                // end trial in draw
                // TODO ?
            }
        }
    }

    

    IEnumerator Init()
    {
        yield return StartCoroutine(GenericTrialInit());

        var uniquePlayers = NetworkRoomManagerExt.singleton.roomplayersToGameplayersDict.Values;
        foreach (var opc in uniquePlayers)
        {
            alivePlayers.Add(opc);
            opc.bag.MaxOutNuts();
        }
    }
}
