namespace Wonkerz {
    public class TransitionCamera : CinematicCamera
    {
        protected override void Awake()
        {
            camType = CAM_TYPE.TRANSITION;
        }

        public override void launch()
        {
            launched = true;
        }

        public override void end()
        {
            launched = false;
            CameraManager.Instance.changeCamera(camTypeOnFinish, false);
        }
    }
}
