using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Schnibble;
using static Schnibble.Physics;

public class SchTestColliderCapsule : MonoBehaviour
{
    [SerializeReference]
    SchColliderCapsule sph = new SchColliderCapsule();

    public Transform collidingPoint;

    void OnDrawGizmos() {
        Color c = Color.white;
        if (sph.Contains(collidingPoint.position)) {
            c = Color.red;
        }

        Gizmos.color = c;

        Gizmos.matrix = sph.GetTransform();
        Gizmos.DrawWireSphere(sph.GetHalfHeight() * Vector3.up, (float)sph.GetRadius());
        Gizmos.DrawWireSphere(-sph.GetHalfHeight() * Vector3.up, (float)sph.GetRadius());
        Gizmos.DrawWireMesh(SchColliderCylinder.GetDebugMesh(), Vector3.zero, Quaternion.identity, new Vector3(sph.GetDiameter(), sph.GetHeight(), sph.GetDiameter()));
        Gizmos.DrawWireSphere(sph.GetCenterOfMassLocal(), 0.1f);
    }
}
