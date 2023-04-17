using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ToonPipeline : MonoBehaviour
{
	public MaterialManager mgr;
	Material outlineExtract;
    public ComputeShader clearOITBufferCompute;
	
	private RenderTexture lightPassTex;
	public RenderTexture outlines;
	// double buffering to make possible read/write
	private RenderTexture outlines_2;
	
	public Light mainLight;
	
    // Start is called before the first frame update
    void Start()
    {
		Shader.SetGlobalBuffer("_SchnibbleMaterialData", mgr.materialsGPUBuffer);
		Shader.SetGlobalTexture("_SchnibbleMaterialData_TexturesRGB",mgr.materialsGPUTextures_RGB);
		Shader.SetGlobalTexture("_SchnibbleMaterialData_TexturesR8", mgr.materialsGPUTextures_R8);
        //Shader.SetGlobalBuffer("_oitFragDepth", mgr.oitFragDepth);
        Shader.SetGlobalBuffer("_oitFragHeadIdx", mgr.oitFragHeadIdx);
	    

		var cam = GetComponent<Camera>();
		cam.depthTextureMode = cam.depthTextureMode | DepthTextureMode.DepthNormals;
        cam.clearStencilAfterLightingPass = true;

		lightPassTex = new RenderTexture(outlines);
		outlines_2 = new RenderTexture(outlines);
	
		CommandBuffer cmdExtractOutline = new CommandBuffer();
		cmdExtractOutline.name = "extractOutline";
		Material outlineExtract = new Material(Shader.Find("Custom/Outlines"));
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
        Material discardAlpha = new Material(Shader.Find("Hidden/DiscardAlpha"));
        //cmdLightFinalPass.Blit(null, BuiltinRenderTextureType.CameraTarget, discardAlpha);
		cam.AddCommandBuffer(CameraEvent.AfterLighting, cmdLightFinalPass);

        CommandBuffer cmdClearOITBufferBeforeForward = new CommandBuffer();
        cmdClearOITBufferBeforeForward.SetComputeBufferParam(clearOITBufferCompute, 0, "_oitFragHeadIdx", mgr.oitFragHeadIdx);
        cmdClearOITBufferBeforeForward.SetComputeIntParam(clearOITBufferCompute, "screenWidth", Screen.width);
        cmdClearOITBufferBeforeForward.DispatchCompute(clearOITBufferCompute, 0, Mathf.CeilToInt(Screen.width / 32.0f), Mathf.CeilToInt(Screen.height / 32.0f), 1);
        cmdClearOITBufferBeforeForward.ClearRandomWriteTargets();
        cmdClearOITBufferBeforeForward.SetRandomWriteTarget(1, mgr.oitFragDepth, true);
        cmdClearOITBufferBeforeForward.SetRandomWriteTarget(2, mgr.oitFragHeadIdx, true);
        cam.AddCommandBuffer(CameraEvent.BeforeForwardAlpha, cmdClearOITBufferBeforeForward);


        CommandBuffer cmdApplyOIT = new CommandBuffer();
        Material oitMat = new Material(Shader.Find("Custom/OITBlit"));
		cmdApplyOIT.GetTemporaryRT(Shader.PropertyToID("_MainTex"), Screen.width, Screen.height);
        cmdApplyOIT.Blit(BuiltinRenderTextureType.CameraTarget, new RenderTargetIdentifier("_MainTex"));
        cmdApplyOIT.Blit(new RenderTargetIdentifier("_MainTex"), BuiltinRenderTextureType.CameraTarget, oitMat);
		cmdApplyOIT.ReleaseTemporaryRT(Shader.PropertyToID("_MainTex"));
        cmdApplyOIT.ClearRandomWriteTargets();
        cam.AddCommandBuffer(CameraEvent.AfterForwardAlpha, cmdApplyOIT);

    }
	
	private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {

		var cam = GetComponent<Camera>();
		
		Matrix4x4 projectionMatrix = cam.projectionMatrix;
        projectionMatrix = GL.GetGPUProjectionMatrix(projectionMatrix, false);
        projectionMatrix.SetColumn(1, projectionMatrix.GetColumn(1) * -1);
		
		Matrix4x4 viewMatrix = cam.worldToCameraMatrix;

		
		Material merge = new Material(Shader.Find("Custom/Merge"));
		merge.SetTexture("_LightPass", lightPassTex);
		merge.SetTexture("_Outlines", outlines);
		merge.SetVector("_LightDir", mainLight.transform.forward);
		merge.SetMatrix("_UNITY_MATRIX_I_V", viewMatrix.inverse);
        Graphics.Blit(source, destination, merge);

    }

    public void Update(){
        mgr.oitFragDepth.SetCounterValue(0);
    }
}
