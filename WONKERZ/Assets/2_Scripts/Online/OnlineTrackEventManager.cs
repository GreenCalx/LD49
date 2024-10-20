using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using Wonkerz;
using Mirror;

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
        int selected = UnityEngine.Random.Range(0,n_eventTypes);

        activeEvent = GetEvent(selected);
        if (activeEvent.duration > OnlineGameManager.singleton.gameTime)
        {
            //this.Log("Not enough time to launch a new event.");
            return;
        }

        RpcEventEffectOn();
        RpcRefreshUI(activeEvent);
//        activeEvent.EffectOn();

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
        RpcEventEffectOff();
        
        
        RpcRefreshUI(null);
    }

    public OnlineTrackEvent GetEvent(int iEventTypeIndex)
    {
        OnlineTrackEvent ev = null;
        switch(iEventTypeIndex)
        {
            case (int)TRACKEVENTS.LOWGRAVITY:
                ev = new GravityTrackEvent();
                break;
            case (int)TRACKEVENTS.HIGHTIDE:
                ev = new HighTideTrackEvent();
                break;
            case (int)TRACKEVENTS.NIGHTTIME:
                ev = new NightTimeTrackEvent();
                break;
            default:
                break;
        }
        return ev;
    }

    [ClientRpc]
    private void RpcEventEffectOn()
    {
        activeEvent.EffectOn();
    }

    [ClientRpc]
    private void RpcEventEffectOff()
    {
        activeEvent.EffectOff();
        activeEvent = null;
    }

    [ClientRpc]
    private void RpcRefreshUI(OnlineTrackEvent iEvent)
    {
        var pui = OnlineGameManager.singleton.UIPlayer;
        if (pui==null)
        return;

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
