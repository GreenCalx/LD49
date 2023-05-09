using System.Collections;

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/SchnibbleToonMaterialCollection", order = 1)]
public class MaterialCollection : ScriptableObject
{
    public static readonly int size = 256;

    public MaterialToonShaderParams[] materials = new MaterialToonShaderParams[size];

    public MaterialToonShaderParams GetMaterial(int id)
    {
        //System.Diagnostics.Debug.Assert(id > 0 && id < size);
        return materials[id];
    }

    public MaterialToonShaderParams GetMaterial_NonNull(int id)
    {
        #if UNITY_EDITOR
        System.Diagnostics.Debug.Assert(id >= 0 && id < size);
        if (materials[id] == null)
        {
            // quick fix: dunno why but the collection sometimes remove refs to the scriptableobjects.
            // this function checks that we did not already created a material with the same name as a quick fix.
            // TODO: Remove quick fix => find root cause and fix for good.
            var assetName = AssetDatabase.GetAssetPath(this).Replace(".asset","") + "Material" + id.ToString() + ".asset";
            if (AssetDatabase.FindAssets(assetName).Length == 0){
                materials[id] = ScriptableObject.CreateInstance<MaterialToonShaderParams>() as MaterialToonShaderParams;
                AssetDatabase.CreateAsset(materials[id], assetName);
                materials[id].toonRamp.mode = GradientMode.Fixed;
            } else {
                materials[id] = (MaterialToonShaderParams)AssetDatabase.LoadAssetAtPath(assetName, typeof(MaterialToonShaderParams));
            }
        }
        #endif
        return materials[id];
    }

}
