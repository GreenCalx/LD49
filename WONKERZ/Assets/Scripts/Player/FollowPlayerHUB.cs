using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayerHUB : MonoBehaviour
{
    public GameObject Following;
    public Vector3 Distance;
    public float LerpMult;
    private bool Active = true;

    public float turnSpeed = 5.0f;
    public CheckPointManager Mng;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!Active) return;

        var FinalPosition = Following.transform.position + Distance.x * Vector3.right + Distance.y * Vector3.up + Distance.z * Vector3.forward;
        var FinalPositionDirection = Following.transform.position - FinalPosition;
        var MaxDistancePosition = (FinalPosition - (FinalPositionDirection * LerpMult));

        var MaxDistanceMagn = (MaxDistancePosition - FinalPosition).magnitude;

        var CurrentDistance = (transform.position - FinalPosition).magnitude;

        var Lerp = (CurrentDistance / MaxDistanceMagn);

        transform.position = Vector3.Lerp(transform.position, FinalPosition, Lerp);
        transform.LookAt(Following.transform.position);

        var xz_cam = new Vector3( transform.forward.x, 0, transform.forward.z);
        var xz_target = new Vector3( Following.transform.forward.x, 0, Following.transform.forward.z);
        var xz_angle = Vector3.Angle( xz_cam, xz_target);

        Debug.Log("ANGLE + " + xz_angle );
        if ( xz_angle >= 0 )
        {
            transform.RotateAround(Following.transform.position, Vector3.up, xz_angle );
        }

        
    }
}
