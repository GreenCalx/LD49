using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Wonkerz;
using Mirror;



public class OnlineStartDispatcher : NetworkBehaviour
{
    public bool dispatchOnStart = true;

    // pool of pre-allocated OnlineStartPortal
    public List<OnlineStartPortal> startPositions;

    // Start is called before the first frame update
    public override void OnStartServer()
    {
        if (dispatchOnStart) DispatchPlayersToPortals();
    }

    [Server]
    public void DispatchPlayersToPortals()
    {
        StartCoroutine(DispatchPlayersCo());
    }

    IEnumerator WaitForDependencies() { 
        while (NetworkRoomManagerExt.singleton                                         == null)  {yield return null;}
        while (NetworkRoomManagerExt.singleton.onlineGameManager                       == null)  {yield return null;}
        while (NetworkRoomManagerExt.singleton.onlineGameManager.AreAllPlayersLoaded() == false) {yield return null;}
    }

    IEnumerator DispatchPlayersCo()
    {
        yield return StartCoroutine(WaitForDependencies());

        List<OnlineStartPortal> unusedSPs = new List<OnlineStartPortal>();
        startPositions.CopyTo(unusedSPs);

        // assign player <> startposition
        foreach ( OnlinePlayerController opc in NetworkRoomManagerExt.singleton.onlineGameManager.uniquePlayers)
        {
            foreach(OnlineStartPortal sp in startPositions)
            {
                if (sp.attachedPlayer==null)
                {
                    sp.AttachPlayer(opc);
                    unusedSPs.Remove(sp);
                    break;
                }
            }
        }
        // clean unused starts
        foreach(OnlineStartPortal sp in unusedSPs) {NetworkServer.Destroy(sp.gameObject);}
    }
}
