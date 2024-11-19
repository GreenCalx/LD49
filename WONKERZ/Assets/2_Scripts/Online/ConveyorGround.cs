using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConveyorGround : MonoBehaviour
{
    public float conveyForce = 1000f;
    //public List<GameObject> conveyedObjects;

    void Start()
    {
        Rigidbody selfRB = GetComponent<Rigidbody>();
        if (selfRB==null)
            return;

        selfRB.sleepThreshold = 0f;
        selfRB.WakeUp();
    }


    public void OnCollsionStay(Collision iCollision)
    {
        Debug.Log("HELLO !!!!");

        Rigidbody rb = iCollision.gameObject.GetComponent<Rigidbody>();
        if (!!rb)
        {
            ContactPoint cp = iCollision.contacts[0];
            Vector3 cp_normal = cp.normal;
            Debug.DrawRay(cp.point, cp.normal * 50f, Color.green );

            Vector3 v = Quaternion.AngleAxis(90, Vector3.right) * cp_normal;
            Debug.DrawRay(cp.point, v * 50f, Color.yellow );

            rb.AddForce(conveyForce * v, ForceMode.Force);
        }
        
    }


}
