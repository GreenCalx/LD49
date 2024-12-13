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
    public  bool                keepPlayerOnLocationUntilGameStart;

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
        if (keepPlayerOnLocationUntilGameStart)
            StartCoroutine(KeepPlayerOnLocation());
    }

    IEnumerator KeepPlayerOnLocation()
    {
        while(NetworkRoomManagerExt.singleton.onlineGameManager==null)
        { 
            yield return null;
        }

        if (NetworkRoomManagerExt.singleton.onlineGameManager.IsInOpenCourse())
        {
            while(!NetworkRoomManagerExt.singleton.onlineGameManager.gameLaunched)
            { 
                attachedPlayer.Relocate(transform.position, transform.rotation);
                yield return null; 
            }
        }
        else if (NetworkRoomManagerExt.singleton.onlineGameManager.IsInTrial())
        { 
            while (NetworkRoomManagerExt.singleton.onlineGameManager.trialManager == null)  { yield return null;}
            while(!NetworkRoomManagerExt.singleton.onlineGameManager.trialManager.trialLaunched)
            { 
                attachedPlayer.Relocate(transform.position, transform.rotation);
                yield return null; 
            }            
        }
         
        NetworkServer.Destroy(gameObject);
    }
}
