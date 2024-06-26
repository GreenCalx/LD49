using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Wonkerz;
using Mirror;
public class OnlineStartDispatcher : NetworkBehaviour
{
    public bool dispatchOnStart = true;
    public List<OnlineStartPortal> startPositions;

    // Start is called before the first frame update
    void Start()
    {
        if (isServer && dispatchOnStart)
            DispatchPlayersToPortals();
    }

    [Server]
    public void DispatchPlayersToPortals()
    {
        StartCoroutine(DispatchPlayersCo());
    }

    IEnumerator DispatchPlayersCo()
    {
        while (NetworkRoomManagerExt.singleton.onlineGameManager == null)
        {
            yield return null;
        }
        while (!NetworkRoomManagerExt.singleton.onlineGameManager.AllPlayersLoaded())
        {
            yield return null;
        }

        List<OnlineStartPortal> unusedSPs = new List<OnlineStartPortal>();
        startPositions.CopyTo(unusedSPs);

        foreach ( OnlinePlayerController opc in NetworkRoomManagerExt.singleton.onlineGameManager.uniquePlayers)
        {
            foreach(OnlineStartPortal sp in startPositions)
            {
                if (sp.attachedPlayer==null)
                {
                    sp.attachedPlayer = opc;
                    unusedSPs.Remove(sp);
                    break;
                }
            }
        }
        // clean unused starts
        foreach(OnlineStartPortal sp in unusedSPs)
        {
            NetworkServer.Destroy(sp.gameObject);
        }

    }
}
