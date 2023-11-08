using UnityEngine;

public abstract class Trap : MonoBehaviour
{
    public enum TRAPSTATE
    {
        ONCOOLDOWN, LOADING, TRIGGERED
    }
    
    public TRAPSTATE status;

    public abstract void OnTrigger(float iCooldownPercent = 1f);
    /*"*/
    public abstract void OnRest(float iCooldownPercent = 1f); // opt parm
    /*'*/
    public abstract void OnCharge(float iLoadPercent = 1f);   // opt parm
}
