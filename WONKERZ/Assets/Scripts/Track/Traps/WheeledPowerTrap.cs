using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheeledPowerTrap : TrapBundle
{   
    [Header("ANIM PARM")]
    public readonly string AnimIsWheeling = "isWheeling";
    public bool isWheeling = false;

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

    // Update is called once per frame
    void Update()
    {
        if ((startTimeOffset >= trapStartElapsedTime)&&(startTimeOffset>=0.1f))
        { trapStartElapsedTime+=Time.deltaTime; return; }

        if (isWheeling)
            trapLoadElapsedTime += Time.deltaTime;
        else
            trapRestElapsedTime += Time.deltaTime;

        if (trapLoadElapsedTime >= timeToLoadTrap)
        {
            // trigger trap
            isWheeling = false;
            updateAnimations();

            linkedTrap.OnTrigger();
            trapLoadElapsedTime = 0f;
        } else if ( trapRestElapsedTime >= restTimeBetweenActivations )
        {
            // loading trap
            isWheeling = true;
            updateAnimations();

            linkedTrap.OnCharge(getTrapLoadStatus());
            trapRestElapsedTime = 0f;
        } else if (isWheeling) {
            // trap is in rest mode
            linkedTrap.OnCharge(getTrapLoadStatus());
        } else {
            linkedTrap.OnRest(getTrapRestStatus());
        }
    }

    private void updateAnimations()
    {
        worker.changeAnimatorBoolParm(AnimIsWheeling, isWheeling);
        workstation.changeAnimatorBoolParm(AnimIsWheeling, isWheeling);
    }
}
