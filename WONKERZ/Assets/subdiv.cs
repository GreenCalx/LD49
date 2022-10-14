using System.Collections.Generic;
using UnityEngine;

public class subdiv : MonoBehaviour
{
    public int X;
    public int Y;
    public int Z;

    public List<Mesh> cells = new List<Mesh>();

    // Start is called before the first frame update
    public void Bake()
    {
        var mesh = GetComponent<MeshFilter>().sharedMesh;
        var rend = GetComponent<MeshRenderer>();

        var existing_result = GameObject.Find(gameObject.name + "_bake");
        if (existing_result)
        {
            GameObject.DestroyImmediate(existing_result);
        }

        GameObject parent = new GameObject(gameObject.name + "_bake");
        parent.transform.parent = gameObject.transform.parent;
        parent.transform.localRotation = gameObject.transform.localRotation;
        parent.transform.localPosition = gameObject.transform.localPosition;


        var b = mesh.bounds;

        var vertices = mesh.vertices;
        var triangles = mesh.triangles;

        int[] vert_table = new int[vertices.Length];
        for (float x = b.min.x; x < b.max.x; x += X)
            for (float y = b.min.y; y < b.max.y; y += Y)
                for (float z = b.min.z; z < b.max.z; z += Z)
                {
                    Mesh new_mesh = new Mesh();
                    new_mesh.name = mesh.name + "_" + cells.Count;

                    List<Vector3> cell_vert = new List<Vector3>();
                    List<Vector2> cell_uv = new List<Vector2>();
                    List<Vector2> cell_uv2 = new List<Vector2>();
                    List<Vector3> cell_normal = new List<Vector3>();
                    List<int> cell_tri = new List<int>();
                    for (var v = 0; v < vert_table.Length; ++v)
                    {
                        vert_table[v] = -1;
                    }
                    Bounds cell = new Bounds(new Vector3(x + X / 2, y + Y / 2, z + Z / 2), new Vector3(X, Y, Z));
                    for (int v = 0; v < vertices.Length; ++v)
                    {
                        if (cell.Contains(vertices[v]))
                        {
                            cell_vert.Add(vertices[v]);
                            cell_uv.Add(mesh.uv[v]);
                            cell_uv2.Add(mesh.uv2[v]);
                            cell_normal.Add(mesh.normals[v]);
                            vert_table[v] = cell_vert.Count - 1;
                        }
                    }

                    if (cell_vert.Count == 0) continue;

                    int[] vert_table_copy = new int[vert_table.Length];
                    for (int i = 0; i < vert_table.Length; ++i)
                    {
                        vert_table_copy[i] = vert_table[i];
                    }
                    for (int t = 0; t < triangles.Length; t += 3)
                    {
                        if (vert_table_copy[triangles[t + 0]] != -1 || vert_table_copy[triangles[t + 1]] != -1 || vert_table_copy[triangles[t + 2]] != -1)
                        {
                            if (vert_table[triangles[t + 0]] != -1)
                                cell_tri.Add(vert_table[triangles[t + 0]]);
                            else
                            {
                                int v = triangles[t + 0];
                                cell_vert.Add(vertices[v]);
                                cell_uv.Add(mesh.uv[v]);
                                cell_uv2.Add(mesh.uv2[v]);
                                cell_normal.Add(mesh.normals[v]);
                                vert_table[v] = cell_vert.Count - 1;
                                cell_tri.Add(vert_table[triangles[t + 0]]);
                            }
                            if (vert_table[triangles[t + 1]] != -1)
                                cell_tri.Add(vert_table[triangles[t + 1]]);
                            else
                            {
                                int v = triangles[t + 1];
                                cell_vert.Add(vertices[v]);
                                cell_uv.Add(mesh.uv[v]);
                                cell_uv2.Add(mesh.uv2[v]);
                                cell_normal.Add(mesh.normals[v]);
                                vert_table[v] = cell_vert.Count - 1;
                                cell_tri.Add(vert_table[triangles[t + 1]]);
                            }
                            if (vert_table[triangles[t + 2]] != -1)
                                cell_tri.Add(vert_table[triangles[t + 2]]);
                            else
                            {
                                int v = triangles[t + 2];
                                cell_vert.Add(vertices[v]);
                                cell_uv.Add(mesh.uv[v]);
                                cell_uv2.Add(mesh.uv2[v]);
                                cell_normal.Add(mesh.normals[v]);
                                vert_table[v] = cell_vert.Count - 1;
                                cell_tri.Add(vert_table[triangles[t + 2]]);
                            }

                        }
                    }

                    new_mesh.vertices = cell_vert.ToArray();
                    new_mesh.triangles = cell_tri.ToArray();
                    new_mesh.uv = cell_uv.ToArray();
                    new_mesh.uv2 = cell_uv2.ToArray();
                    new_mesh.normals = cell_normal.ToArray();
                    cells.Add(new_mesh);
                }

        int idx = 0;
        foreach (var m in cells)
        {
            GameObject g = new GameObject(gameObject.name + '_' + ++idx);
            MeshFilter mf = g.AddComponent<MeshFilter>();
            mf.mesh = m;

            MeshRenderer mr = g.AddComponent<MeshRenderer>();
            mr.material = rend.sharedMaterial;

            //g.AddComponent<DebugShowCulling>();


            if (GetComponent<MeshCollider>())
            {
                g.AddComponent<MeshCollider>();
            }

            if (GetComponent<Ground>())
            {
                var a = g.AddComponent<Ground>();
                var d = GetComponent<Ground>();

                a.GI = d.GI;
            }

            g.transform.parent = parent.transform;
            g.transform.localPosition = Vector3.zero;
            g.transform.localRotation = Quaternion.identity;
            g.transform.localScale = Vector3.one;
        }

        this.gameObject.SetActive(false);
        cells.Clear();
    }

}
