using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using TMPro;

using Schnibble;

public class UIOnlinePlayerInfoLine : MonoBehaviour
{
    public TextMeshProUGUI playerName;
    public TextMeshProUGUI playerNuts;
    public TextMeshProUGUI playerAccels;
    public TextMeshProUGUI playerMaxSpeeds;
    public TextMeshProUGUI playerSprings;
    public TextMeshProUGUI playerTurns;
    public TextMeshProUGUI playerTorqueForces;
    public TextMeshProUGUI playerWeight;

    public OnlinePlayerController player;

    public void Refresh(OnlinePlayerController iPlayer = null)
    {
        OnlinePlayerController playerData;
        if (iPlayer == null) {
            playerData = player;
        } else {
            playerData = iPlayer;
        }

        if (playerData == null) {
            this.LogError("player is null.");
            return;
        }

        playerName        .text = playerData.onlinePlayerName;

        playerNuts        .text = playerData.bag.nuts        .ToString();
        playerAccels      .text = playerData.bag.accels      .ToString();
        playerMaxSpeeds   .text = playerData.bag.maxSpeeds   .ToString();
        playerSprings     .text = playerData.bag.springs     .ToString();
        playerTurns       .text = playerData.bag.turns       .ToString();
        playerTorqueForces.text = playerData.bag.torqueForces.ToString();
        playerWeight      .text = playerData.bag.weights     .ToString();
    }
}
