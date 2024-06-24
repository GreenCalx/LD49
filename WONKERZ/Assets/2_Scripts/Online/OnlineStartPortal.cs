using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using Mirror;
using Wonkerz;

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
        if (isClient)
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
        if (!attachedPlayer.isLocalPlayer)
        {
            yield break;
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

    IEnumerator RelocatePlayer()
    {
        if (isServerOnly)
            yield break;

        if (!attachedPlayer.isLocalPlayer)
            yield break;

        while (attachedPlayer==null)
        {
            yield return null;
        }

         if (isClientOnly)
         {
            //attachedPlayer.RpcRelocate(transform.position, facingPoint);
            attachedPlayer.Relocate(transform.position, facingPoint);
         } else if (isClient && isServer) {
            attachedPlayer.Relocate(transform.position, facingPoint);
         }

    }

}
