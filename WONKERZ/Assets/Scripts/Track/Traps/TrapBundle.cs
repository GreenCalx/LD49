using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapBundle : MonoBehaviour
{
    [Header("MANDATORY")]
    public TrapWorker       worker;
    public TrapWorkstation  workstation;     
    public Trap             linkedTrap;

    [Header("Tweaks")]
    public float startTimeOffset            = 0f;
    public float timeToLoadTrap             = 1f;
    public float restTimeBetweenActivations = 1f;

    protected float trapLoadElapsedTime;
    protected float trapRestElapsedTime;
    protected float trapStartElapsedTime;

    // Start is called before the first frame update
    void Start()
    {
        if ((worker==null)||(workstation==null)||(linkedTrap==null))
        {
            Debug.LogWarning("Missing worker/workstation on " + gameObject.name);
            gameObject.SetActive(false);
        }
        initTimers();
    }

    protected void initTimers()
    {
        trapLoadElapsedTime = 0f;
        trapRestElapsedTime = 0f;
        trapStartElapsedTime = 0f;
    }

    // Range 0f(starting to load) to 1f(trigger)
    public float getTrapLoadStatus()
    {
        if (trapLoadElapsedTime==0f)
            return 0f;
        return trapLoadElapsedTime / timeToLoadTrap;
    }
    // Range 0f(trigger is over) to 1f(starting to load)
    public float getTrapRestStatus()
    {
        if (trapRestElapsedTime==0f)
            return 0f;
        return trapRestElapsedTime / restTimeBetweenActivations;
    }

}
