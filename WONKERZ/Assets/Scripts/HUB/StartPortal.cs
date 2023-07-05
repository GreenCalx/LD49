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
    public bool enable_tricks = false;
    public bool deleteAfterSpawn = false;
    public GameCamera.CAM_TYPE camera_type;

    [Header("Optionals")]
    public Transform facingPoint;
    public CinematicTrigger entryLevelCinematic;

    [Header("Debug")]
    public bool bypassCinematic = true;

    private GameObject playerRef;

    // Start is called before the first frame updatezd
    void Start()
    {
        if (!bypassCinematic)
        {
            if (entryLevelCinematic!=null)
            {
                StartCoroutine(waitEntryLevelCinematic(Access.Player()));
                return;
            }
        }

        init();
    }

    void init()
    {
        playerRef = Access.Player().gameObject;

        relocatePlayer();
        Access.CameraManager().changeCamera(camera_type);

        var states = Access.Player().vehicleStates;
        states.SetState(states.states[(int)PlayerVehicleStates.States.Car]);

        if (deleteAfterSpawn)
        {
            Destroy(gameObject);
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

}
