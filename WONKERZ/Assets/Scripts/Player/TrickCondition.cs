using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrickCondition
{
    public enum TRICK_NATURE { FLAT=0, AIR=1 };
    private readonly bool[] GROUNDED = new bool[4] { true, true, true, true};
    private readonly bool[] AIRED = new bool[4] { false, false, false, false};
    public float x_rot;
    public float y_rot;
    public float z_rot;
    public bool[] wheels;
    public TRICK_NATURE trick_nature;

    public TrickCondition(  bool[] iWheels, TRICK_NATURE iNature= TRICK_NATURE.FLAT )
    {
        x_rot = 0;
        y_rot = 0;
        z_rot = 0;
        wheels = iWheels;
        trick_nature = iNature;
    }

    
    public TrickCondition( float iXRot, float iYRot, float iZRot, TRICK_NATURE iNature= TRICK_NATURE.AIR )
    {
        x_rot = iXRot;
        y_rot = iYRot;
        z_rot = iZRot;
        trick_nature = iNature;
        if (trick_nature == TRICK_NATURE.AIR)
            wheels = AIRED;
        else
            wheels = GROUNDED;
    }

    public bool check(TrickTracker iTT)
    {
        if ( trick_nature == TRICK_NATURE.FLAT )
        {
            return compare_wheels( iTT.wheels_statuses, wheels);
        } 
        else if ( trick_nature == TRICK_NATURE.AIR )
        {
            if ( compare_wheels(iTT.wheels_statuses, wheels) )
                return compare_air(iTT);
        }

        return false;
    }

    public bool compare_wheels( bool[] iL1, bool[] iL2)
    {
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
