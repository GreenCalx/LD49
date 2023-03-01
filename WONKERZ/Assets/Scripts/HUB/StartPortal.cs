using UnityEngine;
using Schnibble;

/// STARTING POINT FOR HUB
// > ACTIVATES TRICKS AUTO
public class StartPortal : AbstractCameraPoint
{
    [Header("Behaviour")]
    public bool enable_tricks = false;
    public GameCamera.CAM_TYPE camera_type;

    [Header("Optionals")]
    public Transform facingPoint;

    private GameObject playerRef;

    // Start is called before the first frame updatezd
    void Start()
    {
        playerRef = Utils.getPlayerRef();
        CarController player = playerRef.GetComponent<CarController>();
        if (!!player)
        {
            relocatePlayer();
            Access.CameraManager().changeCamera(camera_type);

            if (enable_tricks)
                activateTricks();
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void relocatePlayer()
    {
        playerRef.transform.position = transform.position;
        playerRef.transform.rotation = Quaternion.identity;
        if (facingPoint != null)
        {
            playerRef.transform.LookAt(facingPoint.transform);
        }
        Rigidbody rb2d = playerRef.GetComponentInChildren<Rigidbody>();
        if (!!rb2d)
        {
            rb2d.velocity = Vector3.zero;
            rb2d.angularVelocity = Vector3.zero;
        }
    }

    private void activateTricks()
    {
        TrickTracker tt = playerRef.GetComponent<TrickTracker>();
        if (!!tt)
        {
            tt.activate_tricks = true; // activate default in hub
        }
    }

}
