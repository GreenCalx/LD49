using UnityEngine;
using Schnibble;

public class TrapBundle : MonoBehaviour
{

    [Header("MANDATORY")]
    public Trap linkedTrap;

    [Header("If not a standalone trap")]
    public TrapWorker worker;
    public TrapWorkstation workstation;
    

    [Header("Tweaks")]
    public float startTimeOffset = 0f;
    public float timeToLoadTrap = 1f;
    public float restTimeBetweenActivations = 1f;
    public float timeTrapIsTriggered = 1f;

    public bool isAlwaysInCharge = false;

    protected float trapLoadElapsedTime;
    protected float trapRestElapsedTime;
    protected float trapStartElapsedTime;
    protected float trapTriggeredElapsedTime;

    // Start is called before the first frame update
    void Start()
    {
        if ((worker == null) || (workstation == null) || (linkedTrap == null))
        {
            this.LogWarn("Missing worker/workstation on " + gameObject.name);
            gameObject.SetActive(false);
        }
        initTimers();
    }

    protected void initTimers()
    {
        trapLoadElapsedTime = 0f;
        trapRestElapsedTime = 0f;
        trapStartElapsedTime = 0f;
        trapTriggeredElapsedTime = 0f;
    }

    // Range 0f(starting to load) to 1f(trigger)
    public float getTrapLoadStatus()
    {
        if (trapLoadElapsedTime == 0f)
            return 0f;
        return trapLoadElapsedTime / timeToLoadTrap;
    }
    // Range 0f(trigger is over) to 1f(starting to load)
    public float getTrapRestStatus()
    {
        if (trapRestElapsedTime == 0f)
            return 0f;
        return trapRestElapsedTime / restTimeBetweenActivations;
    }

    public float getTrapTriggeredStatus()
    {
        if (trapTriggeredElapsedTime == 0f)
            return 0f;
        return trapTriggeredElapsedTime / timeTrapIsTriggered;
    }

    protected void updateStatus()
    {
        bool rest_done = (trapRestElapsedTime >= restTimeBetweenActivations);
        bool load_done = (trapLoadElapsedTime >= timeToLoadTrap);
        bool trigg_done = (trapTriggeredElapsedTime >= timeTrapIsTriggered);

        if (rest_done)
        {
            linkedTrap.status = Trap.TRAPSTATE.LOADING;
        } else if (trigg_done)
        {
            linkedTrap.status = Trap.TRAPSTATE.ONCOOLDOWN;
        } else if (load_done)
        {
            linkedTrap.status = Trap.TRAPSTATE.TRIGGERED;
        }
        
    }

}
