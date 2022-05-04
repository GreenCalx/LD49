using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Triggers only once in current scene
// > TODO : save seen cinematics to init triggerrs accordingly
public class CinematicTrigger : MonoBehaviour
{
    private bool triggered= false;

    public bool freezePlayer = true;
    public CinematicCamera cam;
    // Start is called before the first frame update
    void Start()
    {
        triggered =false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider iCollider)
    {
        if (!!iCollider.GetComponent<CarController>())
        {
            triggered = true;
            cam.launch();
            // do cinematic stuff    
        }
        
    }
}
