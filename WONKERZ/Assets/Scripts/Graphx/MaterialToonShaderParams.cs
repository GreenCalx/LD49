using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/SchnibbleToonMaterial", order = 1)]
public class MaterialToonShaderParams : ScriptableObject
{
	[System.Serializable]
	public struct SchnibbleCustomOutline {
    public Color lightColor;
	public Color shadowColor;
	public float  threshold;
	public float  width;
	public int    enableDistanceBlend;
	public float  fadeStart;
	public Color distanceColor;
    };
public static int SchnibbleCustomOutlineGPUStride = (sizeof(float) * 4) * 3
                                                     + sizeof(float) * 3
													 + sizeof(int) * 1;

    [System.Serializable]
public struct SchnibbleCustomGPUData {
    public SchnibbleCustomOutline outlineDepth;
	public int enableOutlineDepth;
	public SchnibbleCustomOutline outlineNormal;
	public int enableOutlineNormal;
	public SchnibbleCustomOutline outlineColor;
	public int enableOutlineColor;
};

public static int SchnibbleCustomGPUDataGPUStride = SchnibbleCustomOutlineGPUStride * 3 
                                                    + sizeof(int) * 3;
			public SchnibbleCustomGPUData gpuParams;	

public static int SchTextureW =256;
public static int SchTextureH =256;
// r8 textures offset	
public static int SchTextureR8Count = 3;
public static int SchTextureDiffuseRampOffset =0;
public Texture2D diffuseRamp;
public static int SchTextureDiffuseBRDFOffset =1;
public Texture2D diffuseBRDF;
public static int SchTextureOutlineRampOffset =2;
public Texture2D outlineRamp;
// rgb textures offset
public static int SchTextureRGBCount = 1;
public static int SchTextureToonRampOffset =0;
public Texture2D toonRamp;
	
}
