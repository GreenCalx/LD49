using UnityEngine;


public class StandaloneTrap : TrapBundle
{
    void Start()
    {
        initTimers();
    }

    void Update()
    {
        if ((startTimeOffset >= trapStartElapsedTime) && (startTimeOffset >= 0.1f))
        { trapStartElapsedTime += Time.deltaTime; return; }

        if (isAlwaysInCharge)
        {
            linkedTrap.OnCharge();
            return;
        }

        updateStatus();

        if (linkedTrap.status == Trap.TRAPSTATE.TRIGGERED)
        {
            trapTriggeredElapsedTime += Time.deltaTime;
            linkedTrap.OnTrigger(getTrapTriggeredStatus());
            trapLoadElapsedTime = 0f;
            
        }
        else if (linkedTrap.status == Trap.TRAPSTATE.LOADING)
        {
            trapLoadElapsedTime += Time.deltaTime;
            linkedTrap.OnCharge(getTrapLoadStatus());
            trapRestElapsedTime = 0f;
            
        } else if (linkedTrap.status == Trap.TRAPSTATE.ONCOOLDOWN) 
        {
            trapRestElapsedTime += Time.deltaTime;
            linkedTrap.OnRest(getTrapRestStatus());
            trapTriggeredElapsedTime = 0f;
        }

    }


}