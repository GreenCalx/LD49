using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Schnibble;
using static Schnibble.Physics;

public class SchTestColliderBox : MonoBehaviour
{
    [SerializeReference]
    SchColliderBox box = new SchColliderBox();

    public static Mesh debugMesh;

    public Transform collidingPoint;

    public enum Mode {
        Transform,
        LocalBounds,
        GlobalBounds,
    };
    public Mode mode;

    void OnDrawGizmos() {
        if (debugMesh == null) {
            debugMesh = new Mesh();
            debugMesh.SetVertices(
                new Vector3[]{
                    new Vector3(0.5f, 0.5f, 0.5f),

                    new Vector3(-0.5f, 0.5f, 0.5f),
                    new Vector3(-0.5f, -0.5f, 0.5f),
                    new Vector3(0.5f, -0.5f, 0.5f),

                    new Vector3(0.5f, 0.5f, -0.5f),
                    new Vector3(-0.5f, 0.5f, -0.5f),
                    new Vector3(-0.5f, -0.5f, -0.5f),
                    new Vector3(0.5f, -0.5f, -0.5f),
                }
            );

            debugMesh.SetTriangles(
                new int[] {
                    //front
                    0, 1, 2,
                    2, 3, 0,
                    // top
                    // right
                    0, 3, 7,
                    0, 7, 4,
                    // back
                    4, 5, 6,
                    6, 7, 4,
                    // left
                    1, 2, 6,
                    5, 1, 4,
                    // bottom
                    6, 2, 3,
                    6, 3, 7,
                } , 0
            );

            debugMesh.Optimize();
            debugMesh.RecalculateNormals();
        }

        Color c = Color.white;
        if (box.Contains(collidingPoint.position)) {
            c = Color.red;
        }

        Gizmos.color = c;
        if (mode == Mode.Transform) {
            Gizmos.matrix = box.GetTransform();
            Gizmos.DrawWireMesh(debugMesh, Vector3.zero, Quaternion.identity, box.GetHalfExtents() * 2.0f);
            Gizmos.DrawWireSphere(box.GetCenterOfMassLocal(), 0.1f);

            Gizmos.matrix = Matrix4x4.identity;
            Gizmos.color = Color.green;
            Gizmos.DrawLine(box.GetPosition(), box.GetPosition() + box.GetWorldVector(Vector3.up));
            Gizmos.color = Color.red;
            Gizmos.DrawLine(box.GetPosition(), box.GetPosition() + box.GetWorldVector(Vector3.right));
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(box.GetPosition(), box.GetPosition() + box.GetWorldVector(Vector3.forward));
        }

        if (mode == Mode.LocalBounds) {
            var b = box.GetBoundsLocal();
            Gizmos.DrawWireMesh(debugMesh, box.GetWorldPoint(b.center), box.GetRotationLocal(), b.size);

            Gizmos.color = Color.green;
            Gizmos.DrawLine(box.GetPosition(), box.GetPosition() + box.GetRotationLocal()*Vector3.up);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(box.GetPosition(), box.GetPosition() + box.GetRotationLocal()*Vector3.right);
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(box.GetPosition(), box.GetPosition() + box.GetRotationLocal()*Vector3.forward);
        }

        if (mode == Mode.GlobalBounds) {
            var b = box.GetBounds();
            Gizmos.DrawWireMesh(debugMesh, b.center, box.GetRotation(), b.size);

            Gizmos.matrix = Matrix4x4.identity;
            Gizmos.color = Color.green;
            Gizmos.DrawLine(box.GetPosition(), box.GetPosition() + box.GetWorldVector(Vector3.up));
            Gizmos.color = Color.red;
            Gizmos.DrawLine(box.GetPosition(), box.GetPosition() + box.GetWorldVector(Vector3.right));
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(box.GetPosition(), box.GetPosition() + box.GetWorldVector(Vector3.forward));
        }
    }
}
