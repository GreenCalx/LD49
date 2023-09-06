using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UITrackEvent : MonoBehaviour
{
    [Header("MAND")]
    public TextMeshProUGUI eventTitle;
    public GameObject   objectivesHandle;
    // TODO : Make it a list etc.. if needed in the future
    public UITrackEventObjectiveLine objectiveLine;
    
    void Start()
    {
    }

    // called by observed objects
    public void refreshStatus(string iHintStatus)
    {
         objectiveLine.hintStatus.text    = iHintStatus;
    }

    public void show(string iTrackEventTitle, string iHintText, string iHintStatus)
    {
        eventTitle.text                 = iTrackEventTitle;
        objectiveLine.hintText.text      = iHintText;
        objectiveLine.hintStatus.text    = iHintStatus;
        
        gameObject.SetActive(true);
        objectivesHandle.SetActive(true);
        objectiveLine.gameObject.SetActive(true);
    }

    public void hide()
    {
        objectiveLine.gameObject.SetActive(false);
        objectivesHandle.SetActive(false);
        gameObject.SetActive(false);
    }

}
