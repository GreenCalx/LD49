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

    [Header("MANDATORY")]
    public bool activate_tricks;
    public TrickUI trickUI;
    public Transform player_transform;
    [Header("TWEAK PARAMS")]
    public float combo_multiplier = 1f;
    public float rot_epsilon = 2f;
    public float line_cooldown = 0.4f;
    public float hold_time_start_flat_trick = 0.4f;

    [Header("DEBUG")]
    public bool[] wheels_statuses;
    private bool line_started;
    private bool register_waiting_trick;
    private double time_trick_started;

    [HideInInspector]
    public List<TrickTimePair> trick_line;

    [HideInInspector]
    public int line_score;
    

    [HideInInspector]
    public float init_rot_x, init_rot_y, init_rot_z;
    [HideInInspector]
    public float rec_rot_x, rec_rot_y, rec_rot_z;
    [HideInInspector]
    public float  time_waited_after_line;
    [HideInInspector]
    public float recorded_time_flat_trick;

    private CarController CC;

    void Start()
    {
        CC = GetComponent<CarController>();
        if (!CC)
        {
            activate_tricks = false;
            return;
        }

        wheels_statuses = new bool[4];
        for (int i = 0 ; i < wheels_statuses.Length ; i ++)
            wheels_statuses[i] = true;

        if (trickUI == null)
        {
            Debug.LogWarning("TrickUI is missing.");
        }

        trick_line = new List<TrickTimePair>(0);
        line_started = false;
        time_trick_started = 0;

        init_rot_x = 0f;
        init_rot_y = 0f;
        init_rot_z = 0f;

        register_waiting_trick = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!activate_tricks)
            return;

        updateWheelStatuses();
            
        if (line_started || register_waiting_trick)
        {
            if (checkTricks())
                recordRotations();
        }
        updateUI();
    }
    public void recordRotations()
    {
        rec_rot_x = init_rot_x - player_transform.rotation.x * 360;
        rec_rot_y = init_rot_y - player_transform.rotation.y * 360;
        rec_rot_z = init_rot_z - player_transform.rotation.z * 360;
    }

    public void initRotationsRecord()
    {
        init_rot_x = player_transform.rotation.x * 360;
        init_rot_y = player_transform.rotation.y * 360;
        init_rot_z = player_transform.rotation.z * 360;

        rec_rot_x = 0f;
        rec_rot_y = 0f;
        rec_rot_z = 0f;
    }

    public bool checkTricks()
    {
        register_waiting_trick = false;

        Trick t = TrickDictionary.checkTricks(this);
        if (t!=null)
        {
            if (t.condition.trick_nature == TrickCondition.TRICK_NATURE.FLAT)
            {
                if ( line_started == false )
                {
                    recorded_time_flat_trick = Time.time;
                    line_started = true;
                    return false;
                }
                if ( (Time.time - recorded_time_flat_trick) <= hold_time_start_flat_trick )
                    return false;
            }

            if (trick_line.Count == 0 )
                start_line(t);
            else
                try_add_to_line(t);
            return true;
        } else {
            if ( trick_line.Count > 0 )
            {
                end_line();
            }
        }

        //updateUI();
        return false;
    }

    private void start_line( Trick t)
    {

        trick_line.Clear();
        trick_line.Add( new TrickTimePair(t,0) );

        initRotationsRecord();

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

        time_waited_after_line = Time.time;
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

    public void updateWheelStatuses()
    {
        // update wheels
        CarController.Axle front  = CC.FrontAxle;
        CarController.Axle rear   = CC.RearAxle;

        CarController.Wheel front_left  = front.Left.Wheel;
        CarController.Wheel front_right = front.Right.Wheel;
        CarController.Wheel rear_left   = rear.Left.Wheel;
        CarController.Wheel rear_right  = rear.Right.Wheel;

        wheels_statuses[(int)WHEEL_LOCATION.FRONT_LEFT]  = front_left.IsGrounded;
        wheels_statuses[(int)WHEEL_LOCATION.FRONT_RIGHT] = front_right.IsGrounded;
        wheels_statuses[(int)WHEEL_LOCATION.BACK_LEFT]   = rear_left.IsGrounded;
        wheels_statuses[(int)WHEEL_LOCATION.BACK_RIGHT]  = rear_right.IsGrounded;

        // Look for trickline cooldown
        if ( (Time.time - time_waited_after_line) <= line_cooldown )
        {
            register_waiting_trick = true;
            return;
        }

        // Look for tricks if CD is OK
        checkTricks();
    }
}
