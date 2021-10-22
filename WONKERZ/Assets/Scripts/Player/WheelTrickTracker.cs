using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(WheelCollider))]
public class WheelTrickTracker : MonoBehaviour
{
    public WHEEL_LOCATION wheel_location;

    [HideInInspector]
    public bool is_grounded = true;
    [HideInInspector]
    public TrickTracker tracker;

    private WheelCollider wc;
    private WheelHit last_hit;

    public 
    // Start is called before the first frame update
    void Start()
    {
        wc = GetComponent<WheelCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        bool ground_hit = wc.GetGroundHit(out last_hit);
        if ( ground_hit != is_grounded )
        {
            is_grounded = ground_hit;
            tracker.notify(this);
        }
    }

}
