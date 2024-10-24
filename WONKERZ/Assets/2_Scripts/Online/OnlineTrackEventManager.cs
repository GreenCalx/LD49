using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using Schnibble;

using Wonkerz;
using Mirror;

// NOTE:
// For now we consider that we can have only one Event by EventType.
// This assumption might not be true in the future.
// Also client and server does not have the same event objects for now, only start/end is streamed by EventType.

public class OnlineTrackEventManager : NetworkBehaviour
{
    [SyncVar]
    public OnlineTrackEvent activeEvent;
    private Coroutine trackEventCo;
    public float nextEventTime = 0f;

    [Server]
    public void SpawnEvent()
    {
        if (trackEventCo!=null)
        {
            StopCoroutine(trackEventCo);
            trackEventCo = null;
        }

        int n_eventTypes = Enum.GetNames(typeof(TRACKEVENTS)).Length;
        var selected = (TRACKEVENTS)UnityEngine.Random.Range(1,n_eventTypes);

        activeEvent = GetEvent(selected);
        if (activeEvent.duration > OnlineGameManager.singleton.gameTime)
        {
            //this.Log("Not enough time to launch a new event.");
            return;
        }

        RpcEventEffectOn(selected);
        RpcRefreshUI    (selected);

        trackEventCo = StartCoroutine(WaitForEventIsOver());
    }

    IEnumerator WaitForEventIsOver()
    {
        OnlineGameManager ogm = NetworkRoomManagerExt.singleton.onlineGameManager;
        float endTime = ogm.gameTime - activeEvent.duration;

        while (ogm.gameTime > endTime )
        {
            yield return null;
        }

        //activeEvent.EffectOff();
        RpcEventEffectOff(activeEvent.trackEventType);
        RpcRefreshUI(TRACKEVENTS.None);
    }

    public OnlineTrackEvent GetEvent(TRACKEVENTS eventType)
    {
        OnlineTrackEvent ev = null;
        switch(eventType)
        {
            case TRACKEVENTS.None:
                ev = null;
                break;
            case TRACKEVENTS.LOWGRAVITY:
                ev = new GravityTrackEvent();
                break;
            case TRACKEVENTS.HIGHTIDE:
                ev = new HighTideTrackEvent();
                break;
            case TRACKEVENTS.NIGHTTIME:
                ev = new NightTimeTrackEvent();
                break;
            default:
                break;
        }
        return ev;
    }

    [ClientRpc]
    private void RpcEventEffectOn(TRACKEVENTS eventType)
    {
        activeEvent = GetEvent(eventType);
        if (activeEvent != null) activeEvent.EffectOn();
    }

    [ClientRpc]
    private void RpcEventEffectOff(TRACKEVENTS eventType)
    {
        if (activeEvent == null) {
            this.LogError("Received effect off but none is on.");
            // For now we will still fire the eventType EventOff.
            //return;
        }

        if (activeEvent != null && activeEvent.trackEventType != eventType) {
            this.LogError("Trying to end effect that is not the current running one.");
            // For now we still shutdown the current effect.Might not be a good idea.
            activeEvent.EffectOff();
            // For now we will still fire the eventType EventOff.
            // return;
        }

        var eventObj = GetEvent(eventType);
        if (eventObj != null) eventObj.EffectOff();

        activeEvent = null;
    }

    [ClientRpc]
    private void RpcRefreshUI(TRACKEVENTS eventType)
    {
        var pui = OnlineGameManager.singleton.UIPlayer;
        if (pui==null)
        return;

        var iEvent = GetEvent(eventType);
        if (iEvent!=null)
        {
            // show
            StartCoroutine(DelayedUIShow(pui, true));
            pui.TrackEventOnFXHandle.gameObject.SetActive(true);
            pui.TrackEventOffFXHandle.gameObject.SetActive(false);
            pui.TrackEventNameTxt.text = iEvent.name;
        }
        else {

            // hide
            pui.TrackEventOffFXHandle.gameObject.SetActive(true);
            pui.TrackEventNameTxt.text = "";

            pui.TrackEventOnFXHandle.gameObject.SetActive(false);
            StartCoroutine(DelayedUIShow(pui, false));
        }
    }

    IEnumerator DelayedUIShow(UIPlayerOnline pui, bool iState)
    {
        float delay = 0.5f;
        float elapsed = 0f;
        while (elapsed < delay)
        { elapsed+=Time.deltaTime; yield return null; }

        pui.TrackEventHandle.gameObject.SetActive(iState);
    }
}
