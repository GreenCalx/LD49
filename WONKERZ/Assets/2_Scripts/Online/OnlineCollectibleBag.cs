using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mirror;

public class OnlineCollectibleBag : NetworkBehaviour
{
    public OnlinePlayerController owner;
    [SyncVar]
    public int nuts;

    // Start is called before the first frame update
    void Start()
    {
        owner = Access.OfflineGameManager().localPlayer;
        nuts = 0;

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AsServerCollect(OnlineCollectible iCollectible)
    {
        switch (iCollectible.collectibleType)
        {
            case ONLINE_COLLECTIBLES.NUTS:
                nuts += iCollectible.value;
                break;
            default:
                break;
        }
    }

    [Command]
    public void CmdCollect(OnlineCollectible iCollectible)
    {
        switch (iCollectible.collectibleType)
        {
            case ONLINE_COLLECTIBLES.NUTS:
                nuts += iCollectible.value;
                break;
            default:
                break;
        }
    }

}
