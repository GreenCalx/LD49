using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikedSphere : Trap
{
    public int damageOnCollide = 2;
    public bool trigger = false;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }


    void OnCollisionEnter(Collision iCol)
    {
        CarController cc = iCol.gameObject.GetComponent<CarController>();
        if (!!cc)
        {
            ContactPoint cp = iCol.contacts[0];
            cc.takeDamage(damageOnCollide, cp.point, cp.normal );
        }
    }

    public override void OnTrigger()
    {
        rb.isKinematic = false;
    }

    public override void OnRest(float iCooldownPercent=1f) {}

    public override void OnCharge(float iLoadPercent=1f) {}
}
