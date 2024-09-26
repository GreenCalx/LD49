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

    [Header("Manual self refs")]
    public TextMeshProUGUI raceTimeValue;
    public TextMeshProUGUI nLapsValue;
    public TextMeshProUGUI clientLapsValue;
    public TextMeshProUGUI positionTextNumber;
    public TextMeshProUGUI positionTextSuffix;
    public ParticleSystem LapPassedPS;

    [Header("Internals")]
    public bool initDone = false;

    void Start()
    {
        StartCoroutine(initCo());
    }

    public IEnumerator initCo()
    {
        initDone =false;

        while (OnlineGameManager.singleton==null)
        { yield return null; }

        while (OnlineGameManager.singleton.localPlayer==null)
        { yield return null; }

        initDone = true;
    }

     [TargetRpc]
     public void RpcLocalPlayerUpdatePosition(int iPosition)
     {
        UpdateLocalPlayerPosition(iPosition);
     }

    public void UpdateLocalPlayerPosition(int iPosition)
    {
        positionTextNumber.text = iPosition.ToString();
        if (iPosition==1)
            positionTextSuffix.text = "st";
        else if (iPosition==2)
            positionTextSuffix.text = "nd";
        else if (iPosition==3)
            positionTextSuffix.text = "rd";
        else
            positionTextSuffix.text = "th";
    }
    // [ClientRpc]
    // public void RpcRefreshPositions(Dictionary<OnlinePlayerController, float> iOrderedPlayers)
    // {
    //     RefreshPositions(iOrderedPlayers);
    // }

    // public void RefreshPositions(Dictionary<OnlinePlayerController, float> iOrderedPlayers)
    // {
    //     if (!initDone)
    //         return;
    //     // dic should be ordered by score before entering this
    //     int index = 0;
    //     foreach(OnlinePlayerController opc in iOrderedPlayers.Keys)
    //     {
    //         if (index>=playerPositionLineInsts.Count)
    //             continue;
    //         OnlineUIPlayerRacePositionLine infoLine = playerPositionLineInsts[index];
    //         infoLine.SetPlayerName(opc.onlinePlayerName);
    //         infoLine.SetPlayerPosition(index+1);
    //         index++;
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

    public void PlayLapAnim()
    {
        LapPassedPS.Play();
    }

    [TargetRpc]
    public void RpcPlayLapAnim()
    {
        PlayLapAnim();
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
