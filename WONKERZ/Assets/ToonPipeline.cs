using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ToonPipeline : MonoBehaviour
{
	public MaterialManager mgr;
	Material outlineExtract;
	
	private RenderTexture lightPassTex;
	public RenderTexture outlines;
	// double buffering to make possible read/write
	private RenderTexture outlines_2;
	
    // Start is called before the first frame update
    void Start()
    {
		Shader.SetGlobalBuffer("_SchnibbleMaterialData", mgr.materialsGPUBuffer);
		Shader.SetGlobalTexture("_SchnibbleMaterialData_TexturesRGB",mgr.materialsGPUTextures_RGB);
		Shader.SetGlobalTexture("_SchnibbleMaterialData_TexturesR8", mgr.materialsGPUTextures_R8);
	    
		var cam = GetComponent<Camera>();
		cam.depthTextureMode = cam.depthTextureMode | DepthTextureMode.DepthNormals;
		
		lightPassTex = new RenderTexture(outlines);
		outlines_2 = new RenderTexture(outlines);
	
		CommandBuffer cmdExtractOutline = new CommandBuffer();
		
		cmdExtractOutline.name = "extractOutline";
		Material outlineExtract = new Material(Shader.Find("Custom/Outlines"));
		
		Matrix4x4 projectionMatrix = cam.projectionMatrix;
        projectionMatrix = GL.GetGPUProjectionMatrix(projectionMatrix, false);
        projectionMatrix.SetColumn(1, projectionMatrix.GetColumn(1) * -1);

        Matrix4x4 viewMatrix = cam.worldToCameraMatrix;

        outlineExtract.SetMatrix("_UNITY_MATRIX_I_V", viewMatrix.inverse);
        outlineExtract.SetMatrix("_UNITY_MATRIX_I_P", projectionMatrix.inverse);
        outlineExtract.SetMatrix("_UNITY_MATRIX_I_VP", (projectionMatrix * viewMatrix).inverse);
		
        cmdExtractOutline.Blit(BuiltinRenderTextureType.CameraTarget, outlines, outlineExtract, 0);
		cmdExtractOutline.Blit(outlines, outlines_2, outlineExtract, 1);
		cmdExtractOutline.Blit(outlines_2, outlines, outlineExtract, 2);
		//cmdExtractOutline.Blit(outlines, outlines_2, outlineExtract, 3);
		
		cam.AddCommandBuffer(CameraEvent.BeforeReflections, cmdExtractOutline);
		
		CommandBuffer cmdLightFinalPass = new CommandBuffer();
		
		cmdLightFinalPass.name = "lightFinalPass";
		Material lightFinalPass = new Material(Shader.Find("Custom/Schnibble-DeferredToonShading"));
        Material blitPass = new Material(Shader.Find("Hidden/BlitCopy"));	
	    cmdLightFinalPass.Blit(BuiltinRenderTextureType.CameraTarget, lightPassTex, lightFinalPass, 2 );
		cmdLightFinalPass.Blit(lightPassTex, BuiltinRenderTextureType.CameraTarget, blitPass);
		cam.AddCommandBuffer(CameraEvent.AfterLighting, cmdLightFinalPass);
    }
	
	private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
		Material merge = new Material(Shader.Find("Custom/Merge"));
		merge.SetTexture("_LightPass", lightPassTex);
		merge.SetTexture("_Outlines", outlines);
        Graphics.Blit(source, destination, merge);
    }
}
