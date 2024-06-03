using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schnibble;
using Schnibble.Managers;

namespace Wonkerz {
    public class WkzBoatController : SchBoatController 
    {
        int brake = (int)PlayerInputs.InputCode.Break;

        void Awake() {
            accelerator = (int)PlayerInputs.InputCode.Accelerator;
        }

        public void ProcessInputs(InputManager currentManager, GameController inputs) {
            #if false
            var gear = inputs.Get((int)PlayerInputs.InputCode.ForwardBackward) as GameInputAxis;
            if (gear != null) motor.forward = (gear.GetState().valueRaw > 0.9f
                ? true 
                : (gear.GetState().valueRaw < -0.9f)
                    ? false 
                    : motor.forward);
            #endif
        }
    }
}
