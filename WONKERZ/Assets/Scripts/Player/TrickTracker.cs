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

    [Header("MANDATORY")]
    public bool activate_tricks;
    public TrickUI trickUI;
    public Transform player_transform;
    [Header("TWEAK PARAMS")]
    public float combo_multiplier = 1f;
    public float rot_epsilon = 2f;
    public float line_cooldown = 0.4f;
    public float hold_time_start_flat_trick = 0.4f;
    public int MIN_SCORE_FOR_DISPLAY = 10;

    [Header("DEBUG")]
    public bool[] wheels_statuses;
    private double time_trick_started;


    [HideInInspector]
    public TrickLine trick_line;
    

    //[HideInInspector]
    public float init_rot_x, init_rot_y, init_rot_z;
    //[HideInInspector]
    private List<float> rec_rot_x, rec_rot_y, rec_rot_z;

    public Vector3 rotations;
    
    [HideInInspector]
    public float  time_waited_after_line;
    [HideInInspector]
    public float recorded_time_trick;
    private KeyValuePair<Trick,float> flat_trick_starter;

    private CarController CC;
    public bool ready_to_rec_line;

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

        trick_line = new TrickLine();
        time_trick_started = 0;
        flat_trick_starter = new KeyValuePair<Trick, float>();

        init_rot_x = 0f;
        init_rot_y = 0f;
        init_rot_z = 0f;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!activate_tricks || (trick_line==null))
            return;

        updateWheelStatuses();

        // Look for trickline cooldown
        ready_to_rec_line = ((Time.time - time_waited_after_line) > line_cooldown) /*&& carIsOnGround()*/;

        if (ready_to_rec_line || trick_line.is_opened)
        {
            if (!trick_line.is_opened)
            {
                if (tryOpenLine())
                {
                    trickUI.recordingTrick();
                    initRotationsRecord();
                    recordRotations();
                    time_trick_started = Time.time;
                }
            } else {
                recordRotations();
                if (tryContinueLine())
                { /*continue line..*/ }
                else
                    end_line();
            }
        }
        updateUI();
    }

    public void recordRotations()
    {
        Vector3 curr_PYR = MathUtils.getPYR(player_transform.rotation);

        // rad to deg simplified as integers
        int pitch_deg   = (int)( curr_PYR.x * 180/Mathf.PI ) ;
        int yaw_deg     = (int)( curr_PYR.y * 180/Mathf.PI );
        int roll_deg    = (int)( curr_PYR.z * 180/Mathf.PI );

        // add to rec values
        if (!rec_rot_x.Contains(pitch_deg))
            rec_rot_x.Add( pitch_deg );
        if (!rec_rot_y.Contains(yaw_deg))
            rec_rot_y.Add( yaw_deg );
        if (!rec_rot_z.Contains(roll_deg))
            rec_rot_z.Add( roll_deg );

        updateRotations();
    }

    private float rotDiff(float iInit, float iCurrent)
    {
        //float ret = iCurrent - iInit;
        //return (ret+180) % 360 - 180;
        return Mathf.DeltaAngle( iInit, iCurrent);
    }

    public void updateRotations()
    {
        int xcount = rec_rot_x.Count;
        if (xcount > 0)
            rotations.x = rotDiff( rec_rot_x[0], rec_rot_x[xcount-1]);
        int ycount = rec_rot_y.Count;
        if (ycount > 0)
            rotations.y = rotDiff( rec_rot_y[0], rec_rot_y[ycount-1]);
        int zcount = rec_rot_z.Count;
        if (zcount > 0)
            rotations.z = rotDiff( rec_rot_z[0], rec_rot_z[zcount-1]);
        
    }

    public void initRotationsRecord()
    {
        init_rot_x = player_transform.eulerAngles.x;
        init_rot_y = player_transform.eulerAngles.y;
        init_rot_z = player_transform.eulerAngles.z;

        rec_rot_x = new List<float>();
        rec_rot_y = new List<float>();
        rec_rot_z = new List<float>();
    }

    // consume rotation
    public void updateConsumedRotations( TrickCondition tc )
    {
        //Vector3 curr_PYR = MathUtils.getPYR(player_transform.rotation);
        if (tc.x_rot!=0)
        { rec_rot_x.Clear(); }
        if (tc.y_rot!=0)
        { rec_rot_y.Clear(); }
        if (tc.z_rot!=0)
        { rec_rot_z.Clear(); }
        //updateRotations();
    }

    public bool tryOpenLine()
    {
        Trick opener1 = TrickDictionary.checkTricksIndexed( this, Trick.TRICK_NATURE.NEUTRAL);
        Trick opener2 = TrickDictionary.checkTricksIndexed( this, Trick.TRICK_NATURE.FLAT);

        if (opener1!=null)
        {
            trick_line.open(opener1);
            return true;
        } else if (opener2!=null) {
            
            if ( flat_trick_starter.Equals(default(KeyValuePair<Trick,float>)) )
            { flat_trick_starter = new KeyValuePair<Trick, float>(opener2, 0f); }

            Trick t = flat_trick_starter.Key;
            if ( t.name == opener2.name )
            {
                flat_trick_starter = new KeyValuePair<Trick, float>( opener2, flat_trick_starter.Value + Time.deltaTime );
            } else {
                flat_trick_starter = new KeyValuePair<Trick, float>();
            }
            
            if (flat_trick_starter.Value > hold_time_start_flat_trick)
            { 
                trick_line.open(opener2);
                return true;
            }
        }
        return false;
    }

    public void OnCollisionEnter(Collision iCol)
    {
        if (!carIsOnGround())
            end_line();
    }

    public bool tryContinueLine()
    {
        // NEW TRICK / continuing trick
        Trick tbasic    = TrickDictionary.checkTricksIndexed(this, Trick.TRICK_NATURE.BASIC);
        Trick tflat     = TrickDictionary.checkTricksIndexed(this, Trick.TRICK_NATURE.FLAT);
        Trick tneutral  = TrickDictionary.checkTricksIndexed(this, Trick.TRICK_NATURE.NEUTRAL);
        Trick tignore   = TrickDictionary.checkTricksIndexed(this, Trick.TRICK_NATURE.IGNORE);

        double trick_duration = Time.time - time_trick_started;
        if (tbasic!=null)
        {
            trick_line.add(tbasic, trick_duration);
            updateConsumedRotations( tbasic.condition );
            return true;
        } else if (tflat!=null){
            if (trick_duration > hold_time_start_flat_trick)
            {
                trick_line.add(tflat, trick_duration);
                return true;
            }
        } else if (tneutral!=null){
            trick_line.add(tneutral, trick_duration);
            return true;
        } else if (tignore!=null)
        {
            return true;
        }

        return false;
    }

    private void end_line()
    {
        if (carIsOnGround())
        {
            trickUI.displayTricklineScore(trick_line.getLineScore(combo_multiplier));
            trickUI.displayTricklineTricks(trick_line.getTrickList());
            trickUI.validateTrick();
        }
        else
        {
            trickUI.failTrick();
            trickUI.displayTricklineScore(0);
            trickUI.displayTricklineTricks(new List<Trick>(0));
        }

        trick_line.close();

        rec_rot_x.Clear();
        rec_rot_y.Clear();
        rec_rot_z.Clear();

        time_waited_after_line = Time.time;
    }

    public async void updateUI()
    {
        if (trickUI == null)
        { return; }

        if (!trick_line.is_opened && ready_to_rec_line )
        {
            trickUI.displayTrick("");
            trickUI.displayScore(0);
            return;
        }

        if (trick_line.getLineScore(combo_multiplier) < MIN_SCORE_FOR_DISPLAY )
        {
            return;
        }

        string tricks = "";
        using var enumerator = trick_line.full_line.GetEnumerator();
        var last = !enumerator.MoveNext();
        TrickLine.TrickTimePair ttp;

        while( !last )
        {
            ttp = enumerator.Current;        
            tricks += ttp.trick.name;
            last = !enumerator.MoveNext();

            if (!last)
                tricks += " + ";
        }

        trickUI.displayTrick(tricks);
        trickUI.displayScore( trick_line.getLineScore(combo_multiplier) );

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
    }

    public bool carIsOnGround()
    {
        foreach (bool w in wheels_statuses)
        {
            if (w)
            { return true;}
        }
        return false;
    }
}
