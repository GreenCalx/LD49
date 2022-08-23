using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KickAction : MonoBehaviour
{
    public Transform kickStartPos;
    public Transform kickEndPos;
    public BoxCollider kickCollider;
    public float kickStrength = 5.0f;
    public float kickSpeed = 1.0F;

    public bool kicking = false;

    private float startTime;
    private float journeyLength;

    // Start is called before the first frame update
    void Start()
    {
        kickCollider = GetComponent<BoxCollider>();
        kicking = false;
        kickCollider.enabled = false;

        startTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (kicking)
        {
            float distCovered = (Time.time - startTime) * kickSpeed;
            float fractionOfJourney = distCovered / journeyLength;

            float anim_timing = 10f; // kick fully deployedf at 10th frame
            kickCollider.center = Vector3.Lerp( kickStartPos.localPosition, kickEndPos.localPosition, fractionOfJourney);
            if (kickCollider.center == kickEndPos.localPosition)
            {
                kicking = false;
                kickCollider.center = kickStartPos.localPosition;
                kickCollider.enabled = false;
            }
        }
    }

    public void kick()
    {
        kickCollider.center = kickStartPos.localPosition;
        kicking = true;
        kickCollider.enabled = true;
        startTime = Time.time;
        journeyLength = Vector3.Distance(kickStartPos.localPosition, kickEndPos.localPosition);
    }

    void OnCollisionEnter(Collision collision)
    {
        CarController cc = collision.gameObject.GetComponent<CarController>();
        if (cc!=null)
        {
            Rigidbody rb = cc.GetComponent<Rigidbody>();
            Vector3 kickDirection = (kickEndPos.localPosition - kickStartPos.localPosition).normalized;
            rb.AddForce( -kickDirection * kickStrength, ForceMode.Force);
        }
        Dummy d = collision.gameObject.GetComponent<Dummy>();
        if (d!=null)
        {
            Rigidbody rb = d.GetComponent<Rigidbody>();
            Vector3 kickDirection = (kickEndPos.localPosition - kickStartPos.localPosition).normalized;
            rb.AddForce( -kickDirection * kickStrength, ForceMode.Impulse);
        }
    }
}
