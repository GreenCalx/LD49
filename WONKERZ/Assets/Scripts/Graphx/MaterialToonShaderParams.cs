using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/SchnibbleToonMaterial", order = 1)]
public class MaterialToonShaderParams : ScriptableObject
{
    public struct GPUParams
    {
        public float outlineWidth;
        public int materialID;
    }
    GPUParams gpuParams;
    Material material;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
