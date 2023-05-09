using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schnibble;

public class StartLine : MonoBehaviour
{
    [Header("MAND")]
    public string track_name;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void OnTriggerEnter(Collider iCol)
    {
        CarController cc = iCol.GetComponent<CarController>();

        if (!!cc)
        {
            // start line crossed !! gogogo
            
            Access.TrackManager().launchTrack(track_name);
        }
    }
}