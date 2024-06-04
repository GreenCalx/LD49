using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schnibble;
using static Schnibble.Physics;

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
            public float min;
            public float stiffnessMul;
            public float diMaxForce;
        };
        public WonkerDecal jumpDecal;

        float jumpElapsedTime = 0.0f;
        bool isJumping = false;

        [HideInInspector]
        public Schnibble.Math.Accumulator weightPitch;
        [HideInInspector]
        public Schnibble.Math.Accumulator weightRoll;

        public bool IsTouchingGround() {
            foreach (var a in chassis.axles) {
                if (a.right != null && a.right.grounded) return true;
                if (a.left  != null && a.left.grounded ) return true;
            }
            return false;
        }

        public bool IsTouchingGroundAllWheels() {
            foreach (var a in chassis.axles) {
                if (a.right != null && !a.right.grounded) return false;
                if (a.left  != null && !a.left.grounded ) return false;
            }
            return false;
        }

        protected override void Awake()
        {
            base.Awake();

            wkzDef = def as WkzCarSO;
        }

        protected override void Update() {
            base.Update();

            if (isJumping)
            {
                jumpElapsedTime += Time.deltaTime;
                SetSuspensionTargetPosition();
            }

            jumpDecal.SetAnimationTime(jumpElapsedTime / wkzDef.jumpDef.time);
        }

        protected override void FixedUpdate() {
            base.FixedUpdate();
        }

        public float GetSpeedEffectRatio() {
            return Mathf.Clamp(((float)GetCurrentSpeedInKmH() - speedEffect.thresholdSpeedInKmH) / speedEffect.maxSpeedInKmH, 0.0f, 1.0f);
        }

        // Cleanup : remove temporary function
        // cause : very bad design
        public void StartSuspensionCompression() {
            isJumping = true;
            SetSuspensionTargetPosition();
        }

        public void StopSuspensionCompression() {
            StartJump();
        }

        public void SetSuspensionTargetPosition()
        {
            var jumpRatio = jumpElapsedTime / wkzDef.jumpDef.time;
            // TODO: do not use car def for suspension maxlength
            var targetPosition = Mathf.Lerp(def.frontSuspension.maxLength, wkzDef.jumpDef.min, jumpRatio);
            chassis.SetAxlesSuspensionTargetPosition(targetPosition);

            #if false
            if (constraintSolver)
            {
                foreach (var w in constraintSolver.solver.joints)
                {
                    if (type == SchCar.Type.New)
                    {
                        var physXBody = w as SchSuspensionPhysX;
                        if (physXBody != null)
                        {
                            ConfigurableJoint cj = physXBody.joint;
                            if (cj == null)
                            {
                                this.LogError("Should not happen !");
                                return;
                            }

                            var targetPosition = cj.targetPosition;
                            targetPosition.y = Mathf.Lerp(physXBody.GetMaxLength(), wkzDef.jumpDef.min, jumpRatio);
                            cj.targetPosition = targetPosition;

                            var linearLimit = cj.linearLimit;
                            linearLimit.limit = targetPosition.y;
                            cj.linearLimit = linearLimit;
                        }
                    }
                }

                #if SCH_TELEMETRY
                RegisterJump(dt);
                #endif
            }
            #endif

        }

        public void StartJump()
        {
            // set the new suspension stiffness.
            // it will be used until it cant push up anymore.
            var jumpRatio = jumpElapsedTime / wkzDef.jumpDef.time;
            chassis.SetAxlesSuspensionMultipliers(wkzDef.jumpDef.stiffnessMul * jumpRatio, 0.0f);
            // reset
            jumpElapsedTime = 0.0f;
            SetSuspensionTargetPosition();
            isJumping = false;

            #if false
            foreach (var w in constraintSolver.solver.joints)
            {
                var sus = w as SchSuspension;
                if (sus != null)
                {
                    // explode suspension.
                    sus.dampingMul = 0.0f;
                    sus.stiffnessMul = wkzDef.jumpDef.stiffnessMul * jumpRatio;
                }
            }
            // reset
            jumpElapsedTime = 0.0f;
            SetSuspensionTargetPosition();
            isJumping = false;
            #endif
        }

        public void ResetWheels()
        {
            motor.Reset();
            gearBox.Reset();

            foreach (var a in chassis.axles) {
                if (a.right) a.right.Reset();
                if (a.left ) a.left.Reset();
            }
        }

        public void SetCarCenterOfMass()
        {
            var offset = new Vector3(weightRoll.average * wkzDef.weightControlMaxX, 0f, weightPitch.average * wkzDef.weightControlMaxZ);
            chassis.OffsetCenterOfMass(offset);
        }

        public void SetCarAerialTorque(float dt)
        {
            var torque = new Vector3(wkzDef.jumpDef.diMaxForce * weightPitch.average,
                0,
                -wkzDef.jumpDef.diMaxForce * weightRoll.average);
            torque = chassis.GetBody().transform.TransformDirection(torque);

            chassis.GetBody().AddTorque(torque * dt, ForceMode.VelocityChange);
        }
    }
}
