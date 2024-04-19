using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OctoPump : MonoBehaviour
{
    public List<ParticleSystem> PS_CanonSmoke;
    public Canon canon;
    public float CanonCooldown = 15f;
    private bool targetInSight;
    public Vector3 lastKnownTargetPosition;
    private float cooldown;

    private GameObject projectile_Inst;


    void Start()
    {
        targetInSight = false;
        cooldown = 0f;
    }

    void Update()
    {
        if (targetInSight)
        {
            if (cooldown < CanonCooldown)
            {
                cooldown += Time.deltaTime;
                return;
            }
            if (projectile_Inst!=null)
            {
                HomingMortar asMortar = projectile_Inst.GetComponent<HomingMortar>();
                asMortar.positionTarget = lastKnownTargetPosition;
                return;
            }

            projectile_Inst = canon.Fire();
            foreach(ParticleSystem PS in PS_CanonSmoke)
            { PS.Play(); }


            cooldown = 0f;
        } else {
             if (cooldown > 0f)
             {
                cooldown -= Time.deltaTime;
                cooldown = Mathf.Clamp(cooldown, 0f, CanonCooldown);

             }
        }
    }
    public void Fire()
    {

    }

    public void setTargetInSight(bool iState)
    {
        if (!targetInSight && iState)
        {
            
        }

        targetInSight = iState;
    }
}
