using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIOnlinePostGameLine : MonoBehaviour
{
    public int rank;
    public TextMeshProUGUI playerName;
    public TextMeshProUGUI playerRank;
    public TextMeshProUGUI playerRaceTime;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Refresh(OnlinePlayerController iPlayer)
    {
        playerName.text = iPlayer.onlinePlayerName;
        // HACK: make this better asap.
        if (NetworkRoomManagerExt.singleton == null) return;
        if (NetworkRoomManagerExt.singleton.onlineGameManager == null) return;
        if (NetworkRoomManagerExt.singleton.onlineGameManager.trialManager == null) return;

        OnlineTrialManager OTM = NetworkRoomManagerExt.singleton.onlineGameManager.trialManager;

        // Look for DNF
        if (!OTM.dicPlayerTrialFinishPositions.ContainsKey(iPlayer))
        {
            playerRank.text = "DNF";
            playerRaceTime.text = "--:--";
            return;
        }

        // explicitly DNF
        if (OTM.dicPlayerTrialFinishPositions[iPlayer] < 0)
        {
            playerRank.text = "DNF";
            playerRaceTime.text = "--:--";
            return;
        }

        // Rank finishing players
        rank = OTM.dicPlayerTrialFinishPositions[iPlayer];
        playerRank.text = OTM.dicPlayerTrialFinishPositions[iPlayer].ToString();
        

        float trackTime = OTM.trialTime;
        int trackTime_val_min = (int)(trackTime / 60);
        if (trackTime_val_min<0)
        {
            trackTime_val_min = 0;
        }
        string trackTime_str_min = trackTime_val_min.ToString();
        if (trackTime_str_min.Length<=1)
        {
            trackTime_str_min = "0"+trackTime_str_min;
        }

        int trackTime_val_sec = (int)(trackTime % 60);
        if (trackTime_val_sec<0)
        {
            trackTime_val_min = 0;
        }
        string trackTime_str_sec = trackTime_val_sec.ToString();
        if (trackTime_str_sec.Length<=1)
        {
            trackTime_str_sec = "0"+trackTime_str_sec;
        }

        playerRaceTime.text = trackTime_str_min +":"+ trackTime_str_sec;
    }
}
