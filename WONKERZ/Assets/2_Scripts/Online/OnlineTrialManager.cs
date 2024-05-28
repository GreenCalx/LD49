using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mirror;

public class OnlineTrialManager : NetworkBehaviour
{
    [Header("Mands")]
    public List<OnlineStartPortal> startPositions;
    public OnlineStartLine onlineStartLine;
    [Header("Internals")]
    public bool trialIsOver = false;

    // Start is called before the first frame update
    void Start()
    {
        NetworkRoomManagerExt.singleton.onlineGameManager.trialManager = this;
        DispatchPlayersToPortals();
    
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected void DispatchPlayersToPortals()
    {
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
