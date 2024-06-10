using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

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
        playerNuts.text = iPlayer.bag.nuts.ToString();
        playerAccels.text = iPlayer.bag.accels.ToString();
        playerMaxSpeeds.text = iPlayer.bag.maxSpeeds.ToString();
        playerSprings.text = iPlayer.bag.springs.ToString();
        playerTurns.text = iPlayer.bag.turns.ToString();
        playerTorqueForces.text = iPlayer.bag.torqueForces.ToString();
        playerWeight.text = iPlayer.bag.weights.ToString();
    }
}
