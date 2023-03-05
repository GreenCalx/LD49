using System.Collections;

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/SchnibbleToonMaterialCollection", order = 1)]
public class MaterialCollection : ScriptableObject
{
    private static readonly int size = 256;

    public MaterialToonShaderParams[] materials = new MaterialToonShaderParams[size];

    public MaterialToonShaderParams GetMaterial(int id)
    {
        System.Diagnostics.Debug.Assert(id > 0 && id < size);
        return materials[id];
    }

    public MaterialToonShaderParams GetMaterial_NonNull(int id)
    {
        System.Diagnostics.Debug.Assert(id >= 0 && id < size);
        if (materials[id] == null)
        {
            materials[id] = ScriptableObject.CreateInstance<MaterialToonShaderParams>() as MaterialToonShaderParams;
            AssetDatabase.CreateAsset(materials[id], AssetDatabase.GetAssetPath(this).Replace(".asset","") + "Material" + id.ToString() + ".asset");
            materials[id].toonRamp.mode = GradientMode.Fixed;
        }
        return materials[id];
    }

}
