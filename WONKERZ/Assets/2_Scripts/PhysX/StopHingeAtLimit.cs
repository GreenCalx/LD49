using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(HingeJoint))]
public class StopHingeAtLimit : MonoBehaviour
{
    private HingeJoint 		hinge;
    private Rigidbody       rb;
    public bool limitReached = false;

    public List<UnityEvent> callbacksOnLimitReached;
    // Start is called before the first frame update
    void Start()
    {
        hinge = GetComponent<HingeJoint>();
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (limitReached)
            return;

        checkAngle();
    }

    private void checkAngle()
    {
        if (hinge.angle >= hinge.limits.max)
        {
            limitReached = true;
            rb.isKinematic = true;

            foreach(UnityEvent cb in callbacksOnLimitReached)
            {
                cb.Invoke();
            }
        }
    }
}
