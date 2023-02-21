using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
*       Root of camera hierarchy
*       Is affected to UNDEFINED by default
*       Is the common camera interface
*
*               _ CinematicCamera ___ TravellingCamera
*              /                 \___ OrbitingCamera
*             /                  \___ TransitionCamera
*            / 
* GameCamera
*            \___ PlayerCamera ___ FollowPlayer(TRACK)
*                             \___ ManualCamera(HUB)
*                             \___ FlyCamera(FlyMode) TBD
*                             \___ FPSCamera(FPS)
*
*/
public class GameCamera : MonoBehaviour
{
    public enum CAM_TYPE {
        UNDEFINED=0,    // No behaviour applied
        HUB=1,          // Camera used in the hub
                        // If in HUB scene, will resume to this camera for gameplay
                        // Multiple instances not supported atm (only the first found is used)
        TRACK=2,        // Camera used in tracks
                        // If in track scene, will resume to this camera for gameplay
                        // Multiple instances not supported atm (only the first found is used)
        BOSS=3,         // TBD
        CINEMATIC=4,     // Camera used for cutscenes/dialogs
                        // Bypasses other cameras for a given duration or trigger input
        TRANSITION=5,    // Camera used to transition between 2 cameras
                        // Is automatically cleaned by CameraManager on endTransition()
        FPS=6           // FPS Camera used for looping and such places with limited vision
    }
    [Header("GameCamera")]
    public CAM_TYPE camType = CAM_TYPE.UNDEFINED;

    protected float initial_FOV;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public virtual void init() {}
}
