using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheeledPowerTrap : MonoBehaviour
{
    [Header("MANDATORY")]
    public TrapWorker       worker;
    public TrapWorkstation  workstation;     
    public Trap             linkedTrap;
    
    [Header("ANIM PARM")]
    public readonly string AnimIsWheeling = "isWheeling";
    public bool isWheeling = false;

    private float trapLoadElapsedTime;
    private float trapRestElapsedTime;


    // Start is called before the first frame update
    void Start()
    {
        if ((worker==null)||(workstation==null))
        {
            Debug.LogWarning("Missing worker/workstation on " + gameObject.name);
            gameObject.SetActive(false);
        }
        trapLoadElapsedTime = 0f;
        trapRestElapsedTime = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (isWheeling)
            trapLoadElapsedTime += Time.deltaTime;
        else
            trapRestElapsedTime += Time.deltaTime;

        if (trapLoadElapsedTime >= worker.timeToLoadTrap)
        {
            // trigger trap
            isWheeling = false;
            updateAnimations();

            linkedTrap.OnTrigger();
            trapLoadElapsedTime = 0f;
        } else if ( trapRestElapsedTime >= worker.restTimeBetweenActivations )
        {
            // loading trap
            isWheeling = true;
            updateAnimations();
            
            trapRestElapsedTime = 0f;
        }

        // For test

    }

    private void updateAnimations()
    {
        worker.changeAnimatorBoolParm(AnimIsWheeling, isWheeling);
        workstation.changeAnimatorBoolParm(AnimIsWheeling, isWheeling);
    }
}
