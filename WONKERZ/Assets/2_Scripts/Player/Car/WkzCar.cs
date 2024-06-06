using Schnibble;
using UnityEngine;

namespace Wonkerz
{
    public class WkzCar : SchCar
    {
        public WkzCarSO wkzDef;

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

        }

        float GetJumpCompressionRatio() => 1.0f - Mathf.Clamp01(jumpTimeCurrent / wkzDef.jumpDef.time);

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
                jumpDecal.SetLatencyTime(Mathf.Clamp01(jumpLatencyCurrent / wkzDef.jumpDef.latency));
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

            jumpTimeCurrent = wkzDef.jumpDef.time;
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

        void SetSuspensionTargetPosition()
        {
            chassis.SetAxlesSuspensionTargetPosition(Mathf.Lerp(1.0f, wkzDef.jumpDef.minLengthPercent, GetJumpCompressionRatio()));

#if SCH_TELEMETRY
        RegisterJump(dt);
#endif
        }

        public void StartJump()
        {
            jumpLock = true;
            jumpTimeCurrent = 0.0f;
            jumpLatencyCurrent = wkzDef.jumpDef.latency;
            // set the new suspension stiffness.
            // it will be used until it cant push up anymore.
            chassis.SetAxlesSuspensionMultipliers(wkzDef.jumpDef.stiffnessMul, 0.0f);
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

        public void SetCarCenterOfMass(float dt)
        {
            var targetOffset = new Vector3(weightRoll.average * wkzDef.weightControlMaxX, 0f, weightPitch.average * wkzDef.weightControlMaxZ);
            var currentOffset = chassis.GetBody().centerOfMass - chassis.def.localCenterOfMass;

            var diffOffset = targetOffset - currentOffset;
            var offset = currentOffset + (diffOffset * Mathf.Clamp01(wkzDef.weightControlSpeed * dt));
            chassis.OffsetCenterOfMass(offset);
        }

        public void SetCarAerialTorque(float dt)
        {
            // TODO:
            // Should we use the current COM or the body position to set a
            // Torque that is centered always?
            // Should we have speed?
            var torque = new Vector3(wkzDef.aerialMaxForce * weightPitch.average,
                0,
                -wkzDef.aerialMaxForce * weightRoll.average);

            torque = chassis.GetBody().transform.TransformDirection(torque);

            chassis.GetBody().AddTorque(torque * dt, ForceMode.VelocityChange);
        }
    }
}
