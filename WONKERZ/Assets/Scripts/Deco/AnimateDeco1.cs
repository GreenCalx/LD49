using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimateDeco1 : MonoBehaviour
{
    public float smooth = 1.0f;
    public float tiltAngle = 0.5f;

    private float curr_angle = 0f;
    // Start is called before the first frame update
    void Start()
    {
        curr_angle = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        curr_angle += tiltAngle;
        if (curr_angle >= 360)
            curr_angle = 0;

        float tiltAroundY = curr_angle;

        Quaternion target = Quaternion.Euler( 0, tiltAroundY, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, target,  Time.deltaTime * smooth);    
    }
}
