namespace Wonkerz {
    /**
     *   A static cinematic camera
     */
    public class CinematicCamera : GameCamera
    {
        public delegate int OnCinematicEnd();

        public GameCamera.CAM_TYPE camTypeOnFinish = GameCamera.CAM_TYPE.ORBIT;
        public bool transitionIn = true;
        public bool transitionOut = true;
        public bool exitToCamTypeOnFinish = true;
        protected bool launched = false;
        protected override void Awake()
        {
            camType = CAM_TYPE.CINEMATIC;
        }

        public virtual void launch()
        {
            CameraManager.Instance.changeCamera(this, transitionIn);
            launched = true;
        }

        public virtual void end()
        {
            launched = false;
            if (exitToCamTypeOnFinish)
            CameraManager.Instance.changeCamera(camTypeOnFinish, transitionOut);
        }
    }
}
