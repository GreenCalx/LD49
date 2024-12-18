using UnityEngine;

/**
*       Root of camera hierarchy
*       Is affected to UNDEFINED by default
*       Is the common camera interface
*
*               _ CinematicCamera ___ TravellingCamera
*              /                 \___ CinematicCamera
*             /                  \___ TransitionCamera
             |                   \___ FollowObjectCamera
*           | 
* GameCamera
*            \___ PlayerCamera ___ FollowPlayer(TRACK)
*                             \___ ManualCamera(ORBIT)
*                             \___ FlyCamera(FlyMode) TBD
*                             \___ FPSCamera(FPS)
*           \____ UICamera (UI)
*           \____ InitCamera (INIT)
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
        UI=7,           // UI Only
        OLD_TRACK = 8,
        INIT=9         // First loaded cam in a scene, Used to init transitive parameters/do controlled transition
    }
    [Header("GameCamera")]
    public CAM_TYPE camType = CAM_TYPE.UNDEFINED;
    public Camera cam;
    protected float initial_FOV;

    protected virtual void Awake() {
        if (cam == null) cam = GetComponent<Camera>();
        if (cam != null) initial_FOV = cam.fieldOfView;
    }

    public virtual void init() {}

    public virtual void resetView() {
        if (cam != null) {
            cam.transform.localRotation = Quaternion.identity.normalized;
            cam.transform.localPosition = Vector3.zero;
            cam.fieldOfView = initial_FOV;
        }
    }

    public virtual void disable() {
        this.gameObject.SetActive(false);
        this.enabled            = false;
        this.cam.enabled        = false;
    }

    public virtual void enable() {
        this.enabled            = true;
        this.cam.enabled        = true;
        this.gameObject.SetActive(true);
    }
}
