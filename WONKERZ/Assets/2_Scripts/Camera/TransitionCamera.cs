namespace Wonkerz {
    public class TransitionCamera : CinematicCamera
    {
        void Awake()
        {
            camType = CAM_TYPE.TRANSITION;
        }
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public override void init()
        {

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
