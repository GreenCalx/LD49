using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Controlled by SMBKick into Animator
public class KickAction : MonoBehaviour
{
    public bool drawDebugRay = false;

    private BoxCollider kickCollider;
    [Header("Tweaks")]
    public ForceMode forceMode;
    public float kickStrength = 5.0f;
    public float Y_Slope;
    public float massMultiplier = 10f;
    public bool autoColliderDisable = false;


    [HideInInspector]
    public bool kicking = false;

    // Start is called before the first frame update
    void Start()
    {
        kickCollider = GetComponent<BoxCollider>();
        kicking = false;
        if (autoColliderDisable)
            kickCollider.enabled = false;

    }

    // Update is called once per frame
    void Update()
    {
        if (drawDebugRay)
        {
            Vector3 start = transform.parent.position;
            Vector3 end = transform.parent.forward*10;
            end.y += Y_Slope;
            Debug.DrawRay(start, end, Color.green );
        }
    }

    public void kick()
    {
        kicking = true;
        if (autoColliderDisable)
            kickCollider.enabled = true;
    }

    public void stopKick()
    {
        kicking = false;
        if (autoColliderDisable)
            kickCollider.enabled = false;
    }

    Vector3 computeKickDirection()
    {
        Vector3 end = transform.parent.forward;
        end.y += Y_Slope;

        return end.normalized;
    }

    private float computeKickStrength(float mass)
    {
        return kickStrength + (mass*massMultiplier);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!kicking)
            return;
        
        Vector3 kickDirection   = computeKickDirection();
        
        CarController cc = collision.gameObject.GetComponent<CarController>();
        if (cc!=null)
        {
            Rigidbody rb = cc.GetComponent<Rigidbody>();
            float kickForce = computeKickStrength(rb.mass);
            rb.AddForce( kickDirection * kickForce, forceMode);
        }
        Dummy d = collision.gameObject.GetComponent<Dummy>();
        if (d!=null)
        {
            Rigidbody rb = d.GetComponent<Rigidbody>();
            float kickForce = computeKickStrength(rb.mass);
            rb.AddForce( kickDirection * kickForce, forceMode);
        }
    }
}
