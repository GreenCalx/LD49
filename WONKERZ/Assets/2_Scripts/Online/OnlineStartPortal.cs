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
    //public Transform facingPoint;
    private bool playerIsAttached = false;
    public int seed;


    void OnDrawGizmosSelected()
    {
        // Draws a 5 unit long red line in front of the object
        Gizmos.color = Color.blue;
        Vector3 direction = transform.TransformDirection(Vector3.forward) * 5;
        Gizmos.DrawRay(transform.position, direction);
    }

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
        StartCoroutine(RelocatePlayer());
        
        // // Camera
        if (camera_type != GameCamera.CAM_TYPE.UNDEFINED)
             Access.CameraManager()?.changeCamera(camera_type, false);

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

         while (Vector3.Distance(transform.position, attachedPlayer.transform.position) > 1f)
         {
            if (isClientOnly)
            {
                attachedPlayer.Relocate(transform.position, transform.rotation);
            } else if (isClient && isServer) {
                attachedPlayer.CmdRelocate(transform.position, transform.rotation);
            }
            yield return null;
         }

    }

}
