using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrickCondition
{
    
    private readonly bool[] GROUNDED = new bool[4] { true, true, true, true};
    private readonly bool[] AIRED = new bool[4] { false, false, false, false};
    public float x_rot;
    public float y_rot;
    public float z_rot;
    public bool[] wheels;



    public TrickCondition(  bool[] iWheels )
    {
        x_rot = 0;
        y_rot = 0;
        z_rot = 0;
        wheels = iWheels;
    }

    
    public TrickCondition( float iXRot, float iYRot, float iZRot)
    {
        x_rot = iXRot;
        y_rot = iYRot;
        z_rot = iZRot;

        wheels = null;
    }

    public bool compare_wheels( TrickTracker iTT)
    {
        var iL1 = iTT.wheels_statuses;
        var iL2 = wheels;
        if ( iL1.Length != iL2.Length)
            return false;
        for( int i=0; i < iL1.Length; i++)
        {
            if ( iL1[i] != iL2[i] )
                return false;
        }
        return true;
    }

    public bool compare_air(TrickTracker iTT)
    {
        if (iTT.carIsOnGround())
            return false;

        if ( (x_rot == 0) && (y_rot == 0) && (z_rot == 0) )
            return true;

        // check expected rot sign
        bool validate_x = (x_rot == 0) ? true : (Mathf.Sign(x_rot) == Mathf.Sign(iTT.rec_rot_x));
        bool validate_y = (y_rot == 0) ? true : (Mathf.Sign(y_rot) == Mathf.Sign(iTT.rec_rot_y));
        bool validate_z = (z_rot == 0) ? true : (Mathf.Sign(z_rot) == Mathf.Sign(iTT.rec_rot_z));

        // check rot value
        validate_x &= (x_rot == 0) ? true : Mathf.Abs( (Mathf.Abs(iTT.rec_rot_x) - Mathf.Abs(x_rot)) ) <= iTT.rot_epsilon;
        validate_y &= (y_rot == 0) ? true : Mathf.Abs( (Mathf.Abs(iTT.rec_rot_y) - Mathf.Abs(y_rot)) ) <= iTT.rot_epsilon;
        validate_z &= (z_rot == 0) ? true : Mathf.Abs( (Mathf.Abs(iTT.rec_rot_z) - Mathf.Abs(z_rot)) ) <= iTT.rot_epsilon;

        if (validate_x && validate_y && validate_z)
        {
            return true;
        }

        return false;
    }

}
