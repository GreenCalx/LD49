using UnityEngine;
using System.Collections.Generic;

public class TestUpdateMeshCollider : MonoBehaviour
{
    public float MovingSandsRadius;
    public float Depth;
    public float ForceMultiplier;

    public List<GameObject> SandPositions;
    // Start is called before the first frame update
    private MeshCollider MC;
    private Mesh M;
    private Mesh Copy;
    void Start()
    {
        M = GetComponent<MeshFilter>().mesh;
        Copy = Object.Instantiate(M);
        GetComponent<MeshFilter>().mesh = Copy;
    }

    // Update is called once per frame
    void Update()
    {
        MC = GetComponent<MeshCollider>();

        var newVertices = M.vertices;
        for (var i = 0; i < newVertices.Length; ++i)
        {
            var v = transform.TransformPoint(newVertices[i]);
            foreach (var Sand in SandPositions)
            {
                var D = Vector2.Distance( new Vector2(v.x, v.z), new Vector2(Sand.transform.position.x, Sand.transform.position.z)) / MovingSandsRadius;
                if (D <= 1)
                {
                    newVertices[i] += Depth * transform.TransformDirection(Vector3.up) * (1 - D);
                }
            }
        }

        Copy.SetVertices(newVertices);

        MC.sharedMesh = null;
        MC.sharedMesh = Copy;
    }
}
