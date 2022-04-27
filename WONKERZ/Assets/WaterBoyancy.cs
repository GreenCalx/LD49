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
        if (c.GetComponent<Rigidbody>().velocity.y > WaterSurfaceTension)
        {
            Physics.IgnoreCollision(c, GetComponent<MeshCollider>(), true);
        }
    }

    void OnTriggerStay(Collider c)
    {
        var RB = c.GetComponent<Rigidbody>();
        RB.AddForce(-(RB.velocity + Physics.gravity * Time.deltaTime) * WaterDensity * (1-Vector3.Dot(RB.velocity.normalized, transform.up)), ForceMode.VelocityChange);
    }

    void OnTriggerExit(Collider c)
    {
        Physics.IgnoreCollision(c, GetComponent<MeshCollider>(), false);
    }
}
