using Schnibble;
using UnityEngine;

namespace Wonkerz
{
    public class WkzCar : SchCar
    {
        public WkzCarSO wkzDef;
        public WkzCarSO wkzMutDef;

        [System.Serializable]
        public struct SpeedTrailEffect
        {
            public ParticleSystem particles;
            public float thresholdSpeedInKmH;
            public float maxSpeedInKmH;
        }
        [Header("Effects")]
        public SpeedTrailEffect speedEffect;

        [System.Serializable]
        public struct SchJumpDef
        {
            public float time;
            public float latency;
            [Range(0.1f, 0.9f)]
            public float minLengthPercent;//< in percent of maxLength;
            public float stiffnessMul;
        };
        public WonkerDecal jumpDecal;

        float jumpTimeCurrent = 0.0f;
        float jumpLatencyCurrent = 0.0f;

        [HideInInspector]
        public Schnibble.Math.Accumulator weightPitch;
        [HideInInspector]
        public Schnibble.Math.Accumulator weightRoll;

        public bool IsTouchingGround()
        {
            foreach (var a in chassis.axles)
            {
                if (a.right != null && a.right.grounded) return true;
                if (a.left != null && a.left.grounded) return true;
            }
            return false;
        }

        public bool IsTouchingGroundAllWheels()
        {
            foreach (var a in chassis.axles)
            {
                if (a.right != null && !a.right.grounded) return false;
                if (a.left != null && !a.left.grounded) return false;
            }
            return true;
        }

        protected override void Awake()
        {
            base.Awake();

            wkzDef = def as WkzCarSO;
            wkzDef.CopyTo(ref wkzMutDef);
        }

        float GetJumpCompressionRatio() => 1.0f - Mathf.Clamp01(jumpTimeCurrent / wkzMutDef.jumpDef.time);

        public bool IsInJumpLatency() => jumpLatencyCurrent > 0.0f;

        protected override void Update()
        {
            base.Update();

            if (jumpTimeCurrent > 0.0f)
            {
                SetSuspensionTargetPosition();
                jumpTimeCurrent -= Time.deltaTime;
                jumpDecal.SetJumpTime(GetJumpCompressionRatio());
            }
            else if (jumpLatencyCurrent > 0.0f)
            {
                jumpLatencyCurrent -= Time.deltaTime;
                jumpDecal.SetLatencyTime(Mathf.Clamp01(jumpLatencyCurrent / wkzMutDef.jumpDef.latency));
            }
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
        }

        public float GetSpeedEffectRatio()
        {
            return Mathf.Clamp(((float)GetCurrentSpeedInKmH() - speedEffect.thresholdSpeedInKmH) / speedEffect.maxSpeedInKmH, 0.0f, 1.0f);
        }

        public void StartSuspensionCompression()
        {
            if (IsInJumpLatency())
            {
                this.LogError("Should never happen.");
            }

            jumpTimeCurrent = wkzMutDef.jumpDef.time;
            jumpLock = false;
        }

        public bool jumpLock = true;
        public void StopSuspensionCompression()
        {
            if (jumpLock)
            {
                this.LogError("Should never happen.");
            }

            StartJump();
        }

        public void SetSuspensionTargetPosition()
        {
            chassis.SetAxlesSuspensionTargetPosition(Mathf.Lerp(1.0f, wkzMutDef.jumpDef.minLengthPercent, GetJumpCompressionRatio()));

#if SCH_TELEMETRY
        RegisterJump(dt);
#endif
        }

        public void StartJump()
        {
            jumpLock = true;
            jumpTimeCurrent = 0.0f;
            jumpLatencyCurrent = wkzMutDef.jumpDef.latency;
            // set the new suspension stiffness.
            // it will be used until it cant push up anymore.
            chassis.SetAxlesSuspensionMultipliers(wkzMutDef.jumpDef.stiffnessMul, 0.0f);
            chassis.SetAxlesSuspensionTargetPosition(1.0f);
            jumpDecal.SetAnimationTime(0.0f);
        }

        public void ResetWheels()
        {
            motor.Reset();
            gearBox.Reset();

            foreach (var a in chassis.axles)
            {
                if (a.right) a.right.Reset();
                if (a.left) a.left.Reset();
            }
        }

        public void ResetCarCenterOfMass() {
            chassis.OffsetCenterOfMass(Vector3.zero);
        }

        public void SetCarCenterOfMass(float dt)
        {
            var maxVect = new Vector3(wkzMutDef.weightControlMaxX, .0f, wkzMutDef.weightControlMaxZ);
            var targetOffset = new Vector3(weightRoll.average * wkzMutDef.weightControlMaxX, 0f, weightPitch.average * wkzMutDef.weightControlMaxZ);
            // try hemisphere instead of plan.
            // COM will be lowered the farther out it is.
            var Y = (targetOffset.magnitude / maxVect.magnitude) * wkzMutDef.weightControlMaxY;
            targetOffset.y = Y;

            var currentOffset = chassis.GetBody().centerOfMass - chassis.def.localCenterOfMass;

            var diffOffset = targetOffset - currentOffset;
            var offset = currentOffset + (diffOffset * Mathf.Clamp01(wkzMutDef.weightControlSpeed * dt));
            chassis.OffsetCenterOfMass(offset);
        }

        public void SetCarAerialTorque(float dt)
        {
            // TODO:
            // Should we use the current COM or the body position to set a
            // Torque that is centered always?
            // Should we have speed?
            var torque = new Vector3(wkzMutDef.aerialMaxForce * weightPitch.average,
                0,
                -wkzMutDef.aerialMaxForce * weightRoll.average);

            torque = chassis.GetBody().transform.TransformDirection(torque);

            chassis.GetBody().AddTorque(torque * dt, ForceMode.VelocityChange);
        }
    }
}
