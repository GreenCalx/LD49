using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schnibble;
using static Schnibble.Physics;

namespace Wonkerz
{
    public class WkzCar : SchCar 
    {
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

        public SchJumpDef  jumpDef;
        public WonkerDecal jumpDecal;

        float jumpElapsedTime = 0.0f;
        bool isJumping = false;

        [HideInInspector]
        public Schnibble.Math.Accumulator weightPitch;
        [HideInInspector]
        public Schnibble.Math.Accumulator weightRoll;

        public float weightControlMaxX;
        public float weightControlMaxZ;

        public float centrifugalForceMul = 1.0f;
        public float minCentrifugalVelocity = 3.0f;

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

        protected override void Update() {
            base.Update();

            if (isJumping)
            {
                jumpElapsedTime += Time.deltaTime;
                SetSuspensionTargetPosition();
            }

            jumpDecal.SetAnimationTime(jumpElapsedTime / jumpDef.time);
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
            var jumpRatio = jumpElapsedTime / jumpDef.time;
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
                            targetPosition.y = Mathf.Lerp(physXBody.GetMaxLength(), jumpDef.min, jumpRatio);
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
        }

        public void StartJump()
        {
            // set the new suspension stiffness.
            // it will be used until it cant push up anymore.
            var jumpRatio = jumpElapsedTime / jumpDef.time;
            foreach (var w in constraintSolver.solver.joints)
            {
                var sus = w as SchSuspension;
                if (sus != null)
                {
                    // explode suspension.
                    sus.dampingMul = 0.0f;
                    sus.stiffnessMul = jumpDef.stiffnessMul * jumpRatio;
                }
            }
            // reset
            jumpElapsedTime = 0.0f;
            SetSuspensionTargetPosition();
            isJumping = false;
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
            chassis.rb.SetCenterOfMassLocal(chassis.localCenterOfMass + new Vector3(weightRoll.average * weightControlMaxX, 0f, weightPitch.average * weightControlMaxZ));
        }

        public void SetCarAerialTorque(float dt)
        {
            var torque = new Vector3(jumpDef.diMaxForce * weightPitch.average,
                0,
                -jumpDef.diMaxForce * weightRoll.average);
            torque = chassis.rb.GetPhysXBody().transform.TransformDirection(torque);

            chassis.rb.GetPhysXBody().AddTorque(torque * dt, ForceMode.VelocityChange);
        }
    }
}
