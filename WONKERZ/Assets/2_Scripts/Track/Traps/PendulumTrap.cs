using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PendulumTrap : MonoBehaviour
{

    public Vector3 axis;
    [Range(0.01f, 2f)]
    public float amplitude = 1f;
    [Range(0.01f, 4f)]
    public float speed = 1f;
    [Range(0,3)]
    public int phase = 0;
    [Range(0.1f,5f)]
    public float timerForPhaseSwitch = 1f;    
    
    float timer = 0f;
    

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
}
