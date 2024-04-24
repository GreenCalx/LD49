using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static Schnibble.Physics;

public class SchTestColliderCylinder : MonoBehaviour
{
    [SerializeReference]
    SchColliderCylinder box = new SchColliderCylinder();

    public Transform collidingPoint;

    public enum Mode {
        Transform,
        LocalBounds,
        GlobalBounds,
    };
    public Mode mode;


    void OnDrawGizmos() {

        Color c = Color.white;
        if (box.Contains(collidingPoint.position)) {
            c = Color.red;
        }

        if (mode == Mode.Transform) {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(box.GetPosition(), box.GetPosition() + box.GetWorldVector(Vector3.up));
            Gizmos.color = Color.red;
            Gizmos.DrawLine(box.GetPosition(), box.GetPosition() + box.GetWorldVector(Vector3.right));
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(box.GetPosition(), box.GetPosition() + box.GetWorldVector(Vector3.forward));

            Gizmos.matrix = box.GetTransform();
            Gizmos.color = c;
            Gizmos.DrawWireMesh(SchColliderCylinder.GetDebugMesh(), Vector3.zero, Quaternion.identity, new Vector3(box.GetDiameter(), box.GetHeight(), box.GetDiameter()));
            Gizmos.DrawWireSphere(box.GetCenterOfMassLocal(), 0.1f);
        }

        if (mode == Mode.LocalBounds) {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(box.GetPosition(), box.GetPosition() + box.GetRotationLocal() * (Vector3.up));
            Gizmos.color = Color.red;
            Gizmos.DrawLine(box.GetPosition(), box.GetPosition() + box.GetRotationLocal() * (Vector3.right));
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(box.GetPosition(), box.GetPosition() + box.GetRotationLocal() * (Vector3.forward));

            Gizmos.color = c;
            var b = box.GetBoundsLocal();
            Gizmos.DrawWireMesh(SchTestColliderBox.debugMesh, box.GetWorldPoint(b.center), box.GetRotationLocal(), b.size);
        }

        if (mode == Mode.GlobalBounds) {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(box.GetPosition(), box.GetPosition() + box.GetWorldVector(Vector3.up));
            Gizmos.color = Color.red;
            Gizmos.DrawLine(box.GetPosition(), box.GetPosition() + box.GetWorldVector(Vector3.right));
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(box.GetPosition(), box.GetPosition() + box.GetWorldVector(Vector3.forward));

            Gizmos.color = c;
            var b = box.GetBounds();
            Gizmos.DrawWireMesh(SchTestColliderBox.debugMesh, b.center, box.GetRotation(), b.size);
        }
    }
}
