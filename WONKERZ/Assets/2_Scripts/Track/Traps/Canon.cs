using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Canon : MonoBehaviour
{
    public Transform spawnPoint;
    public Transform aimPoint;
    public float throwForce = 10f;
    public GameObject projectile_Ref;
    public float projectileDuration = 3f;
    ///

    public GameObject Fire()
    {
        GameObject projectile_Inst = Instantiate(projectile_Ref);
        projectile_Inst.transform.position = spawnPoint.position;
        projectile_Inst.transform.parent = null;

        Rigidbody projectile_rb = projectile_Inst.GetComponentInChildren<Rigidbody>();
        if (!!projectile_rb)
        {
            Vector3 f_dir = aimPoint.position - spawnPoint.position;
            projectile_rb.AddForce( f_dir * throwForce, ForceMode.VelocityChange);
        }

        Destroy(projectile_Inst, projectileDuration);
        return projectile_Inst;
    }
}