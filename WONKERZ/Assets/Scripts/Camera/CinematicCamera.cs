
/**
*   A static cinematic camera
*/
public class CinematicCamera : GameCamera
{
    public delegate int OnCinematicEnd();

    public GameCamera.CAM_TYPE camTypeOnFinish = GameCamera.CAM_TYPE.HUB;
    protected bool launched = false;
    void Awake()
    {
        camType = CAM_TYPE.CINEMATIC;
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

    public virtual void launch()
    {
        CameraManager.Instance.changeCamera(this);
        launched = true;
    }

    public virtual void end()
    {
        launched = false;
        CameraManager.Instance.changeCamera(camTypeOnFinish);
    }
}
