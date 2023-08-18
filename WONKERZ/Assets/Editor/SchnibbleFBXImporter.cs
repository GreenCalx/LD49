using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SchnibbleFBXImporter : AssetPostprocessor
{
    void OnPreprocessModel()
    {
        ModelImporter importer = assetImporter as ModelImporter;

        // Scene

        // units, scale, axis
        importer.globalScale = 1f;
        importer.useFileUnits = false;
        importer.bakeAxisConversion = true;
        // what to import
        importer.importBlendShapes = false;
        importer.importVisibility = false;
        importer.importCameras = false;
        importer.importLights = false;
        importer.preserveHierarchy = true;
        importer.sortHierarchyByName = true;

        // Meshes
        importer.meshCompression = ModelImporterMeshCompression.Medium;
        //importer.isReadable = false;
        importer.meshOptimizationFlags = MeshOptimizationFlags.Everything;
        // todo toffa: make a function to generate automatically collider data
        // according to some naming in blender?
        importer.addCollider = false;

        // Geometry
        importer.keepQuads = false;
        importer.weldVertices = true;
        importer.indexFormat = ModelImporterIndexFormat.Auto;
        //importer.legacyBlendShapeNormals = false;
        importer.importNormals = ModelImporterNormals.Import;
        // not used when importNormals = import
        importer.normalCalculationMode = ModelImporterNormalCalculationMode.AreaAndAngleWeighted;
        importer.normalSmoothingSource = ModelImporterNormalSmoothingSource.PreferSmoothingGroups;
        importer.normalSmoothingAngle = 60;
        // end not used
        importer.importTangents = ModelImporterTangents.CalculateMikk;
        importer.swapUVChannels = false;
        importer.generateSecondaryUV = false;

        // Animation
        importer.animationType = ModelImporterAnimationType.Generic;
        importer.avatarSetup = ModelImporterAvatarSetup.NoAvatar;
        importer.skinWeights = ModelImporterSkinWeights.Standard;
        importer.optimizeBones = true;
        importer.importConstraints = true;
        importer.importAnimation = true;

        // Materials
        // will call OnAssignMaterialModel and apply the error shader
        importer.materialImportMode = ModelImporterMaterialImportMode.None;
    }

    readonly static string defaultSchnibbleErrorMaterial = "Materials/Sch-ErrorShader";
    Material OnAssignMaterialModel(Material source, Renderer rend)
    {
        var m = Resources.Load<Material>(defaultSchnibbleErrorMaterial);
        if (m == null)
        {
            Debug.LogError("default material not found, will use StandardMaterial");
        }
        return m;
    }

    void OnPostprocessModel(GameObject go)
    {
        RecursiveAddColliders(go.transform);
    }

    void RecursiveAddColliders(Transform go)
    {

        AddCollider(go);
        foreach (Transform child in go)
        {
            AddCollider(child);
        }
    }

    void AddCollider(Transform t)
    {
        var mr = t.gameObject.GetComponent<MeshRenderer>();
        if (!mr) return;


        var name = t.name.ToLower();
        if (name.Contains("collider_mesh"))
        {
            var c = t.gameObject.AddComponent<MeshCollider>();
            c.convex = name.Contains("_convex");
        }
        else if (name.Contains("collider_mesh"))
        {
            t.gameObject.AddComponent<MeshCollider>();
        }
        else if (name.Contains("collider_box"))
        {
            var c = t.gameObject.AddComponent<BoxCollider>();
        }
        else if (name.Contains("collider_sphere"))
        {
            var c = t.gameObject.AddComponent<SphereCollider>();
        }

        if (name.Contains("_nomesh"))
        {
            mr.enabled = false;
        }
    }

    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
    {
        foreach (string str in importedAssets)
        {
            if (str.EndsWith("fbx"))
            {
                // try to find corresponding prefab
                if (!System.IO.File.Exists(str.Replace(".fbx", ".prefab")))
                {
                    // prefab does not exists, create one
                    var fbx = (GameObject)AssetDatabase.LoadAssetAtPath(str, typeof(GameObject));
                    var prefab = (GameObject)PrefabUtility.InstantiatePrefab(fbx);
                    var go = new GameObject();
                    prefab.transform.parent = go.transform;
                    if (fbx != null)
                    {
                        PrefabUtility.SaveAsPrefabAssetAndConnect(go, str.Replace(".fbx", ".prefab"), InteractionMode.AutomatedAction, out bool success);
                        if (!success)
                        {
                            Debug.LogError("Could not create prefab from fbx");
                        }
                    }
                    GameObject.DestroyImmediate(go);
                }
            }
        }
        foreach (string str in deletedAssets)
        {
            Debug.Log("Deleted Asset: " + str);
        }

        for (int i = 0; i < movedAssets.Length; i++)
        {
            Debug.Log("Moved Asset: " + movedAssets[i] + " from: " + movedFromAssetPaths[i]);
        }

        if (didDomainReload)
        {
            Debug.Log("Domain has been reloaded");
        }
    }
}
