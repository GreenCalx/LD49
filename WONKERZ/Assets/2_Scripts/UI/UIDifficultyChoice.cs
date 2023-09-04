using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIDifficultyChoice : UIPanelTabbed
{
    [HideInInspector]
    public bool choice_made = false;

    [HideInInspector]
    public DIFFICULTIES chosen_difficulty = DIFFICULTIES.EASY;

    [Header("MAND")]
    public GameObject childUIToActivate;

    public TextMeshProUGUI trackNameTxt;
    public TextMeshProUGUI panelHintTxt;
    public TextMeshProUGUI cpHintTxt;

    // Start is called before the first frame update
    void Start()
    {
        string target_scene = Access.SceneLoader().targetScene;
        if (Array.Exists(Constants.SN_TRACKS, e => e == target_scene))
        {
            // activate menu
            trackNameTxt.text = target_scene;
            childUIToActivate.SetActive(true);
            Access.SceneLoader().lockScene();
            onActivate.Invoke();
        } else {
            Destroy(gameObject);
        }
        updateUIFromDiff();
    }

    // Update is called once per frame
    void Update()
    {
        if (choice_made)
        {
            Access.TrackManager().track_score.selected_diff = chosen_difficulty;
            Access.SceneLoader().unlockScene();
            onDeactivate.Invoke();
            Destroy(gameObject);
        }

        updateUIFromDiff();
    }

    private void updateUIFromDiff()
    {
        switch (chosen_difficulty)
        {
            case DIFFICULTIES.EASY:
                string str = (Constants.EASY_N_PANELS < 99) ? Constants.EASY_N_PANELS.ToString() : "inf";
                panelHintTxt.text = str;
                cpHintTxt.text = "YES";
                break;
            case DIFFICULTIES.MEDIUM:
                panelHintTxt.text = Constants.MEDIUM_N_PANELS.ToString();
                cpHintTxt.text = "YES";
                break;
            case DIFFICULTIES.HARD:
                panelHintTxt.text = Constants.HARD_N_PANELS.ToString();
                cpHintTxt.text = "YES";
                break;
            case DIFFICULTIES.IRONMAN:
                panelHintTxt.text = Constants.IRONMAN_N_PANELS.ToString();
                cpHintTxt.text = "NO";
                break;
            default:
                break;
        }
    }

    override public void activate()
    {
        base.activate();
    }
}