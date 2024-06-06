using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PendulumTrap : MonoBehaviour
{
    HingeJoint joint;
    Rigidbody body;
    public float angleLimit     = 45.0f;
    public float targetVelocity = 10.0f;
    public float minTargetVelocity = 10.0f;


    #if false
    Vector3 axis;
    [Range(0.01f, 2f)]
    public float amplitude = 1f;
    [Range(0.01f, 4f)]
    public float speed = 1f;
    [Range(0,3)]
    public int phase = 0;
    [Range(0.1f,5f)]
    public float timerForPhaseSwitch = 1f;    
    
    float timer = 0f;
    #endif

    void Awake() {
        joint = GetComponent<HingeJoint>();
        body = GetComponent<Rigidbody>();

        var motor = joint.motor;
        motor.targetVelocity = targetVelocity;
        joint.motor = motor;
    }

    void FixedUpdate() {
        var angle = joint.angle;
        var motor = joint.motor;
        if (angle < -angleLimit || angle > angleLimit) targetVelocity = -targetVelocity;

        motor.targetVelocity = Mathf.Lerp(minTargetVelocity * Mathf.Sign(targetVelocity), targetVelocity, (angleLimit - Mathf.Abs(angle)) / angleLimit);

        joint.motor = motor;
    }

    #if false
    // Update is called once per frame
    void FixedUpdate()
    {
        timer += Time.deltaTime;
        if (timer>timerForPhaseSwitch)
        {
            phase++;
            phase %= 4;
            timer = 0f;
        }

        switch (phase)
        {
            case 0:
                transform.Rotate(
                    axis.x * (speed * (1-timer))*amplitude, 
                    axis.y * (speed * (1-timer))*amplitude, 
                    axis.z * (speed * (1-timer))*amplitude
                );
                break;
            case 1:
                transform.Rotate(
                    axis.x * (-speed * timer)*amplitude, 
                    axis.y * (-speed * timer)*amplitude, 
                    axis.z * (-speed * timer)*amplitude);
                break;
            case 2:
                transform.Rotate(
                    axis.x * (-speed * (1-timer))*amplitude, 
                    axis.y * (-speed * (1-timer))*amplitude, 
                    axis.z * (-speed * (1-timer))*amplitude);
                break;
            case 3:
                transform.Rotate(
                    axis.x * (speed * timer)*amplitude, 
                    axis.y * (speed * timer)*amplitude, 
                    axis.z * (speed * timer)*amplitude
                );
                break;
        }
    }
    #endif
}
