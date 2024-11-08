using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Wonkerz;
using Mirror;

public class OnlineDeathZone : MonoBehaviour
{

    void OnTriggerEnter(Collider iCol)
    {
        killIfPlayer(iCol);
    }

    void OnTriggerStay(Collider iCol)
    {
        killIfPlayer(iCol);
    }

    private void killIfPlayer(Collider iCol)
    {
        if (!Wonkerz.Utils.colliderIsPlayer(iCol))
        { return; }

        OnlinePlayerController as_OPC = iCol.gameObject.GetComponentInParent<OnlinePlayerController>();

        if (!as_OPC.isLocalPlayer)
        { return;}

        if (!as_OPC.IsAlive)
        { return; }

        // Set Player as DNF if in trial
        if (!!NetworkRoomManagerExt.singleton.onlineGameManager.trialManager)
        {
            if (NetworkRoomManagerExt.singleton.onlineGameManager.trialManager.trialLaunched)
                NetworkRoomManagerExt.singleton.onlineGameManager.trialManager.NotifyPlayerIsDNF(as_OPC);
        } else  // Open course
        {
            NetworkRoomManagerExt.singleton.onlineGameManager.NotifyPlayerDeath(as_OPC);
        }

        // explode player
        as_OPC.self_PlayerController.Kill();
        //NetworkServer.Destroy(as_OPC);
        //as_OPC.gameObject.SetActive(false);

    }

}
