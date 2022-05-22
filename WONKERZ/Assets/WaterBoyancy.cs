using UnityEngine;


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
        if(!Utils.Math.ValidateForce(RB.velocity).Item2) {
            Debug.Break();
        }
        if (c.GetComponent<Rigidbody>().velocity.y > WaterSurfaceTension)
        {
            Physics.IgnoreCollision(c, GetComponent<MeshCollider>(), true);
        }
    }

    void OnTriggerStay(Collider c)
    {
        var RB = c.GetComponent<Rigidbody>();
        var F = -(RB.velocity + Physics.gravity * Time.deltaTime) * WaterDensity * (1-Vector3.Dot(RB.velocity.normalized, transform.up));
        bool IsGoodValue = false;
        (F, IsGoodValue) = Utils.Math.ValidateForce(F);
        if (!IsGoodValue) {
            Debug.Break();
            Debug.Log("VALIDATIOIN OF FORCE FAILED");
        }
        Debug.Log(F);
        RB.AddForce(F, ForceMode.VelocityChange);
    }

    void OnTriggerExit(Collider c)
    {
        Physics.IgnoreCollision(c, GetComponent<MeshCollider>(), false);
    }
}
