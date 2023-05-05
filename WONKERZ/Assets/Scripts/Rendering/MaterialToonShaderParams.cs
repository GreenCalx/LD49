using System.Collections;

using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/SchnibbleToonMaterial", order = 1)]
public class MaterialToonShaderParams : ScriptableObject
{
    [System.Serializable]
    public struct SchnibbleCustomOutline
    {
        public Color lightColor;
        public Color shadowColor;
        public float threshold;
        public float width;
        public int enableDistanceBlend;
        public float fadeStart;
        public Color distanceColor;
    };
    public static int SchnibbleCustomOutlineGPUStride = (sizeof(float) * 4) * 3
                                                        + sizeof(float) * 3
                                                        + sizeof(int) * 1;
    [System.Serializable]
    public struct SchnibbleCrossHatchData
    {
        public float maxSize;
        public float minSize;
        public float spaceSize;
        public float lightPower;
        public float blendPower;
    };
    public static int SchnibbleCrossHatchDataGPUStride = (sizeof(float) * 5);

    [System.Serializable]
    public struct SchnibbleCustomGPUData
    {
        public SchnibbleCustomOutline outlineDepth;
        public int enableOutlineDepth;
        public SchnibbleCustomOutline outlineNormal;
        public int enableOutlineNormal;
        public SchnibbleCustomOutline outlineColor;
        public int enableOutlineColor;
        public SchnibbleCrossHatchData hatch;
        public int enableCrossHatch;
        // use color or uniform textures for ramp, etc...
        public int useColoredTexture;
    };

    public static int SchnibbleCustomGPUDataGPUStride = SchnibbleCustomOutlineGPUStride * 3
                                                        + SchnibbleCrossHatchDataGPUStride * 1
                                                        + sizeof(int) * 5;
    public SchnibbleCustomGPUData gpuParams;

    public static int SchTextureW = 256;
    public static int SchTextureH = 256;
    // r8 textures offset
    public static int SchTextureR8Count = 3;
    public static int SchTextureDiffuseRampOffset = 0;
    public static int SchTextureDiffuseBRDFOffset = 1;
    public static int SchTextureOutlineRampOffset = 2;
    public Texture2D outlineRamp;
    // rgb textures offset
    public static int SchTextureRGBCount = 1;
    public static int SchTextureToonRampOffset = 0;


    // use to define the 1d toonramp
    // material manager is responsible to transfer this data to the gpu
    public Gradient toonRamp = new Gradient();
    public Texture2D toonRampColored;

    // if non-null will be used
    public Texture2D diffuseRampColored;
    public Texture2D diffuseBRDF;
    public Texture2D diffuseBRDFColored;
    public enum DiffuseBRDFType
    {
        Lambert,
        HalfLambert,
        Custom
    };
    public DiffuseBRDFType brdfType = DiffuseBRDFType.Lambert;

    [System.NonSerialized]
    public bool needUpdateOnGPU = true;
    public void OnValidate()
    {
        needUpdateOnGPU = true;
    }
}
