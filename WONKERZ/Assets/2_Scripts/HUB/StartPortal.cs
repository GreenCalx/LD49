using UnityEngine;
using UnityEngine.SceneManagement;
using Schnibble;
using System.Collections;
using System.Collections.Generic;

/// STARTING POINT FOR HUB
// > ACTIVATES TRICKS AUTO
public class StartPortal : AbstractCameraPoint
{
    [Header("Behaviour")]
    public bool forceSinglePlayer = false;
    public bool enable_tricks = false;
    public bool deleteAfterSpawn = false;
    public GameCamera.CAM_TYPE camera_type;
    public bool isTutorialStartPortal = false; 
    public GameObject UIHandle;  // for tricktracker

    [Header("Optionals")]
    public Transform facingPoint;
    public CinematicTrigger entryLevelCinematic;

    [Header("Debug")]
    public bool bypassCinematic = true;

    // Start is called before the first frame updatezd
    void Start()
    {
        PlayerController pc = Access.Player();

        init(pc);

        if (enable_tricks)
        activateTricks();

        if (forceSinglePlayer)
        {
            pc.inputMgr = Access.PlayerInputsManager().player1;
        }

        if (!bypassCinematic)
        {
            if (entryLevelCinematic!=null)
            {
                relocatePlayer(pc);
                entryLevelCinematic.StartCinematic();
                StartCoroutine(waitEntryLevelCinematic(pc));
            }
        }

    }

    void init(PlayerController pc)
    {
        pc.Freeze();

        relocatePlayer(pc);
        if (camera_type != GameCamera.CAM_TYPE.UNDEFINED)
        Access.CameraManager()?.changeCamera(camera_type, false);

        var states = pc.vehicleStates;
        states.SetState(states.states[(int)PlayerVehicleStates.States.Car]);
        
        if (deleteAfterSpawn)
        {
            Destroy(gameObject);
        }

        if (isTutorialStartPortal)
        {
            initTutorial();
        }

        pc.UnFreeze();
    }

    void initTutorial()
    {
        CheckPointManager cpm = Access.CheckPointManager();
        TrackManager tm = Access.TrackManager();
        if (!!cpm && !!tm)
        {
            tm.track_score.selected_diff = DIFFICULTIES.NONE;
            tm.launchTrack(Constants.SN_INTRO);
            cpm.init();
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator waitEntryLevelCinematic(PlayerController iPC)
    {
        iPC.Freeze();
        while(!entryLevelCinematic.cinematicDone)
        {
            yield return new WaitForSeconds(0.1f);
        }
        iPC.UnFreeze();
        init(iPC);
    }

    public void relocatePlayer(PlayerController pc)
    {
        PlayerController pc = Access.Player();
        pc.GetTransform().position = transform.position;
        pc.GetTransform().rotation = Quaternion.identity;
        if (facingPoint != null)
        {
            pc.GetTransform().LookAt(facingPoint.transform);
        }

        if (pc.GetRigidbody())
        {
            pc.GetRigidbody().velocity = Vector3.zero;
            pc.GetRigidbody().angularVelocity = Vector3.zero;
        }
    }

    public void relocatePlayer()
    {
        relocatePlayer(Access.Player());
    }

    // needed in intro as there is no startline, also for the hub, maybe?
    private void activateTricks()
    {
        TrickTracker tt = Access.Player().gameObject.GetComponent<TrickTracker>();
        if (!!tt)
        {
            tt.activate_tricks = true; // activate default in hub
            tt.init(UIHandle);
        }
    }

}
