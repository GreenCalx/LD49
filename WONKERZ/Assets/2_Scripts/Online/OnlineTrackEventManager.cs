using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using Wonkerz;
using Mirror;

public class OnlineTrackEventManager : NetworkBehaviour
{
   public OnlineTrackEvent activeEvent;
   private Coroutine trackEventCo;
    public float nextEventTime = 0f;
   
   void Start()
   {
        RefreshUI();
   }

    public void SpawnEvent()
    {
        if (trackEventCo!=null)
        {
            StopCoroutine(trackEventCo);
            trackEventCo = null;
        }

        int n_eventTypes = Enum.GetNames(typeof(TRACKEVENTS)).Length;
        int selected = UnityEngine.Random.Range(0,n_eventTypes-1);

        activeEvent = GetEvent(selected);
        RefreshUI();

        activeEvent.EffectOn();
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

        activeEvent.EffectOff();
        activeEvent = null;

        RefreshUI();
    }

    public OnlineTrackEvent GetEvent(int iEventTypeIndex)
    {
        OnlineTrackEvent ev = null;
        switch(iEventTypeIndex)
        {
            case (int)TRACKEVENTS.LOWGRAVITY:
                ev = new GravityTrackEvent();
                break;
            case (int)TRACKEVENTS.TSUNAMI:
                ev = new TsunamiTrackEvent();
                break;
            default:
                break;
        }
        return ev;
    }

    private void RefreshUI()
    {
        UIPlayerOnline pui = Access.UIPlayerOnline();
        if (pui==null)
            return;

        if (activeEvent!=null)
        {
            // show
            pui.TrackEventHandle.gameObject.SetActive(true);
            pui.TrackEventNameTxt.text = activeEvent.name;
        }
        else {
            // hide
            pui.TrackEventNameTxt.text = "";
            pui.TrackEventHandle.gameObject.SetActive(false);
        }
    }
}