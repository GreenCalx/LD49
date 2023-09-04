using UnityEngine;
using Schnibble;

public class WheeledPowerTrap : TrapBundle
{
    public readonly string AnimIsWheeling = "isWheeling";

    public bool isWheeling = false;

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

    // Update is called once per frame
    void FixedUpdate()
    {
        if ((startTimeOffset >= trapStartElapsedTime) && (startTimeOffset >= 0.1f))
        { trapStartElapsedTime += Time.fixedDeltaTime; return; }

        if (isAlwaysInCharge)
        {
            if (!isWheeling)
            {
                isWheeling = true;
                updateAnimations();
            }
            linkedTrap.OnCharge();
            return;
        }

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
        }
        else if (trapRestElapsedTime >= restTimeBetweenActivations)
        {
            // loading trap
            isWheeling = true;
            updateAnimations();

            linkedTrap.OnCharge(getTrapLoadStatus());
            trapRestElapsedTime = 0f;
        }
        else if (isWheeling)
        {
            // trap is in rest mode
            linkedTrap.OnCharge(getTrapLoadStatus());
        }
        else
        {
            linkedTrap.OnRest(getTrapRestStatus());
        }
    }

    private void updateAnimations()
    {
        worker.changeAnimatorBoolParm(AnimIsWheeling, isWheeling);
        workstation.changeAnimatorBoolParm(AnimIsWheeling, isWheeling);
    }
}
