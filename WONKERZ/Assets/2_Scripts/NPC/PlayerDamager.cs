using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schnibble;

public class PlayerDamager : MonoBehaviour
{
    public int damageOnCollide = 99;

    [Range(0,500)]
    public int optional_repulsion_force = 0;
    private readonly float delayBetweenDamages = 0.2f;
    private float elapsedTimeSinceLastDamage = 0f;

    

    // Start is called before the first frame update
    void Start()
    {
        elapsedTimeSinceLastDamage = delayBetweenDamages;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        elapsedTimeSinceLastDamage += Time.fixedDeltaTime;
    }

    void OnCollisionEnter(Collision iCol)
    {
        
        if (Utils.collisionIsPlayer(iCol))
        {
            ContactPoint cp = iCol.contacts[0];
            tryDoDamage(cp.point, cp.normal);
        }
    }

    void OnTriggerEnter(Collider iCol)
    {
        if (!!Utils.colliderIsPlayer(iCol))
        {
            tryDoDamage(transform.position, transform.forward);
        }
    }

    void OnCollisionStay(Collision iCol)
    {
        
        if (Utils.collisionIsPlayer(iCol))
        {
            ContactPoint cp = iCol.contacts[0];
            tryDoDamage(cp.point, cp.normal);
        }
    }

    void OnTriggerStay(Collider iCol)
    {
        if (!!Utils.colliderIsPlayer(iCol))
        {
            tryDoDamage(transform.position, transform.forward);
        }
    }

    private void tryDoDamage(Vector3 iDamageOrigin, Vector3 iDamageDir)
    {
        if (elapsedTimeSinceLastDamage < delayBetweenDamages)
            return;

        if (optional_repulsion_force != 0)
        {
            Access.Player().takeDamage(damageOnCollide, iDamageOrigin, iDamageDir, optional_repulsion_force);
        } else {
            Access.Player().takeDamage(damageOnCollide, iDamageOrigin, iDamageDir);
        }

        elapsedTimeSinceLastDamage = 0f;
    }
}
