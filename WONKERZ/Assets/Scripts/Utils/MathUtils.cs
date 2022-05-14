using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct MathUtils
{
    // Get Pitch Yaw Roll as Vec3(Pitch, Yaw, Roll)
    public static Vector3 getPYR( Quaternion iRotation )
    {
        // Reference :
        // http://www.euclideanspace.com/maths/geometry/rotations/conversions/quaternionToEuler/
        Quaternion q = iRotation.normalized; // sqrt operation
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

        return new Vector3( pitch, yaw, roll);
    }
}