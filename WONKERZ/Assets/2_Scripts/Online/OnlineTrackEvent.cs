using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Wonkerz;
using Mirror;


public enum TRACKEVENTS
{
    LOWGRAVITY = 0,
    HIGHTIDE = 1
}

/////////////////////
/// Track Event Definition
public class OnlineTrackEvent
{
    public TRACKEVENTS trackEventType;
    public string name;
    public float duration;
    
    public virtual void EffectOn() {}
    public virtual void EffectOff() {}

    protected void DestroySelf()
    {
        //NetworkServer.Destroy(gameObject);
    }
}

/////////////////////
/// Track Events
///

public class GravityTrackEvent : OnlineTrackEvent
{
    private Vector3 initGravity = Vector3.zero;
    public GravityTrackEvent()
    {
        trackEventType = TRACKEVENTS.LOWGRAVITY;
        name = "Low Gravity !";
        duration = 10f;
        initGravity = Physics.gravity;
    }

    public override void EffectOn()
    {
        RpcGravityChange( initGravity / 2f );
    }

    [ClientRpc]
    public void RpcGravityChange(Vector3 iNewGravity)
    {
        Physics.gravity = iNewGravity;
    }

    public override void EffectOff()
    {
        RpcGravityChange( initGravity );
        DestroySelf();
    }
}

public class HighTideTrackEvent : OnlineTrackEvent
{
    private Vector3 initWaterPosition = Vector3.zero;
    public HighTideTrackEvent()
    {
        trackEventType = TRACKEVENTS.HIGHTIDE;
        name = "High Tide!";
        duration = 10f;
    }
    public override void EffectOn()
    {

    }

    public override void EffectOff()
    {
        DestroySelf();
    }
}