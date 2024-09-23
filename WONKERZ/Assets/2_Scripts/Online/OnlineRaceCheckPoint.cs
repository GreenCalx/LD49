using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Wonkerz;
using Mirror;

public class OnlineRaceCheckPoint : NetworkBehaviour
{
    private readonly string GO_Prefix = "CP_";
    [Header("Auto Refs")]
    public OnlineRaceTrialManager ORTM;
    [Header("Manual Refs")]
    public int id = 0; // lap done
    public List<OnlineRaceCheckPoint> prev_CPs;
    public List<OnlineRaceCheckPoint> next_CPs;

    // Start is called before the first frame update
    void Start()
    {
        gameObject.name = GO_Prefix + id.ToString();
    }

    void OnTriggerEnter(Collider iCollider)
    {
        OnlinePlayerController opc = iCollider.gameObject.GetComponentInParent<OnlinePlayerController>();
        if (opc!=null)
        {
            NotifyORTM(opc);
        }
    }

    [Server]
    private void NotifyORTM(OnlinePlayerController iOPC)
    {
        ORTM.NotifyPlayerPassedCP(iOPC, this);
    }

    // If multiple path available, gotta do an api like GetClosestSuccessor..
    public OnlineRaceCheckPoint GetFirstSuccessor()
    {
        return next_CPs[0];
    }

    public bool IsPredecessorOf(OnlineRaceCheckPoint iOtherCP)
    {
        foreach(OnlineRaceCheckPoint orcp in iOtherCP.prev_CPs)
        {
            if (orcp.id == id)
            {
                return true;
            }
        }
        return false;
    }
}
