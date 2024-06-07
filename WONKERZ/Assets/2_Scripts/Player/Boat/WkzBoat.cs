

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Schnibble;

namespace Wonkerz
{
    public class WkzBoat : SchBoat
    {
        public SchWaterColliderManager waterCollider;
        // Start is called before the first frame update
        void Start()
        {
            if (waterCollider == null) {
                waterCollider = SchWaterColliderManager.inst;
            }
        }

        override protected void FixedUpdate()
        {
            var motorCollider = motorTransform.GetComponent<SphereCollider>();
            var collisions = waterCollider.GetCollisions(motorCollider);
            if (collisions.Count > 0.0f)
            {
                foreach (var p in collisions)
                {
                    var maxVolume = p.GetSphereVolume(motorCollider);
                    var submergedVolume = p.GetSubmergedVolume(motorCollider);
                    var motorPercentInWater = Mathf.Clamp(.0f, 1f, submergedVolume / maxVolume);
                }
            }

            motor.ApplyDamping(Time.fixedDeltaTime);
            var motorTorque = (float)motor.GetOutput(throttle);
            gearBox.SetInput(motorTorque);
            float clutch = 0.0f;
            gearBox.Update(Time.fixedDeltaTime, motor.GetRPMRatio(), false, ref clutch);

            var torque = gearBox.GetOutput();

            var velForward = Vector3.Dot(rb.velocity, rb.transform.forward);
            if (Mathf.Abs(velForward) > 1) rb.AddForce(brake * rb.transform.forward * -Mathf.Sign(velForward));

            rb.AddForceAtPosition(torque * motorTransform.forward, motorTransform.position);
        }
    }
}
