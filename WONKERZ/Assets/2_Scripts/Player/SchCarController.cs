using UnityEngine;
using System;
using Schnibble.Managers;

namespace Schnibble {
    public class SchCarController : MonoBehaviour, IControllable {
        public SchCar car;

        // Cleanup : move back to PlayerController.
        [System.Serializable]
        public struct SpeedTrailEffect
        {
            public ParticleSystem particles;

            public float thresholdSpeedInKmH;

            public float maxSpeedInKmH;
        }
        [Header("Effects")]
        public SpeedTrailEffect speedEffect;

        public float jumpElapsedTime = 0.0f;
        public float jumpTime = 1.0f;

        public float maxTorque = 200.0f;
        // end cleanup

        public void ProcessInputs(InputManager mgr, GameController inputs) {
            // set car input values.
            var accelInput = inputs.Get((int)PlayerInputs.InputCode.Accelerator) as GameInputAxis;
            if (accelInput != null) car.engine.throttle = accelInput.GetState().valueSmooth;

            var brakeInput = inputs.Get((int)PlayerInputs.InputCode.Break) as GameInputAxis;
            if (brakeInput != null) car.brakeDifferential.SetInput(brakeInput.GetState().valueSmooth);

            var steeringInput = inputs.Get((int)PlayerInputs.InputCode.Turn) as GameInputAxis;
            if (steeringInput != null) car.steeringDifferential.SetInput(steeringInput.GetState().valueSmooth);

            var handbrakeInput = inputs.Get((int)PlayerInputs.InputCode.Handbrake) as GameInputButton;
            if (handbrakeInput != null) car.handBrakeDifferential.SetInput(handbrakeInput.GetState().heldDown ? 1.0f : 0.0f);

            var gear = inputs.Get((int)PlayerInputs.InputCode.ForwardBackward) as GameInputAxis;
            if (gear != null) car.forward = (gear.GetState().valueRaw > 0.9f
                ? true 
                : (gear.GetState().valueRaw < -0.9f)
                    ? false 
                    : car.forward);

            var jump = inputs.Get((int)PlayerInputs.InputCode.Jump) as GameInputButton;
            if (jump != null) {
                if (jump.GetState().up)
                {
                    // start jump phase
                    var lerp = Mathf.Clamp(jumpElapsedTime / jumpTime, 0.0f, 1.0f);
                    car.StartJump(lerp);
                    // reset
                    jumpElapsedTime = 0.0f;
                    car.SetSuspensionTargetPosition(jumpElapsedTime);
                }
                else if (jump.GetState().heldDown)
                {
                    jumpElapsedTime += Time.deltaTime;

                    var lerp = Mathf.Clamp(jumpElapsedTime / jumpTime, 0.0f, 1.0f);
                    car.SetSuspensionTargetPosition(lerp);
                }
            }

            var weightXAxis = inputs.Get((int)PlayerInputs.InputCode.WeightX) as GameInputAxis;
            var weightYAxis = inputs.Get((int)PlayerInputs.InputCode.WeightY) as GameInputAxis;

            if (weightXAxis != null)
            {
                car.weightRoll.Add(weightXAxis.GetState().valueSmooth);
            }

            if (weightYAxis != null)
            {
                car.weightPitch.Add(weightYAxis.GetState().valueSmooth);
            }
        }
    }
}

