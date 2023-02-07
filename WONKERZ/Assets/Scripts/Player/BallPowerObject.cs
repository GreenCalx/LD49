using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallPowerObject : MonoBehaviour
{
    public Rigidbody rb;
    private Vector3 prevVel;
    private Vector3 prevAngVel;
    private Vector3 prevPos;
    
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        prevVel     = rb.velocity;
        prevAngVel  = rb.angularVelocity;
        prevPos     = transform.position;
    }

    public void breakableObjectCollisionCorrection()
    {
        rb.velocity         = prevVel;
        rb.angularVelocity  = prevAngVel;
        transform.position  = prevPos + rb.velocity * Time.deltaTime;
    }

    void OnCollisionStay(Collision iCol)
    {
        Ground gnd = iCol.collider.GetComponent<Ground>();
        if (!!gnd)
        {
            if (gnd.GI.Type == Ground.EType.WATER)
            {
                PowerController PC = Access.Player().gameObject.GetComponent<PowerController>();
                PC.setNextPower(0);
                PC.tryTriggerPower();
            }
        }
    }
}
