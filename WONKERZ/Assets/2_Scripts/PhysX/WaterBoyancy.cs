using UnityEngine;

using Schnibble;
using static Schnibble.Physics;
using static UnityEngine.Physics;
using static UnityEngine.Debug;
using static Schnibble.Utils;


public class WaterBoyancy : MonoBehaviour
{
    public float WaterSurfaceTension;
    public float WaterDensity;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter(Collider c)
    {
        var RB = c.GetComponent<Rigidbody>();
        if (!ValidateForce(RB.velocity).Item2)
        {
            Break();
        }
        if (c.GetComponent<Rigidbody>().velocity.y > WaterSurfaceTension)
        {
            IgnoreCollision(c, GetComponent<MeshCollider>(), true);
        }
    }

    void OnTriggerStay(Collider c)
    {
        var RB = c.GetComponent<Rigidbody>();
        var F = -(RB.velocity + gravity * Time.deltaTime) * WaterDensity * (1 - Vector3.Dot(RB.velocity.normalized, transform.up));
        bool IsGoodValue = false;
        (F, IsGoodValue) = ValidateForce(F);
        if (!IsGoodValue)
        {
            Break();
            this.Log("VALIDATIOIN OF FORCE FAILED");
        }

        RB.AddForce(F, ForceMode.VelocityChange);
    }

    void OnTriggerExit(Collider c)
    {
        IgnoreCollision(c, GetComponent<MeshCollider>(), false);
    }
}
