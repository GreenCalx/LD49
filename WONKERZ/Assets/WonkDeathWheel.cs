using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WonkDeathWheel : MonoBehaviour
{
    public GameObject COM;
    public Rigidbody rb;
    public Transform wheel;
    public Vector3 hingePoint;
    public float counterMass;
    // Start is called before the first frame update
    void Start()
    {
        hingePoint = gameObject.transform.position;
        rb.centerOfMass = rb.transform.InverseTransformPoint(COM.transform.position);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        rb.AddForceAtPosition(Physics.gravity, wheel.transform.position);
            // mirror
        Vector3 dir = (wheel.position - hingePoint);
        rb.AddForceAtPosition( (rb.mass + counterMass) * Physics.gravity, hingePoint - dir);


        Debug.DrawRay(wheel.transform.position, Physics.gravity);
        Debug.DrawRay(hingePoint-dir, Physics.gravity);
    }
}
