using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using Mirror;

public class OnlineStartPortal : NetworkBehaviour
{
    [Header("Internals Exposed")]
    [SyncVar]
    public OnlinePlayerController attachedPlayer;
    [Header("Tweaks")]
    public GameCamera.CAM_TYPE camera_type;
    public bool deleteAfterSpawn;
    public Transform facingPoint;
    private bool playerIsAttached = false;
    public int seed;


    void Start()
    {
        StartCoroutine(WaitPlayerForInit());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator WaitPlayerForInit()
    {
        while (attachedPlayer==null)
        {
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
        //attachedPlayer.self_PlayerController.Freeze();

        if (isServer)
            StartCoroutine(RelocatePlayer());
        
        // // Camera
        if (camera_type != GameCamera.CAM_TYPE.UNDEFINED)
             Access.CameraManager()?.changeCamera(camera_type, false);
        
        if (deleteAfterSpawn)
        {
            Destroy(gameObject);
        }

        //attachedPlayer.self_PlayerController.UnFreeze();
    }

    [ServerCallback]
    IEnumerator RelocatePlayer()
    {
        while (attachedPlayer==null)
        {
            yield return null;
        }

        attachedPlayer.self_PlayerController.UnFreeze();

        attachedPlayer.transform.position = transform.position;
        attachedPlayer.transform.rotation = Quaternion.identity;
        if (facingPoint != null)
        {
            attachedPlayer.transform.LookAt(facingPoint.transform);
        }
        attachedPlayer.self_PlayerController.rb.velocity = Vector3.zero;
        attachedPlayer.self_PlayerController.rb.angularVelocity = Vector3.zero;

        attachedPlayer.self_PlayerController.Freeze();
    }

}
