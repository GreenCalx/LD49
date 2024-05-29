using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarWeightIndicator : MonoBehaviour
{
    public Rigidbody body;
    public GameObject indicator;
    public Vector3 offsetLocal;
    public bool useRotation = false;
    // Update is called once per frame
    void Update()
    {
        var com = body.worldCenterOfMass;
        indicator.transform.position = com + body.transform.TransformVector(offsetLocal);
        if (useRotation)
        {
            // try to add a lil bit of rotation
            // TODO: make it real! Do not assume COM was at 0, Vector3.up is the up, etc...
            var diff = body.transform.TransformVector(body.centerOfMass + offsetLocal).normalized;
            indicator.transform.rotation = Quaternion.LookRotation(body.transform.forward, diff);
        }
    }
}
