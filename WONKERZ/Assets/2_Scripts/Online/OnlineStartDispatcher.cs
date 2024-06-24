using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Wonkerz;
using Mirror;
public class OnlineStartDispatcher : NetworkBehaviour
{
    public List<OnlineStartPortal> startPositions;

    // Start is called before the first frame update
    void Start()
    {
        if (isServer)
            DispatchPlayersToPortals();
    }

    [Server]
    protected void DispatchPlayersToPortals()
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

        foreach( OnlinePlayerController opc in NetworkRoomManagerExt.singleton.onlineGameManager.uniquePlayers)
        {
            foreach(OnlineStartPortal sp in startPositions)
            {
                if (sp.attachedPlayer==null)
                {
                    sp.attachedPlayer = opc;
                    break;
                }
            }
        }
    }
}
