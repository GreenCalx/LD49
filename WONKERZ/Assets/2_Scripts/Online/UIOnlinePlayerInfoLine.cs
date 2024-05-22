using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIOnlinePlayerInfoLine : MonoBehaviour
{

    public TextMeshProUGUI playerName;
    public TextMeshProUGUI playerNuts;

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

    }
}
