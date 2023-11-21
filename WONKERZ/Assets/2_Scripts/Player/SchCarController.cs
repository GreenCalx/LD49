using UnityEngine;
using System;
using Schnibble.Managers;

namespace Schnibble {
    public class SchCarController : MonoBehaviour, IControllable {
        public SchCar car;

        public PlayerInputsManager inputMgr;

        void Start() {
            inputMgr.player1.Attach(this);
        }

        void IControllable.ProcessInputs(InputManager mgr, GameController inputs) {
            // set car input values.
            var accelInput = inputs.Get((int)PlayerInputs.InputCode.Accelerator) as GameInputAxis;
            if (accelInput != null) car.engine.throttle = accelInput.GetState().valueSmooth;

            var brakeInput = inputs.Get((int)PlayerInputs.InputCode.Break) as GameInputAxis;
            if (brakeInput != null) car.brakeDifferential.SetInput(brakeInput.GetState().valueSmooth);

            var steeringInput = inputs.Get((int)PlayerInputs.InputCode.Turn) as GameInputAxis;
            if (steeringInput != null) car.steeringDifferential.SetInput(steeringInput.GetState().valueSmooth);

            var handbrakeInput = inputs.Get((int)PlayerInputs.InputCode.Handbrake) as GameInputButton;
            if (handbrakeInput != null) car.handBrakeDifferential.SetInput(handbrakeInput.GetState().heldDown ? 1.0f : 0.0f);
        }
    }
}

