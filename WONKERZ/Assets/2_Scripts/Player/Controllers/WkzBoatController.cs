using Schnibble;
using Schnibble.Managers;

namespace Wonkerz
{
    public class WkzBoatController : SchBoatController
    {
        public PlayerController.InputMode inputMode = PlayerController.InputMode.Local;

        int brake = (int)PlayerInputs.InputCode.Break;
        int gear = (int)PlayerInputs.InputCode.ForwardBackward;

        void Awake()
        {
            accelerator = (int)PlayerInputs.InputCode.Accelerator;
            turn = (int)PlayerInputs.InputCode.Turn;
        }

        public override void ProcessInputs(InputManager currentManager, GameController inputs)
        {
            base.ProcessInputs(currentManager, inputs);
            var gearInput = inputs.GetAxisState(gear);
            if (gearInput.valueRaw > 0.9f) boat.gearBox.invertInput = false;
            if (gearInput.valueRaw < 0.9f) boat.gearBox.invertInput = true;
        }
    }
}
