using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Wonkerz;
using Mirror;


public enum TRACKEVENTS
{
    LOWGRAVITY = 0,
    HIGHTIDE = 1,
    NIGHTTIME = 2
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
        duration = 30f;
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
    public float tideChangeTime = 10f;
    public float tideChangeYAmount = 80f;
    public HighTideTrackEvent()
    {
        trackEventType = TRACKEVENTS.HIGHTIDE;
        name = "High Tide!";
        duration = 50f;
    }
    public override void EffectOn()
    {
        OpenCourseMutator ocm = Access.OCMutator();
        if (ocm==null)
            return;
        ocm.RiseSeaLevel(true, tideChangeTime, tideChangeYAmount);
    }

    public override void EffectOff()
    {
        OpenCourseMutator ocm = Access.OCMutator();
        if (ocm==null)
            return;
        ocm.RiseSeaLevel(false, tideChangeTime, tideChangeYAmount);
        DestroySelf();
    }
}

public class NightTimeTrackEvent : OnlineTrackEvent
{

    public NightTimeTrackEvent()
    {
        trackEventType = TRACKEVENTS.NIGHTTIME;
        name = "Night Time";
        duration = 50f;
    }
    public override void EffectOn()
    {
        OpenCourseMutator ocm = Access.OCMutator();
        if (ocm==null)
            return;
        ocm.NightTime();
    }

    public override void EffectOff()
    {
        OpenCourseMutator ocm = Access.OCMutator();
        if (ocm==null)
        {
            DestroySelf();
            return;
        }
            
        ocm.DayTime();
        DestroySelf();
    }
}
