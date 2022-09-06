using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Trap : MonoBehaviour
{
    public enum TRAPSTATE {
        ONCOOLDOWN, LOADING, TRIGGERED
    }
    public abstract void OnTrigger();
    /*"*/
    public abstract void OnRest(float iCooldownPercent=1f); // opt parm
    /*'*/
    public abstract void OnCharge(float iLoadPercent=1f);   // opt parm
}