

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Schnibble;

namespace Wonkerz
{
    public class WkzBoat : SchBoat
    {
        // TODO: create boat def
        // for now duplicated from WkzCarSO.cs
        [Header("Gameplay")]
        public float aerialMaxForce = 5.0f;
        public float groundAerialSwitchLatency = 0.1f;
        public float weightControlMaxX = 1.0f;
        public float weightControlMaxZ = 1.0f;
        public float weightControlMaxY = 0.0f;
        public float weightControlSpeed = 1.0f;
        public float centrifugalForceMul = 1.0f;
        public float minCentrifugalVelocity = 3.0f;

        public SchWaterColliderManager waterCollider;

        public List<SchBuoyancy> floaters = new List<SchBuoyancy>();
        // Start is called before the first frame update
        void Start()
        {
            if (waterCollider == null) {
                waterCollider = SchWaterColliderManager.inst;
            }
        }

        public void SetFloatersSize(float mul) {
            foreach (var f in floaters) {
                f.c.radius = f.floaterSize * mul;
            }
        }

        public bool IsInAir() {
            // TODO: make this more robust,
            // for now just check if one wheel touch some water.
            foreach (var f in floaters) {
                if (f.hasContacts) return false;
            }
            return true;
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
