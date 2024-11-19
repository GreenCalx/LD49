using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
            if (alivePlayers.Count<=1)
            {
                //end trial
            }
        }
    }

    public void OnPlayerKill(OnlinePlayerController iOPC)
    {
        if (alivePlayers.Contains(iOPC))
        {
            alivePlayers.Remove(iOPC);
            NotifyPlayerIsDNF(iOPC);
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
