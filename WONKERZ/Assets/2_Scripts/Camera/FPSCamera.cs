using Schnibble;

namespace Wonkerz
{

    public class FPSCamera : PlayerCamera
    {
        protected override void Awake()
        {
            init();
        }

        // Start is called before the first frame update
        void Start()
        {
            init();
        }

        // Update is called once per frame
        void Update()
        {
            if (playerRef == null)
            {
                this.LogWarn("Bad ref on player in FPSCamera.");
                playerRef = Access.Player().GetTransform().gameObject;
            }
            transform.position = playerRef.transform.position;
            transform.rotation = playerRef.transform.rotation;
        }

        public override void init()
        {
            camType = CAM_TYPE.FPS;

            if (playerRef == null)
            {
                this.LogWarn("Bad ref on player in FPSCamera.");
                playerRef = Access.Player().GetTransform().gameObject;
            }

            transform.position = playerRef.transform.position;
            transform.rotation = playerRef.transform.rotation;
        }
    }
}
