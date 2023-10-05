using UnityEngine;

/**
*       Root of camera hierarchy
*       Is affected to UNDEFINED by default
*       Is the common camera interface
*
*               _ CinematicCamera ___ TravellingCamera
*              /                 \___ CinematicCamera
*             /                  \___ TransitionCamera
*            / 
* GameCamera
*            \___ PlayerCamera ___ FollowPlayer(TRACK)
*                             \___ ManualCamera(ORBIT)
*                             \___ FlyCamera(FlyMode) TBD
*                             \___ FPSCamera(FPS)
*           \____ UICamera (UI)
*
*
*/
public class GameCamera : MonoBehaviour
{
    public enum CAM_TYPE {
        UNDEFINED=0,    // No behaviour applied
        ORBIT=1,          // Main Camera type
                        // If in HUB scene, will resume to this camera for gameplay
                        // Multiple instances not supported atm (only the first found is used)
        ORBIT_LARGE=2,
        BOSS=3,         // TBD
        CINEMATIC=4,     // Camera used for cutscenes/dialogs
                        // Bypasses other cameras for a given duration or trigger input
        TRANSITION=5,    // Camera used to transition between 2 cameras
                        // Is automatically cleaned by CameraManager on endTransition()
        FPS=6,           // FPS Camera used for looping and such places with limited vision
        UI=7,
        OLD_TRACK = 8
    }
    [Header("GameCamera")]
    public CAM_TYPE camType = CAM_TYPE.UNDEFINED;
    public Camera cam;
    protected float initial_FOV;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public virtual void init() { }

    public virtual void resetView() { }
}
