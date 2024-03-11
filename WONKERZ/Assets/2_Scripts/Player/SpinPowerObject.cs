using UnityEngine;

public class SpinPowerObject : MonoBehaviour

{
    [Header("Tweaks")]
    public float repulsionForce = 10f;

    [Header("Legacy?")]
    public Rigidbody rb;
    private Vector3 prevVel;
    private Vector3 prevAngVel;
    private Vector3 prevPos;

    // Start is called before the first frame update
    void Start()
    {
        // bool i = false;
        // string s = "test";
        // int k = 100;
        // Rigidbody r;
        // rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // prevVel = rb.velocity;
        // prevAngVel = rb.angularVelocity;
        // prevPos = transform.position;
    }

    public void	breakableObjectCollisionCorrection()
    {
        // rb.velocity = prevVel;
        // rb.angularVelocity = prevAngVel;
        // transform.position = prevPos + rb.velocity * Time.deltaTime;
    }

    void OnCollisionEnter(Collision iCol)
    {
        Debug.Log("SpinPowerObject collision : " + iCol.gameObject.name);
        Rigidbody collider_rb = iCol.gameObject.GetComponent<Rigidbody>();
        if (!!collider_rb)
            collider_rb = iCol.gameObject.GetComponentInChildren<Rigidbody>();

        if (!!collider_rb)
        {
            Reflectable as_reflectable = iCol.gameObject.GetComponent<Reflectable>();
            if (!!as_reflectable)
            {
                if (as_reflectable.tryReflect())
                {
                    repulsionForce *= as_reflectable.reflectionMultiplier;

                    if (as_reflectable.autoAimOnReflection != null)
                    {
                        Vector3 autoAimDir = as_reflectable.autoAimOnReflection.position - iCol.gameObject.transform.position;
                        collider_rb.AddForce(autoAimDir.normalized * as_reflectable.reflectionMultiplier, ForceMode.VelocityChange);
                        return;
                    }
                } else { 
                    return; // already reflected
                }
            } 
            Vector3 f_dir = iCol.gameObject.transform.position - transform.position;
            collider_rb.AddForce(f_dir.normalized * repulsionForce, ForceMode.VelocityChange);
        }
    }

    void OnCollisionStay(Collision iCol)
    {
        //Ground gnd = iCol.collider.GetComponent<Ground>();
        //if (!!gnd)
        //{
            //if (gnd.GI.Type == Ground.EType.WATER)
        //{
        //PowerController PC = Access.Player().gameObject.GetComponent<PowerController>();
        //PC.setNextPower(0);
        //PC.tryTriggerPower();
        //}
        //}
    }
}
