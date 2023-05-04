using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class UIDifficultyChoice : UIPanelTabbed
{
    [HideInInspector]
    public bool choice_made = false;

    [HideInInspector]
    public DIFFICULTIES chosen_difficulty = DIFFICULTIES.EASY;

    [Header("MAND")]
    public GameObject childUIToActivate;

    // Start is called before the first frame update
    void Start()
    {
        string target_scene = Access.SceneLoader().targetScene;
        if (Array.Exists(Constants.SN_TRACKS, e => e == target_scene))
        {
            // activate menu
            childUIToActivate.SetActive(true);
            Access.SceneLoader().lockScene();
            onActivate.Invoke();
        } else {
            Destroy(gameObject);
        }
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
    }

    override public void activate()
    {
        base.activate();
    }
}
