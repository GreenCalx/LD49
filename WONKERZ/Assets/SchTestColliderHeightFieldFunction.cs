using Schnibble.SIUnits;
using System.Collections.Generic;
using UnityEngine;
using static Schnibble.Physics;
public class SchTestColliderHeightFieldFunction : MonoBehaviour
{
    [SerializeReference]
    SchColliderHeightFieldFunction col = new SchColliderHeightFieldFunction();

    Mesh debugMesh;

    public Transform collidingPoint;

    public int x;
    public int y;

    public float A;
    public float w;

    public float radius;

    Meter GetHeight(float x, float y)
    {
        // diff in local space.
        var pt_local = col.GetLocalPoint(new Vector3(x, col.GetPosition().y, y));
        if (pt_local.sqrMagnitude > (radius*radius)) return (Meter)0.0f;
        return (Meter)(A * Mathf.Cos((x + y) - w * Time.realtimeSinceStartup)) + (Meter)(A * Mathf.Cos((x - y) + w * Time.realtimeSinceStartup));
    }

    void OnDrawGizmos()
    {
        col.fun = GetHeight;

        if (debugMesh == null) debugMesh = new Mesh();

        // recompute mesh, very inefficient but only for demo purposes.
        float step_x = col.size.x / x;
        float step_y = col.size.z / y;

        Vector3[] verts = new Vector3[(x + 1) * (y + 1)];

        for (int i = 0; i < (x + 1); ++i)
        {
            for (int j = 0; j < (y + 1); ++j)
            {
                var v = new Vector3(step_x * i - col.size.x*0.5f, 0.0f, step_y * j - col.size.z*0.5f);
                var height = (float)col.GetHeight(col.GetWorldPoint(v));
                v.y = height;
                verts[i + j * (x + 1)] = v;
            }
        }

        List<int> tris = new List<int>();
        for (int i = 0; i < x; ++i)
        {
            for (int j = 0; j < y; ++j)
            {

                var p1 = i + j * (x + 1);
                var p2 = p1 + 1;
                var p3 = p1 + (x + 1);
                var p4 = p2 + (x + 1);


                tris.Add(p1);
                tris.Add(p2);
                tris.Add(p4);

                tris.Add(p1);
                tris.Add(p3);
                tris.Add(p4);
            }
        }

        debugMesh.SetVertices(verts);
        debugMesh.SetTriangles(tris, 0);
        debugMesh.Optimize();
        debugMesh.RecalculateNormals();

        Color c = Color.white;
        if (col.Contains(collidingPoint.position))
        {
            c = Color.red;
        }

        Gizmos.color = c;

        Gizmos.matrix = col.GetTransform();
        Gizmos.DrawWireMesh(debugMesh);

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(col.GetBounds().center, col.GetBounds().size);
    }
}
