using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushingTrap : Trap
{
    [Header("Mandatory")]
    public KickAction pushingAction;
    private GameObject pushingObject;

    [Header("Tweaks")]
    public Transform  endPos;
    public float timeBeforeReset = 2f;
    public float trapSpeed = 1f;
    public float smoothTimeOnReset = 0.5f;

    private Vector3 localInitPos;
    private Vector3 localEndPos;
    private Vector3 dampVelocity;

    //[HideInInspector]
    public bool isTriggered = false;

    // Start is called before the first frame update
    void Start()
    {
        pushingObject = pushingAction.gameObject;
        
        localInitPos    = transform.position;
        localEndPos     = endPos.position;
        pushingObject.transform.position = localInitPos;
    }

    // Update is called once per frame
    void Update()
    {
        if (isTriggered)
        {
            pushingObject.transform.position = Vector3.MoveTowards(pushingObject.transform.position, localEndPos, trapSpeed * Time.deltaTime);
            if ( Vector3.Distance(pushingObject.transform.position , localEndPos) < 0.1f )
            {
                isTriggered = false;
            }
        
        } else if ( Vector3.Distance(pushingObject.transform.position, localInitPos) > 0.1f ) 
        {
            pushingObject.transform.position = Vector3.SmoothDamp(pushingObject.transform.position, localInitPos, ref dampVelocity, smoothTimeOnReset, trapSpeed);
        }


    }

    public override void OnTrigger()
    {
        isTriggered = true;
        pushingAction.kick();
    }

}
