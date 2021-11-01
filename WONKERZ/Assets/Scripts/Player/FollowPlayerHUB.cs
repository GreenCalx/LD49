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

    private Vector3 offset;
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
        var MaxDistance = MaxDistancePosition.magnitude;

        var MaxDistanceMagn = (MaxDistancePosition - FinalPosition).magnitude;

        var CurrentDistance = (transform.position - FinalPosition).magnitude;

        var Lerp = (CurrentDistance / MaxDistanceMagn);

        Debug.Log("ANGLE + " + Vector3.Angle(transform.forward,Following.transform.forward) );
        
        if ( Vector3.Angle(transform.forward,Following.transform.forward) > 90 )
        {
            offset = Quaternion.AngleAxis (turnSpeed, Vector3.up) * offset;
        }

        //transform.position = Vector3.Lerp(transform.position, FinalPosition, Lerp);
        transform.position = Vector3.Lerp(transform.position + offset, FinalPosition, Lerp);

        transform.LookAt(Following.transform.position);

    }
}
