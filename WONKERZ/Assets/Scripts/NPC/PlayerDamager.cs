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
        
        if (Utils.collisionIsPlayer(iCol))
        {
            ContactPoint cp = iCol.contacts[0];
            Access.Player().takeDamage(damageOnCollide, cp.point, cp.normal);
        }
    }

    void OnTriggerEnter(Collider iCol)
    {
        if (!!Utils.colliderIsPlayer(iCol))
        {
            Access.Player().takeDamage(damageOnCollide, transform.position, transform.forward);
        }
    }
}
