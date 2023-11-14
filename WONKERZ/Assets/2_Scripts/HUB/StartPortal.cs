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

    [Header("Optionals")]
    public Transform facingPoint;
    public CinematicTrigger entryLevelCinematic;
    public GameObject UITransitionIN;
    public bool doTransitionIN;

    [Header("Debug")]
    public bool bypassCinematic = true;

    // Start is called before the first frame updatezd
    void Start()
    {
        init();

        if (forceSinglePlayer)
        {
            Access.Player().inputMgr = Access.PlayerInputsManager().player1;
        }

        if (!bypassCinematic)
        {
            if (entryLevelCinematic!=null)
            {
                relocatePlayer();
                StartCoroutine(waitEntryLevelCinematic(Access.Player()));
            }
        }

        if (isTutorialStartPortal)
        {
            initTutorial();
        }

    }

    void init()
    {
        if (doTransitionIN)
            Instantiate(UITransitionIN);

        relocatePlayer();
        if (camera_type != GameCamera.CAM_TYPE.UNDEFINED)
            Access.CameraManager().changeCamera(camera_type, false);

        var states = Access.Player().vehicleStates;
        states.SetState(states.states[(int)PlayerVehicleStates.States.Car]);
        Access.Player().UnFreeze();

        if (deleteAfterSpawn)
        {
            Destroy(gameObject);
        }
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
        init();
    }

    public void relocatePlayer()
    {
        PlayerController pc = Access.Player();
        pc.transform.position = transform.position;
        pc.transform.rotation = Quaternion.identity;
        if (facingPoint != null)
        {
            pc.transform.LookAt(facingPoint.transform);
        }
        pc.rb.velocity = Vector3.zero;
        pc.rb.angularVelocity = Vector3.zero;
    }

}