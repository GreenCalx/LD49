using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimateDeco1 : MonoBehaviour
{
    public float smooth = 1.0f;
    public float tiltAngle = 0.5f;

    public float y_delta = 1.0f;
    public float y_step = 0.1f;

    private float curr_angle = 0f;
    private float curr_y = 0f;

    private float base_y = 0f;
    private int y_direction = 1; // + or -
    private float upper_y_lim = 0f;
    private float lower_y_lim = 0f;

    // Start is called before the first frame update
    void Start()
    {
        curr_angle = 0f;
        curr_y = 0f;

        base_y = transform.position.y;
        y_direction = 1;

        upper_y_lim = base_y + y_delta;
        lower_y_lim = base_y - y_delta;
    }

    // Update is called once per frame
    void Update()
    {

        // rota
        curr_angle += tiltAngle;
        if (curr_angle >= 360)
            curr_angle = 0;

        float tiltAroundY = curr_angle;

        Quaternion target = Quaternion.Euler( 0, tiltAroundY, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, target,  Time.deltaTime * smooth);    

        // transfo
        if ( (transform.position.y >= upper_y_lim) || (transform.position.y <= lower_y_lim) )
        {
            y_direction *= (-1);
        }
        float new_y = transform.position.y + (y_step*y_direction);
        //transform.position = new Vector3(transform.position.x, new_y, transform.position.z);
        Vector3 new_pos = new Vector3(transform.position.x, new_y, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, new_pos, 0.5f );
    }
}
