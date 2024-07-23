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
    public  GameCamera.CAM_TYPE camera_type;
    public  bool                deleteAfterSpawn;

    public  int                 seed;

    #if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        // Draws a 5 unit long red line in front of the object
        Gizmos.color = Color.blue;
        Vector3 direction = transform.TransformDirection(Vector3.forward) * 10;
        Gizmos.DrawRay(transform.position, direction);

        Gizmos.DrawCube(transform.position, new Vector3(4,6,5));
    }
#endif

    [Server]
    public void AttachPlayer(OnlinePlayerController player) {
        attachedPlayer = player;
        if (attachedPlayer != null) {
            attachedPlayer.Relocate(transform.position, transform.rotation);
        }
    }
}
