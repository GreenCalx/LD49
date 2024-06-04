using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schnibble;
using static Schnibble.Physics;

#if SCH_SI_UNITS
using Schnibble.SIUnits;
using Inertia = Schnibble.SIUnits.KilogramMeter2;
using RadianPerSec = Schnibble.SIUnits.PerSecond;
using Stiffness = Schnibble.SIUnits.KilogramPerSecond2;
using AngularVelocity = Schnibble.SIUnits.PerSecond;
using Torque = Schnibble.SIUnits.KilogramMeter2PerSecond2;
using Force = Schnibble.SIUnits.KilogramMeterPerSecond2;
#else
using Inertia = Schnibble.SIUnits.FloatWrapper;
using RadianPerSec = Schnibble.SIUnits.FloatWrapper;
using Stiffness = Schnibble.SIUnits.FloatWrapper;
using Torque = Schnibble.SIUnits.FloatWrapper;
using Degree = Schnibble.SIUnits.FloatWrapper;
using KilogramMeter2 = Schnibble.SIUnits.FloatWrapper;
using Radian = Schnibble.SIUnits.FloatWrapper;
#endif

namespace Wonkerz
{
    public class WkzWheelCollider : SchWheelCollider
    {
        public override void FixedUpdate()
        {
            base.FixedUpdate();

            var wkzCar = chassis.GetCar() as WkzCar;
            // centrifugal forces
            AngularVelocity chassisAngVelY = (AngularVelocity)chassis.GetBody().angularVelocity.y;
            if (Mathf.Abs((float)chassisAngVelY) > wkzCar.wkzDef.minCentrifugalVelocity)
            {
                Vector3 radius = suspension.GetAnchorA() - suspension.GetAnchorB();
                Meter radiusLength = (Meter)radius.magnitude;
                Vector3 radiusDir = radius / (float)radiusLength;

                Force centrifugalForceZ = wkzCar.wkzDef.centrifugalForceMul * (chassisAngVelY * chassisAngVelY) * (tire.mass * radiusLength);

                Vector3 up = Vector3.Cross(radiusDir, chassis.transform.right);
                Vector3 tangent = Vector3.Cross(up, radiusDir);
                UnityEngine.Debug.DrawLine(transform.position, transform.position + radiusDir * (float)centrifugalForceZ, Color.yellow);
                chassis.GetBody().AddForceAtPosition((float)centrifugalForceZ * radiusDir, transform.position);
            }
        }
    }
}
