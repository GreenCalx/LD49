using UnityEngine;

/**
*   Translates from start to each point of the path
*   Start point is the camera position in the scene
*   Each path point, defined by a Transform
*   > handles posittion and rotation of the transform
*   Rotation is ABSOLUTE, not relative
*/
public class TravellingCamera : CinematicCamera
{
    public enum TRAVEL_MODE
    {
        LINEAR, // LERP
        SMOOTH, // SMOOTH STEP
    }
    public Transform[] path;
    public TRAVEL_MODE mode = TRAVEL_MODE.SMOOTH;
    public float step_duration = 2f;


    private float elapsedTime;
    private Transform startLocation = null;
    private Transform prevPathPoint;
    private Transform nextPathPoint;
    private int pathIterator;
    void Awake()
    {
        camType = CAM_TYPE.CINEMATIC;
    }
    // Start is called before the first frame update
    void Start()
    {
        elapsedTime = 0f;
        pathIterator = 0;


        if (startLocation == null)
        {
            startLocation = new GameObject("PATH_START").transform;
            startLocation.SetParent(transform.parent);
            startLocation.transform.position = transform.position;
            startLocation.transform.rotation = transform.rotation;
        }
        prevPathPoint = startLocation; // copy cam location for first LERP
        nextPathPoint = path[pathIterator];
    }

    // Update is called once per frame
    void Update()
    {
        if (!launched || CameraManager.Instance.inTransition)
            return;

        elapsedTime += Time.deltaTime;
        if (step_duration < elapsedTime)
        {
            if (!moveNext())
                end();
        }

        move();
    }

    public override void launch()
    {
        CameraManager.Instance.changeCamera(this, transitionIn);
        launched = true;
    }

    public override void end()
    {
        CameraManager.Instance.changeCamera(camTypeOnFinish, transitionOut);
        launched = false;
    }

    // Forces movement to next path point
    // @return true if next point exists, false otherwise
    public bool moveNext()
    {
        elapsedTime = 0f;
        pathIterator++;
        if (pathIterator > path.Length - 1)
            return false;
        prevPathPoint = nextPathPoint;
        nextPathPoint = path[pathIterator];

        transform.position = prevPathPoint.position;
        transform.rotation = prevPathPoint.rotation;
        return true;
    }

    public void move()
    {
        switch (mode)
        {
            case TRAVEL_MODE.LINEAR:
                transform.position = Vector3.Lerp(prevPathPoint.position,
                                                  nextPathPoint.position,
                                                  elapsedTime / step_duration);
                transform.rotation = Quaternion.Lerp(prevPathPoint.rotation,
                                                    nextPathPoint.rotation,
                                                    elapsedTime / step_duration);
                break;
            case TRAVEL_MODE.SMOOTH:
                float s = elapsedTime / step_duration;
                s = s * s * (3f - 2f * s); // smoothstep formula
                transform.position = Vector3.Lerp(prevPathPoint.position,
                                            nextPathPoint.position,
                                            s);
                transform.rotation = Quaternion.Lerp(prevPathPoint.rotation,
                                                    nextPathPoint.rotation,
                                                    s);
                break;
            default:
                mode = TRAVEL_MODE.LINEAR;
                move();
                break;
        }
    }

}
