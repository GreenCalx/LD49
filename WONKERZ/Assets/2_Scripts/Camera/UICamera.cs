
/*
* A Camera for UI only scenes
*/
public class UICamera : GameCamera
{
    void Awake()
    {
        camType = CAM_TYPE.UI;
    }
    // Start is called before the first frame update
    void Start()
    {
        Access.CameraManager().changeCamera(camType);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public override void init()
    {
        
    }
}