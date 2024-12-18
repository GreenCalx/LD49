using Schnibble;
using Schnibble.Managers;

namespace Wonkerz
{
    public class WkzCarController : SchCarController
    {
        // TODO: create real online version
        // should be on anline object
        PlayerController.InputMode _inputMode = PlayerController.InputMode.Local;
        public PlayerController.InputMode inputMode
        {
            get
            {
                return _inputMode;
            }
            set
            {
                _inputMode = value;
                if (wkzCar != null)
                {
                    wkzCar.onlineMode = (SchMotoredVehicle.OnlineMode)((int)_inputMode);
                }
            }
        }


        public WkzCar wkzCar {get; private set;}

        int gearUpDown = (int)PlayerInputs.InputCode.ForwardBackward;
        int jump = (int)PlayerInputs.InputCode.Jump;
        int weightX = (int)PlayerInputs.InputCode.WeightX;
        int weightY = (int)PlayerInputs.InputCode.WeightY;

        void Awake()
        {
            if (car != null)
            {
                if (car as WkzCar == null)
                {
                    this.LogError("car should be of wkzCar type.");
                }
                wkzCar = car as WkzCar;
            }
            else
            {
                this.LogError("CarController does not have a Car objects attached.");
            }

            accelerator = (int)PlayerInputs.InputCode.Accelerator;
            brake = (int)PlayerInputs.InputCode.Break;
            handbrake = (int)PlayerInputs.InputCode.Handbrake;
            turn = (int)PlayerInputs.InputCode.Turn;
        }

        public void SetCar(SchCar car)
        {
            if (car != null)
            {
                if (car as WkzCar == null)
                {
                    this.LogError("car should be of wkzCar type.");
                }
                else
                {
                    this.car = car;
                    this.wkzCar = car as WkzCar;
                }
            }
            else
            {
                this.car = null;
                this.wkzCar = null;
            }
        }

        public WkzCar GetCar()
        {
            return wkzCar;
        }

        void FixedUpdate()
        {
            firstProcessInputs = true;
        }

        bool firstProcessInputs = true;
        public override void ProcessInputs(InputManager mgr, GameController inputs)
        {
            // TODO:
            // This is because ProcessInputs is called in Update,
            // but weightRoll/Pitch are used in FixedUpdate.
            // FixedUpdate con be called several times so we cannot reset in it.
            // But we dont have yet any syncro point after EVERY fixedupdate has passed.
            // One of those point would be the InputManager Update.
            // Therefore we reset it the first time we process inputs.
            // In the future we need a way to do it more elegantly.
            if (firstProcessInputs)
            {
                wkzCar.weightRoll.Reset();
                wkzCar.weightPitch.Reset();
                firstProcessInputs = false;
            }

            base.ProcessInputs(mgr, inputs);

            var gearValue = inputs.GetAxisState(gearUpDown).valueRaw;
            if      (gearValue > 0.9f ) car.gearBox.GoForward();
            else if (gearValue < -0.9f) car.gearBox.GoBackward();

            var jumpValue = inputs.GetButtonState(jump);
            if (jumpValue.up)
            {
                if (!wkzCar.jumpLock) wkzCar.StopSuspensionCompression();
            }
            else if (jumpValue.down)
            {
                if (!wkzCar.IsInJumpLatency()) wkzCar.StartSuspensionCompression();
            }

            wkzCar.weightRoll.Add(inputs.GetAxisState(weightX).valueSmooth);
            wkzCar.weightPitch.Add(inputs.GetAxisState(weightY).valueSmooth);
        }
    }
}
