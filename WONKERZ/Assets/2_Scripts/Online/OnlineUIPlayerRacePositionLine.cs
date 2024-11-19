using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class OnlineUIPlayerRacePositionLine : MonoBehaviour
{
    public TextMeshProUGUI playerName;
    public TextMeshProUGUI playerPosition;

    public void SetPlayerName(string iPName)
    {
        playerName.text = iPName;
    }

    public void SetPlayerPosition(int iPos)
    {
        playerPosition.text = iPos.ToString();
    }
}
