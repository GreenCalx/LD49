using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class UseDepthMask : MonoBehaviour
{ 
    public Camera cam;
	public Material mat;
	public MeshRenderer mesh;
    private void Start() {
		CommandBuffer buf = new CommandBuffer();
		buf.GetTemporaryRT(Shader.PropertyToID("_DepthMaskTexture"), 1920, 1080);
		buf.Blit(BuiltinRenderTextureType.GBuffer1, new RenderTargetIdentifier("_DepthMaskTexture"));
		buf.SetRenderTarget(new RenderTargetIdentifier[4]{ 
		BuiltinRenderTextureType.GBuffer0, BuiltinRenderTextureType.GBuffer1, 
		BuiltinRenderTextureType.GBuffer2, BuiltinRenderTextureType.CameraTarget}, BuiltinRenderTextureType.CameraTarget);
		buf.DrawRenderer(mesh, mat, 0, 0);
		buf.ReleaseTemporaryRT(Shader.PropertyToID("_DepthMaskTexture"));
		
		cam.AddCommandBuffer(CameraEvent.AfterGBuffer, buf);
	}
}
