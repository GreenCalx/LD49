using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OnlineStartPortal : MonoBehaviour
{
    public List<GameObject> broadcastPlayerTo;

    [Header("Internals Exposed")]
    public PlayerController attachedPlayer;
    [Header("Tweaks")]
    public GameCamera.CAM_TYPE camera_type;
    public bool deleteAfterSpawn;
    public Transform facingPoint;
    private bool playerIsAttached = false;


    // Start is called before the first frame update
    void Start()
    {
        // TODO : All portals triggers this shit thus linking to same player multiple times
        // > need to only care about its "local spawn"
        //  use a trigger again instead of this search ?
        StartCoroutine(WaitPlayerForInit());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // void OnTriggerEnter(Collider iCollider)
    // {
    //     if (Utils.colliderIsPlayer(iCollider))
    //     {
    //         attachedPlayer = iCollider.GetComponent<PlayerController>();

    //         init();
    //         playerIsAttached = true;

    //         if (deleteAfterSpawn)
    //         {
    //             Destroy(gameObject);
    //         }
    //     }
    // }

    IEnumerator WaitPlayerForInit()
    {
        while (attachedPlayer==null)
        {
            attachedPlayer = Access.Player();
            yield return null;
        }
        init();
        playerIsAttached  = true;
        if (deleteAfterSpawn)
        {
            Destroy(gameObject);
        }
    }

    void init()
    {
        attachedPlayer.Freeze();

        //relocatePlayer();
        
        
        // Camera
        if (camera_type != GameCamera.CAM_TYPE.UNDEFINED)
            Access.CameraManager()?.changeCamera(camera_type, false);

        var states = attachedPlayer.vehicleStates;
        states.SetState(states.states[(int)PlayerVehicleStates.States.Car]);
        
        if (deleteAfterSpawn)
        {
            Destroy(gameObject);
        }

        attachedPlayer.UnFreeze();
    }

    public void relocatePlayer()
    {
        attachedPlayer.transform.position = transform.position;
        attachedPlayer.transform.rotation = Quaternion.identity;
        if (facingPoint != null)
        {
            attachedPlayer.transform.LookAt(facingPoint.transform);
        }
        attachedPlayer.rb.velocity = Vector3.zero;
        attachedPlayer.rb.angularVelocity = Vector3.zero;
    }

}
