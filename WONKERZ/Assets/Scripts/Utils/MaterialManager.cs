using UnityEngine;
using System.Collections.Generic;

public class MaterialManager : MonoBehaviour
{
    public List<MaterialToonShaderParams> materials = new List<MaterialToonShaderParams>();

    public ComputeBuffer materialsGPUBuffer;
	[HideInInspector]
	public Texture2DArray materialsGPUTextures_R8;
    [HideInInspector]
	public Texture2DArray materialsGPUTextures_RGB;

    const int maxDepthPerFrag = 8;
    const int oitFragDepthStride = 5 * sizeof(float) + 1 * sizeof(uint);
    const int oitFragHeadIdxStride = 1 * sizeof(uint);
    public ComputeBuffer oitFragDepth;
    public ComputeBuffer oitFragHeadIdx;
	
    private void Awake()
    {
		if (materials.Count == 0) {
		   return;
		}
		   
        MaterialToonShaderParams.SchnibbleCustomGPUData[] gpuData = new MaterialToonShaderParams.SchnibbleCustomGPUData[materials.Count];
		materialsGPUTextures_R8 = new Texture2DArray(MaterialToonShaderParams.SchTextureW, MaterialToonShaderParams.SchTextureH, materials.Count * MaterialToonShaderParams.SchTextureR8Count, materials[0].diffuseBRDF.format, false, true);
        materialsGPUTextures_R8.wrapMode = TextureWrapMode.Clamp;
		materialsGPUTextures_RGB = new Texture2DArray(MaterialToonShaderParams.SchTextureW, MaterialToonShaderParams.SchTextureH, materials.Count * MaterialToonShaderParams.SchTextureRGBCount, materials[0].toonRamp.format, false, true);
        materialsGPUTextures_RGB.wrapMode = TextureWrapMode.Clamp;
		for (int i = 0; i < materials.Count; ++i)
        {
			var currentMaterial = materials[i];
            gpuData[i] = currentMaterial.gpuParams;
			
			materialsGPUTextures_R8.SetPixels(currentMaterial.diffuseRamp.GetPixels(0), i * MaterialToonShaderParams.SchTextureR8Count + MaterialToonShaderParams.SchTextureDiffuseRampOffset, 0);
			materialsGPUTextures_R8.SetPixels(currentMaterial.diffuseBRDF.GetPixels(0), i * MaterialToonShaderParams.SchTextureR8Count + MaterialToonShaderParams.SchTextureDiffuseBRDFOffset, 0);
			materialsGPUTextures_R8.SetPixels(currentMaterial.outlineRamp.GetPixels(0), i * MaterialToonShaderParams.SchTextureR8Count + MaterialToonShaderParams.SchTextureOutlineRampOffset, 0);
			materialsGPUTextures_RGB.SetPixels(currentMaterial.toonRamp.GetPixels(0), i * MaterialToonShaderParams.SchTextureRGBCount + MaterialToonShaderParams.SchTextureToonRampOffset, 0);
        }

        materialsGPUBuffer = new ComputeBuffer(gpuData.Length, MaterialToonShaderParams.SchnibbleCustomGPUDataGPUStride, ComputeBufferType.Structured, ComputeBufferMode.Immutable);
        materialsGPUBuffer.SetData(gpuData);
		
		materialsGPUTextures_R8.Apply();
		materialsGPUTextures_RGB.Apply();

        oitFragDepth = new ComputeBuffer(Screen.width * Screen.height * maxDepthPerFrag, oitFragDepthStride, ComputeBufferType.Counter);
        oitFragHeadIdx = new ComputeBuffer(Screen.width*Screen.height, oitFragHeadIdxStride, ComputeBufferType.Structured, ComputeBufferMode.Immutable);
    }

    private void OnDestroy()
    {
        if(materialsGPUBuffer != null) materialsGPUBuffer.Release();
        if(oitFragDepth != null) oitFragDepth.Release();
        if(oitFragHeadIdx != null) oitFragHeadIdx.Release();
    }
}
