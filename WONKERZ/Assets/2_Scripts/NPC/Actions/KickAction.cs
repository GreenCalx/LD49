using UnityEngine;
using Schnibble;
using static UnityEngine.Debug;

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
    public bool useContactPointNormalForDirection = false;

    [Header("Debug")]
    public bool drawHitPointNormals = false;


    [HideInInspector]
    public bool kicking = false;

    // Start is called before the first frame update
    void Start()
    {
        kickCollider = GetComponent<BoxCollider>();
        kicking = false;
        if (autoColliderDisable)
            kickCollider.enabled = false; // if we manage collider with start/stop kick
        else
            kick(); // else we kick at all time

    }

    // Update is called once per frame
    void Update()
    {
        if (drawDebugRay)
        {
            Vector3 start = transform.parent.position;
            Vector3 end = transform.parent.forward * 100;
            end.y += Y_Slope;
            DrawRay(start, end, Color.green);
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

    Vector3 computeKickDirectionFromParent()
    {
        // default : based on parent forward
        Vector3 end = transform.parent.forward;
        end.y += Y_Slope;
        return end.normalized;
    }

    Vector3 computeKickDirectionFromContactPoint(Collision collision)
    {
        Vector3 end = new Vector3(0f, 0f, 0f);
        if (drawHitPointNormals)
        {
            foreach (var item in collision.contacts)
            {
                DrawRay(item.point, item.normal * 100, Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f), 10f);
            }
        }
        // take the first hitpoint as it can only be a player or a dummy
        end = -collision.contacts[0].normal;

        return end.normalized;
    }

    private float computeKickStrength(float mass)
    {
        return kickStrength + (mass * massMultiplier);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!kicking)
            return;

        SchCarController cc = collision.gameObject.GetComponent<SchCarController>();
        Dummy d = collision.gameObject.GetComponent<Dummy>();
        if (!cc && !d)
            return;

        Vector3 kickDirection = (useContactPointNormalForDirection) ?
            computeKickDirectionFromContactPoint(collision) :
            computeKickDirectionFromParent();


        if (cc != null)
        {
            Rigidbody rb = cc.car.chassis.rb.GetPhysXBody();
            float kickForce = computeKickStrength(rb.mass);
            rb.AddForce(kickDirection * kickForce, forceMode);
        }
        if (d != null)
        {
            Rigidbody rb = d.GetComponent<Rigidbody>();
            float kickForce = computeKickStrength(rb.mass);
            rb.AddForce(kickDirection * kickForce, forceMode);
        }
    }
}
