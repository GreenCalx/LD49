using UnityEngine;
using Schnibble;
using static UnityEngine.Debug;

[RequireComponent(typeof(Rigidbody))]
public class BullyablePNJ : MonoBehaviour
{
    private Rigidbody rb;
    public Transform centerOfMass;

    public bool enable_stabilization;
    public float unstablized_duration;

    public float stabilization_speed;
    private float elapsed_time;

    private Quaternion qBaseRot;
    public Transform childToStabilize;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (!!centerOfMass)
            rb.centerOfMass = centerOfMass.localPosition;
        enable_stabilization = true;
        elapsed_time = 0f;

        qBaseRot = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        DrawRay(childToStabilize.position, childToStabilize.up * 100, Color.red);
        if (enable_stabilization)
        {
            stabilize();
        }
        else
        {
            elapsed_time += Time.deltaTime;
            if (elapsed_time > unstablized_duration)
            { enable_stabilization = true; }
        }
    }

    void OnCollisionEnter(Collision iCol)
    {
        SchCarController cc = iCol.gameObject.GetComponent<SchCarController>();
        if (!cc)
            return;
        enable_stabilization = false;
        elapsed_time = Time.deltaTime;
    }

    void stabilize()
    {
        //this.Log("STABILIZE");
        //transform.rotation = Quaternion.Lerp( transform.rotation, qBaseRot, Time.time * stabilization_speed);
    }
}
