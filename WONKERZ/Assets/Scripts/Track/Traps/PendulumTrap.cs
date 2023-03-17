using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PendulumTrap : MonoBehaviour
{
    [Range(0.01f, 2f)]
    public float amplitude = 1f;
    [Range(0.01f, 4f)]
    public float speed = 1f;
    [Range(0,3)]
    public int phase = 0;
    
    
    float timer = 0f;
    

    // Update is called once per frame
    void FixedUpdate()
    {
        timer += Time.deltaTime;
        if (timer>1f)
        {
            phase++;
            phase %= 4;
            timer = 0f;
        }

        switch (phase)
        {
            case 0:
                transform.Rotate(0f, 0f, (speed * (1-timer))*amplitude);
                break;
            case 1:
                transform.Rotate(0f, 0f, (-speed * timer)*amplitude);
                break;
            case 2:
                transform.Rotate(0f, 0f, (-speed * (1-timer))*amplitude);
                break;
            case 3:
                transform.Rotate(0f, 0f, (speed * timer)*amplitude);
                break;
        }
    }
}
