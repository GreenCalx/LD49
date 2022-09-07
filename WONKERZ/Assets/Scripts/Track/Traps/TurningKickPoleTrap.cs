using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurningKickPoleTrap : Trap
{
    public float x_turningSpeed = 1f;
    public float y_turningSpeed = 1f;
    public float z_turningSpeed = 1f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void OnTrigger()
    {
        // always triggered
    }
    
    public override void OnCharge(float iCooldownPercent)
    {
        // turning as a worker is turning the wheel
        transform.Rotate(x_turningSpeed, y_turningSpeed, z_turningSpeed, Space.Self);
    }

    public override void OnRest(float iCooldownPercent)
    {
        // never in rest

    }
}
