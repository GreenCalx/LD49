using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schnibble;

public class PlayerDamager : MonoBehaviour
{
    public int damageOnCollide = 99;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionEnter(Collision iCol)
    {
        CarController cc = iCol.gameObject.GetComponent<CarController>();
        if (!!cc)
        {
            ContactPoint cp = iCol.contacts[0];
            cc.takeDamage(damageOnCollide, cp.point, cp.normal);
        }
    }

    void OnTriggerEnter(Collider iCol)
    {
        CarController cc = iCol.gameObject.GetComponent<CarController>();
        if (!!cc)
        {
            cc.takeDamage(damageOnCollide, transform.position, transform.forward);
        }
    }
}
