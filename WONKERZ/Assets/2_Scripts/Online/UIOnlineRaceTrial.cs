using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using TMPro;
using Wonkerz;
using Mirror;

// Note:
// It needs to be a network behaviour to retrieve 
// player race position and the laps values from ortm
// which is server only
public class UIOnlineRaceTrial : NetworkBehaviour
{
    [Header("Manual refs")]
    public TextMeshProUGUI raceTimeValue;
    public TextMeshProUGUI nLapsValue;
    public TextMeshProUGUI clientLapsValue;

    // [Header("Auto Refs")]
    // public OnlineRaceTrialManager ORTM;

    // void Update()
    // {
    //     if (!!ORTM)
    //     {
    //         if (ORTM.dicPlayerTrialFinishPositions.ContainsKey(OnlineGameManager.Get().localPlayer))
    //             return;
    //         updateRaceTime();
    //     }
    // }
    void updateRaceTime(string iRaceTimeAsStr)
    {
        raceTimeValue.text = iRaceTimeAsStr;
    }
    [ClientRpc]
    public void RpcUpdateRaceTime(string iRaceTimeAsStr)
    {
        updateRaceTime(iRaceTimeAsStr);
    }

    public void updateLap(int iCurrLap)
    {
        clientLapsValue.text = iCurrLap.ToString();
    }

    [TargetRpc]
    public void RpcUpdateLap(int iCurrentLap)
    {
        updateLap(iCurrentLap);
    }

    public void updateNLapsValue(int iNLaps)
    {
        nLapsValue.text = iNLaps.ToString();
    }

    [ClientRpc]
    public void RpcUpdateNLapsValue(int iNLaps)
    {
        updateNLapsValue(iNLaps);
    }
}
