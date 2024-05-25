using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static Schnibble.Physics;

public class SchTestColliderSphere : MonoBehaviour
{
    [SerializeReference]
    SchColliderSphere sph = new SchColliderSphere();

    public Transform collidingPoint;

    void OnDrawGizmos() {
        Color c = Color.white;
        if (sph.Contains(collidingPoint.position)) {
            c = Color.red;
        }

        Gizmos.color = c;

        Gizmos.matrix = sph.GetTransform();
        Gizmos.DrawWireSphere(sph.GetPositionLocal(), (float)sph.GetRadius());
        Gizmos.DrawWireSphere(sph.GetCenterOfMass(), 0.1f);
    }

}
