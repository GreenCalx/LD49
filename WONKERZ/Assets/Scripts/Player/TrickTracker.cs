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
        Vector3 currentAngles = player_transform.eulerAngles;

        //rec_rot_x = rotDiff(player_transform.eulerAngles.x, init_rot_x) - cons_rot_x;
        //rec_rot_y = rotDiff(player_transform.eulerAngles.y, init_rot_y) - cons_rot_y;
        //rec_rot_z = rotDiff(player_transform.eulerAngles.z, init_rot_z) - cons_rot_z;
                                              
        //rec_rot_x.Add( rotDiff(player_transform.eulerAngles.x, init_rot_x) - cons_rot_x );
        //rec_rot_x.Add(currentAngles.x);
        //rec_rot_y.Add(currentAngles.y);
        //rec_rot_z.Add(currentAngles.z);
        
        // Reference :
        // http://www.euclideanspace.com/maths/geometry/rotations/conversions/quaternionToEuler/
        Quaternion q = player_transform.rotation.normalized;
        float x = q.x;
        float y = q.y;
        float z = q.z;
        float w = q.w;

        double gimbal_lock_tst = x*y + z*w;

        float yaw =0f, pitch=0f, roll=0f;
        if ( gimbal_lock_tst > 0.499 ) // north pole
        {
            yaw    = 0;
            pitch  = 2*Mathf.Atan2(x,w);
            roll   = Mathf.PI/2;
        }
        else if ( gimbal_lock_tst < -0.499) // south pole
        {
            yaw    = 0;
            pitch  = -2*Mathf.Atan2(x,w);
            roll   = -Mathf.PI/2;
        } else {
            yaw    = Mathf.Atan2(2*y*w - 2*x*z, 1 - 2*y*y - 2*z*z );
            pitch  = Mathf.Atan2(2*x*w - 2*y*z, 1-  2*x*x - 2*z*z );
            //pitch *= -2;  // pithc is between -90 and 90
            roll   = Mathf.Asin( 2*x*y + 2*z*w);
        }

        // rad to deg
        rec_rot_x.Add( pitch * 180/Mathf.PI );
        rec_rot_y.Add( yaw   * 180/Mathf.PI );
        rec_rot_z.Add( roll  * 180/Mathf.PI );

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
        // Available rotation computation
        // i=0..n => sum(recrot(i-1), recrot(i))  
        rotations = new Vector3(0f,0f,0f);
        
        for( int i=0; i < rec_rot_x.Count; i++ )
        {
            if (i==0)
            {
                //rots.x += rec_rot_x[i];
                continue;
            }
            rotations.x += rotDiff(rec_rot_x[i-1], rec_rot_x[i]);
        }

        for( int i=0; i < rec_rot_y.Count; i++ )
        {
            if (i==0)
            {
                //rots.y += rec_rot_y[i];
                continue;
            }
            rotations.y += rotDiff(rec_rot_y[i-1], rec_rot_y[i]);
        }

        for( int i=0; i < rec_rot_z.Count; i++ )
        {
            if (i==0)
            {
                //rots.z += rec_rot_z[i];
                continue;
            }
            rotations.z += rotDiff(rec_rot_z[i-1], rec_rot_z[i]);
        }
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
        if (tc.x_rot!=0)
        { rec_rot_x.Clear(); rec_rot_x.Add(player_transform.eulerAngles.x); }
        if (tc.y_rot!=0)
        { rec_rot_y.Clear(); rec_rot_x.Add(player_transform.eulerAngles.y); }
        if (tc.z_rot!=0)
        { rec_rot_z.Clear(); rec_rot_x.Add(player_transform.eulerAngles.z); }
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
