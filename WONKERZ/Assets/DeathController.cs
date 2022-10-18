using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathController : MonoBehaviour
{
    public List<Rigidbody> objects;
    public float force;
    public float radius;
    public float upmodif;
    // Start is called before the first frame update
    void Start()
    {
        foreach (var rb in objects){
            rb.isKinematic = true;
            rb.detectCollisions = false;
        }
    }

    public void Activate(){
        foreach (var rb in objects){
            rb.isKinematic = false;
            rb.detectCollisions = true;
            rb.AddExplosionForce(force, transform.position, radius, upmodif);
        }

        GetComponent<CarController>().isFrozen = true;
        GetComponent<Rigidbody>().AddExplosionForce(force, transform.position, radius, upmodif, ForceMode.Acceleration);
    }
}
