using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schnibble;
using Schnibble.Managers;

namespace Wonkerz {
    public class WkzGliderController : SchGliderController
    {
        public PlayerController.InputMode inputMode = PlayerController.InputMode.Local;

        void Awake() {
            accelerator = (int)PlayerInputs.InputCode.Accelerator;
            brake       = (int)PlayerInputs.InputCode.Break;
            turn        = (int)PlayerInputs.InputCode.Turn;
        }
    }
}
