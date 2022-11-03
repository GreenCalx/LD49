using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(MeshDeform))]
public class MeshDeformInspector : Editor
{
    private MeshDeform mesh;
    private Transform handleTransform;
    private Quaternion handleRotation;

    void OnSceneGUI()
    {
        mesh = target as MeshDeform;
        handleTransform = mesh.transform;
        handleRotation = Tools.pivotRotation == PivotRotation.Local ? handleTransform.rotation : Quaternion.identity;

        if (mesh.isEditMode)
        {
            if (mesh.originalVertices == null || mesh.normals.Length == 0)
            {
                mesh.Init();
            }
            for (int i = 0; i < mesh.originalVertices.Length; i++)
            {
                ShowHandle(i);
            }
        }
    }

    void ShowHandle(int index)
    {
        Vector3 point = handleTransform.TransformPoint(mesh.originalVertices[index]);

        // unselected vertex
        if (!mesh.selectedIndices.Contains(index))
        {
            Handles.color = Color.blue;
            if (Handles.Button(point, handleRotation, mesh.pickSize, mesh.pickSize, Handles.DotHandleCap))
            {
                mesh.selectedIndices.Add(index);
            }
        }
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        mesh = target as MeshDeform;

        if (mesh.isEditMode)
        {
            if (GUILayout.Button("Show Normals"))
            {
                Vector3[] verts = mesh.modifiedVertices.Length == 0 ? mesh.originalVertices : mesh.modifiedVertices;
                Vector3[] normals = mesh.normals;
                for (int i = 0; i < verts.Length; i++)
                {
                    //if (mesh.normalsForQuadMesh)
                    //    Debug.DrawLine(handleTransform.TransformPoint(verts[i]), handleTransform.TransformPoint(normals[i]) + handleTransform.TransformPoint(verts[i]), Color.green, 10.0f, true);
                    //else
                        Debug.DrawLine(handleTransform.TransformPoint(verts[i]), handleTransform.TransformPoint(normals[i]), Color.green, 10.0f, true);
                }
            }

            if (GUILayout.Button("Force Init"))
            {
                mesh.Init();
            }
        }

        if (GUILayout.Button("Clear Selected Vertices"))
        {
            mesh.ClearAllData();
        }
    }
}
