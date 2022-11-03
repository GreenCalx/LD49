using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeshDeform : MonoBehaviour
{
    public string cloneName = "clone";
    public bool normalsForQuadMesh = false;
    MeshCollider meshCollider;

    Mesh originalMesh;
    Mesh clonedMesh;
    MeshFilter meshFilter;

    [HideInInspector]
    public int targetIndex;

    [HideInInspector]
    public Vector3 targetVertex;

    [HideInInspector]
    public Vector3[] originalVertices;

    [HideInInspector]
    public Vector3[] modifiedVertices;

    [HideInInspector]
    public Vector3[] normals;

    public bool isEditMode = true;
    public List<int> selectedIndices = new List<int>();
    public float pickSize = 0.01f;

    public float radiusOfEffect = 0.3f;
    public float pullValue = 0.3f;
    public float duration = 0.5f;
    int currentIndex = 0;
    bool isAnimate = false;
    float startTime = 0f;
    float runTime = 0f;

    void Start()
    {
        Init();
    }

    public void Init()
    {
        meshFilter = GetComponent<MeshFilter>();

        currentIndex = 0;

        if (isEditMode)
        {
            originalMesh = meshFilter.sharedMesh;
            clonedMesh = new Mesh();
            clonedMesh.name = cloneName;
            clonedMesh.vertices = originalMesh.vertices;
            clonedMesh.triangles = originalMesh.triangles;
            clonedMesh.normals = originalMesh.normals;
            clonedMesh.RecalculateBounds();
            clonedMesh.RecalculateNormals();
            clonedMesh.RecalculateTangents();

            meshFilter.mesh = clonedMesh;

            originalVertices = clonedMesh.vertices;
            normals = clonedMesh.normals;

            Debug.Log("Init & Cloned");
        }
        else
        {
            originalMesh = meshFilter.mesh;
            originalVertices = originalMesh.vertices;
            normals = originalMesh.normals;

            modifiedVertices = new Vector3[originalVertices.Length];
            for (int i = 0; i < originalVertices.Length; i++)
            {
                modifiedVertices[i] = originalVertices[i];
            }

            StartDisplacement();
        }

    }

    public void StartDisplacement()
    {
        targetVertex = originalVertices[selectedIndices[currentIndex]];
        startTime = Time.time;
        isAnimate = true;
    }

    public void StartDisplacement(Vector3 iTargetVertex)
    {
        targetVertex = iTargetVertex;
        startTime = Time.time;
        isAnimate = true;    
    }

    protected void FixedUpdate()
    {
        if (!isAnimate)
            return;

        runTime = Time.time - startTime;

        if (runTime < duration)
        {
            Vector3 targetVertexPos = meshFilter.transform.InverseTransformPoint(targetVertex);
            DisplaceVertices(targetVertexPos, pullValue, radiusOfEffect);
        }
        else
        {
            currentIndex++;
            if (currentIndex < selectedIndices.Count)
            {
                StartDisplacement();
            }
            else
            {
                originalMesh = GetComponent<MeshFilter>().mesh;
                isAnimate = false;
            }
        }
    }

    private void UpdateCollider()
    {
        if (meshCollider==null)
            meshCollider = GetComponent<MeshCollider>();
        meshCollider.sharedMesh = originalMesh;
    }

    void DisplaceVertices(Vector3 targetVertexPos, float force, float radius)
    {
        Vector3 currentVertexPos = Vector3.zero;
        float sqrRadius = radius * radius;

        for (int i = 0; i < modifiedVertices.Length; i++)
        {
            currentVertexPos = modifiedVertices[i];
            float sqrMagnitude = ((currentVertexPos - targetVertexPos)-transform.position).sqrMagnitude;
            if (sqrMagnitude > sqrRadius)
            {
                continue;
            }
            float distance = Mathf.Sqrt(sqrMagnitude);
            float falloff = GaussFalloff(distance, radius);
            Vector3 translate = (currentVertexPos * force) * falloff;
            translate.z = 0f;
            translate.x = 0f;
            // Somehow the following line fucks up translation at TRS.
            // Thus, this deform is for non-rotated objects only.
            //Quaternion rotation = Quaternion.Euler(translate);
            Quaternion rotation = Quaternion.identity;

            Matrix4x4 m = Matrix4x4.TRS(translate, rotation, Vector3.one);
            modifiedVertices[i] = m.MultiplyPoint3x4(currentVertexPos);
        }
        originalMesh.vertices = modifiedVertices;
        originalMesh.RecalculateNormals();
        originalMesh.RecalculateTangents();

        UpdateCollider();
    }

    public void ClearAllData()
    {
        selectedIndices = new List<int>();
        targetIndex = 0;
        targetVertex = Vector3.zero;
    }

    private float GaussFalloff(float dist, float inRadius)
    {
        return Mathf.Clamp01(Mathf.Pow(360, -Mathf.Pow(dist / inRadius, 2.5f) - 0.01f));
    }

}
