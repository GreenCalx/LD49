using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// STARTING POINT FOR HUB
// > ACTIVATES TRICKS AUTO
public class StartPortal : MonoBehaviour
{
    public GameObject playerRef;

    // Start is called before the first frame update
    void Start()
    {
        CarController player = playerRef.GetComponent<CarController>();
        if (!!player)
        {
            playerRef.transform.position = transform.position;
            TrickTracker tt = playerRef.GetComponent<TrickTracker>();
            if (!!tt)
            {
                tt.activate_tricks = true; // activate default in hub
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
