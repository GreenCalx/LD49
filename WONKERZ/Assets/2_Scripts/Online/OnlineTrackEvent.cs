using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Wonkerz;
using Mirror;


public enum TRACKEVENTS
{
    LOWGRAVITY = 0,
    TSUNAMI = 1
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
    public GravityTrackEvent()
    {
        trackEventType = TRACKEVENTS.LOWGRAVITY;
        name = "Low Gravity !";
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

public class TsunamiTrackEvent : OnlineTrackEvent
{
    public TsunamiTrackEvent()
    {
        trackEventType = TRACKEVENTS.TSUNAMI;
        name = "Tsunami";
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