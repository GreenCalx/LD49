using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : GameCamera
{   
    [Header("PlayerCamera")]
    [Range(0.0f, 1.0f)]
    public float FOVEffect_damp;
    [Range(50f, 100.0f)]
    public float FOVEffect_speedThreshold;
    [Range(0.01f,1.00f)]
    public float FOVEffect_LerpStep;
    public GameObject playerRef;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void applySpeedEffect(float iSpeed)
    {
        Camera cam = GetComponent<Camera>();

        // apply_factor : [0:~1,1] intensity of effect
        double apply_factor = (iSpeed-FOVEffect_speedThreshold)/100;
        apply_factor = (apply_factor<0)? 0:apply_factor; // remove neg vals
        float newFOV = initial_FOV + ((initial_FOV*(float)apply_factor)*FOVEffect_damp);
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, newFOV, FOVEffect_LerpStep);
    }


    public override void init() 
    {
        playerRef = Utils.getPlayerRef();
    }
}
