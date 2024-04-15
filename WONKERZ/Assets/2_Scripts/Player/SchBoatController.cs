using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wonkerz;
using Schnibble.Managers;

namespace Schnibble {
    public class SchBoatController : MonoBehaviour, Managers.IControllable
    {
        public Rigidbody rb;

        public SchBoatMotor motor;

        public void ProcessInputs(Managers.InputManager currentManager, Managers.GameController inputs) {
            // set car input values.
            var accelInput = inputs.Get((int)PlayerInputs.InputCode.Accelerator) as GameInputAxis;
            if (accelInput != null) motor.throttle = accelInput.GetState().valueSmooth;

            var brakeInput = inputs.Get((int)PlayerInputs.InputCode.Break) as GameInputAxis;
            if (brakeInput != null) motor.brake = brakeInput.GetState().valueSmooth * motor.maxBrake;

            var gear = inputs.Get((int)PlayerInputs.InputCode.ForwardBackward) as GameInputAxis;
            if (gear != null) motor.forward = (gear.GetState().valueRaw > 0.9f
                ? true 
                : (gear.GetState().valueRaw < -0.9f)
                    ? false 
                    : motor.forward);

            var steeringInput = inputs.Get((int)PlayerInputs.InputCode.Turn) as GameInputAxis;
            if (steeringInput != null) {
                var steerIn = steeringInput.GetState().valueSmooth;
                motor.transform.localRotation *= Quaternion.AngleAxis(motor.maxAngle * (steerIn - motor.lastSteerInput), Vector3.up);
                motor.lastSteerInput = steerIn;
            }
        }

    }
}
