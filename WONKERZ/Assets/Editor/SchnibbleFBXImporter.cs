using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

using Schnibble;

public class SchnibbleFBXImporter : AssetPostprocessor
{
    #pragma warning disable CS0414
    readonly static string kCollider           = "_collider";
    readonly static string kColliderMesh       = "_collider_mesh";
    readonly static string kColliderBox        = "_collider_box";
    readonly static string kColliderSphere     = "_collider_sphere";
    readonly static string kColliderCapsule    = "_collider_capsule";
    readonly static string kColliderMeshConvex = "_convex";
    readonly static string kNoMesh             = "_nomesh";
    #pragma warning restore CS0414

    readonly static string defaultSchnibbleErrorMaterial = "Materials/Sch-ErrorShader";

    void OnPreprocessModel()
    {
        ModelImporter importer = assetImporter as ModelImporter;

        // Scene

        // units, scale, axis
        importer.globalScale = 1f;
        importer.useFileUnits = false;
        importer.bakeAxisConversion = true;
        // what to import
        importer.importBlendShapes = true;
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

    Material OnAssignMaterialModel(Material source, Renderer rend)
    {
        var unityMaterials = AssetDatabase.FindAssets(source.name + " t:material");
        if (unityMaterials.Length != 0)
        {
            var mat = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(unityMaterials[0]));
            if (unityMaterials.Length > 1)
            {
                this.LogWarning("[Material] Found more than one material with name containing : " + source.name + " => using first possible one : " + mat.name);
            }
            return mat;
        }

        var defaultMat = Resources.Load<Material>(defaultSchnibbleErrorMaterial);
        if (defaultMat == null)
        {
            this.LogError("[Material] Default material not found, will use StandardMaterial");
        }
        else
        {
            this.LogWarning(" [Material] Material will be default one instead of : " + source.name);
        }

        return defaultMat;
    }

    readonly static string kPropertyCollider = "unity_collider";
    readonly static string kPropertyShowMesh = "unity_showMesh";
    void OnPostprocessModel(GameObject go)
    {
        //RecursiveAddColliders(go.transform);
        string[] extraUserPropertyNames =
            {
                kPropertyCollider,
                kPropertyShowMesh,
            };
        ((ModelImporter)assetImporter).extraUserProperties = extraUserPropertyNames;
    }

    private void OnPostprocessGameObjectWithUserProperties(GameObject go, string[] propNames, object[] values)
    {
        var idx = 0;
        foreach (var propName in propNames)
        {
            if (propName == kPropertyCollider)
            {

                var property = (string)values[idx];
                if (property.Contains("mesh"))
                {
                    var c = go.AddComponent<MeshCollider>();
                    c.convex = property.Contains("convex");
                }
                else if (property.Contains("box"))
                {
                    var c = go.AddComponent<BoxCollider>();
                }
                else if (property.Contains("sphere"))
                {
                    var c = go.AddComponent<SphereCollider>();
                }
            }

            else if (propName == kPropertyShowMesh)
            {
                var val = (int)values[idx];


                if (val == 0)
                {
                    var mr = go.GetComponent<MeshRenderer>();
                    if (!mr) continue;
                    GameObject.DestroyImmediate(mr);

                    var mf = go.GetComponent<MeshFilter>();
                    if (!mf) continue;
                    GameObject.DestroyImmediate(mf);
                }
            }
            idx++;
        }
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
        if (name.Contains(kColliderMesh))
        {
            var c = t.gameObject.AddComponent<MeshCollider>();
            c.convex = name.Contains(kColliderMeshConvex);
        }
        else if (name.Contains(kColliderBox))
        {
            var c = t.gameObject.AddComponent<BoxCollider>();
        }
        else if (name.Contains(kColliderSphere))
        {
            var c = t.gameObject.AddComponent<SphereCollider>();
        }

        if (name.Contains(kNoMesh))
        {
            mr.enabled = false;
        }
    }

    // remove every parametric string in the fbx filename
    static string CleanUpFilename(string filename)
    {
        //var name = filename.ToLower();
        var name = filename;

        name = name.Replace(kColliderMesh, "");
        name = name.Replace(kColliderMeshConvex, "");
        name = name.Replace(kColliderBox, "");
        name = name.Replace(kColliderSphere, "");
        name = name.Replace(kNoMesh, "");

        return name;
    }

    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
    {
        foreach (string str in importedAssets)
        {
            // only apply to fbx files
            if (str.EndsWith("fbx"))
            {
                var filepath = System.IO.Path.GetDirectoryName(str);
                var prefabname = CleanUpFilename(System.IO.Path.GetFileName(str)).Replace(".fbx", ".prefab");
                var prefabfullpath = System.IO.Path.Join(filepath, prefabname);
                // try to find corresponding prefab
                if (!System.IO.File.Exists(prefabfullpath))
                {
                    // prefab does not exists, create one
                    var fbx = (GameObject)AssetDatabase.LoadAssetAtPath(str, typeof(GameObject));
                    if (fbx != null)
                    {
                        var prefab = (GameObject)PrefabUtility.InstantiatePrefab(fbx);

                        var go = new GameObject();
                        prefab.transform.parent = go.transform;
                        // make the parent selected in editor by default instead of the fbx!
                        go.AddComponent<Selectable>();

                        PrefabUtility.SaveAsPrefabAssetAndConnect(go, prefabfullpath, InteractionMode.AutomatedAction, out bool success);
                        if (!success)
                        {
                            SchLog.LogError("Could not create prefab from fbx");
                        }
                        GameObject.DestroyImmediate(go);
                    }
                    else
                    {
                        SchLog.LogError("Cannot find FBX file.");
                    }
                }
            }
        }
        foreach (string str in deletedAssets)
        {
            SchLog.Log("Deleted Asset: " + str);
        }

        for (int i = 0; i < movedAssets.Length; i++)
        {
            SchLog.Log("Moved Asset: " + movedAssets[i] + " from: " + movedFromAssetPaths[i]);
        }

        if (didDomainReload)
        {
            SchLog.Log("Domain has been reloaded");
        }
    }
}
