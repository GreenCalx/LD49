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

    // Happens only if everyone died but one player, can also be draw if everyone died.
    public void RefreshFromOC(OnlinePlayerController iPlayer)
    {
        playerName.text = iPlayer.onlinePlayerName;
        if (!iPlayer.IsAlive)
        {
            playerRank.text = "DNF";
            playerRaceTime.text = iPlayer.GetTimeOfDeath();
        }
        else {
            playerRank.text = "1";
            playerRaceTime.text = "--:--";
        }

    }

    public void RefreshFromTrial(OnlinePlayerController iPlayer)
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
        
        playerRaceTime.text = OTM.GetTrialTime();
    }
}
