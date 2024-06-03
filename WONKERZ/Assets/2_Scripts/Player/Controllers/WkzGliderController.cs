using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schnibble;
using Schnibble.Managers;

namespace Wonkerz {
    public class WkzGliderController : SchGliderController
    {
        void Awake() {
            accelerator = (int)PlayerInputs.InputCode.Accelerator;
            brake = (int)PlayerInputs.InputCode.Break;
            turn = (int)PlayerInputs.InputCode.Turn;
        }

        #if false
        public override void ProcessInputs(InputManager currentManager, GameController inputs) {
            base.ProcessInputs(currentManager, inputs);

            #if false
            // set car input values.
            var accelInput = inputs.Get((int)PlayerInputs.InputCode.Accelerator) as GameInputAxis;
            if (accelInput != null) plane.accel = accelInput.GetState().valueSmooth * plane.maxAccelAngle;

            plane.brakeLeft = 0.0f;
            plane.brakeRight = 0.0f;
            var brakeInput = inputs.Get((int)PlayerInputs.InputCode.Break) as GameInputAxis;
            if (brakeInput != null)
            {
                plane.brakeLeft += brakeInput.GetState().valueSmooth * plane.maxBrakeAngle;
                plane.brakeRight += brakeInput.GetState().valueSmooth * plane.maxBrakeAngle;
            }

            var steeringInput = inputs.Get((int)PlayerInputs.InputCode.Turn) as GameInputAxis;
            if (steeringInput != null) {
                plane.brakeLeft += steeringInput.GetState().valueSmooth > 0.0f ? 0.0f : -steeringInput.GetState().valueSmooth * plane.maxBrakeAngle;
                plane.brakeRight += steeringInput.GetState().valueSmooth < 0.0f ? 0.0f : steeringInput.GetState().valueSmooth * plane.maxBrakeAngle;
            }

            //plane.brakeLeft = Mathf.Clamp(0.0f, plane.maxBrakeAngle, plane.brakeLeft);
            //plane.brakeRight = Mathf.Clamp(0.0f, plane.maxBrakeAngle, plane.brakeRight);
            #endif
        }
        #endif
    }
}
