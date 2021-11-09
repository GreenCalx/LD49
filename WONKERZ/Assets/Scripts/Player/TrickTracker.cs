using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// should be in namespace or sm shit but access to tricks not working for some reason ?
public enum WHEEL_LOCATION 
{
        FRONT_RIGHT = 0,
        FRONT_LEFT = 1,
        BACK_RIGHT = 2,
        BACK_LEFT = 3
}

public class TrickTracker : MonoBehaviour
{

    public class TrickTimePair {
        public Trick trick;
        public double time;
        public TrickTimePair( Trick iTrick, double iTime)
        {
            trick   = iTrick;
            time    = iTime;
        }

        public int computeScore()
        {
            return (int)(trick.value * (1+time));
        }
    }

    public TrickUI trickUI;
    [HideInInspector]
    public List<WheelTrickTracker> wheels;
    [HideInInspector]
    public bool[] wheels_statuses;
    private bool check_tricks, line_started;
    private double time_trick_started;

    public List<TrickTimePair> trick_line;
    public float combo_multiplier = 1f;
    public int line_score;


    void Start()
    {
        WheelTrickTracker[] arr_wheels = GetComponentsInChildren<WheelTrickTracker>();
        wheels = new List<WheelTrickTracker>();
        wheels.AddRange(arr_wheels);
        if (wheels.Count!=4)
        {
            Debug.LogWarning( wheels.Count + " Wheels in trick tracker. Should have 4.");
        }
        foreach( WheelTrickTracker w in wheels)
        {
            w.tracker = this;
        }

        wheels_statuses = new bool[wheels.Count];
        check_tricks = false;

        if (trickUI == null)
        {
            Debug.LogWarning("TrickUI is missing.");
        }

        trick_line = new List<TrickTimePair>(0);
        line_started = false;
        time_trick_started = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (check_tricks || line_started)
            checkTricks();
        updateUI();
    }

    public void checkTricks()
    {
        //Debug.Log("CHECK TRICKS");
        Trick t = TrickDictionary.checkTricks(this);
        if (t!=null)
        {
            if (trick_line.Count == 0 )
                start_line(t);
            else
                try_add_to_line(t);
        } else {
            if ( trick_line.Count > 0 )
            {
                end_line();
            }
        }

        //updateUI();
        check_tricks = false;
    }

    private void start_line( Trick t)
    {
        Debug.Log("START LINE");

        trick_line.Clear();
        trick_line.Add( new TrickTimePair(t,0) );
        line_started = true;
        time_trick_started = Time.time;
    }
    private void try_add_to_line( Trick t )
    {
        TrickTimePair trickpair = trick_line[trick_line.Count-1];
        Trick last_trick = trickpair.trick;

        if ( last_trick.name.Equals(t.name) )
        {
            trick_line[trick_line.Count-1].time = Time.time - time_trick_started;
            return; // same trick

        } else { // different trick
            trick_line.Add( new TrickTimePair(t,0) );
            time_trick_started = Time.time;
        }
    }

    private void end_line()
    {
        Debug.Log("END LINE : " + trick_line.Count);

        line_score = 0;
        if ( trick_line.Count <= 0 )
        { return; }

        List<Trick> tricks = new List<Trick>();
        for ( int i=0; i < trick_line.Count; i++ )
        {
            TrickTimePair trickpair = trick_line[i];

            line_score += (int) (trickpair.computeScore() * (i+combo_multiplier));
            tricks.Add(trickpair.trick);
        }

        trickUI.displayTricklineScore(line_score);
        trickUI.displayTricklineTricks(tricks);
        trick_line.Clear();
        line_started = false;
    }

    public void updateUI()
    {
        if (trick_line.Count <= 0)
        {
            trickUI.displayTrick("");
            trickUI.displayScore(0);
            return;
        }

        TrickTimePair last_trick = trick_line[trick_line.Count-1];
        trickUI.displayTrick(last_trick.trick.name);
        trickUI.displayScore( last_trick.computeScore() );
    }

    public void notify(WheelTrickTracker wtt)
    {
        int wheel_location = (int) wtt.wheel_location;
        wheels_statuses[wheel_location] = wtt.is_grounded;
        check_tricks = true;
    }
}
