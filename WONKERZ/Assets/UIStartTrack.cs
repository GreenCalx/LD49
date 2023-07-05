using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIStartTrack : MonoBehaviour
{
    public TMPro.TextMeshProUGUI countdown_txt;

    // Update is called once per frame
    public void updateDisplay(float iCurrCountdownValue)
    {

        if (iCurrCountdownValue < 1f)      {countdown_txt.text = "3";}
        if ((iCurrCountdownValue >= 1f))   {countdown_txt.text = "2";}
        if ((iCurrCountdownValue >= 2f))   {countdown_txt.text = "1";}
        if ((iCurrCountdownValue >= 3f))   {countdown_txt.text = "GO!";}
    }


}
