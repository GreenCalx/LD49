using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class DecalRenderer : MonoBehaviour
{
    public Mesh cubeMesh;
    public Mesh fillDepthMesh;
    public Material mat;
    public Material matDepth;
    public Camera cam;
    // Start is called before the first frame update
    void Start()
    {
        CommandBuffer cmd = new CommandBuffer();

        int normalsid = Shader.PropertyToID("_GBufferNormals");
		cmd.GetTemporaryRT(normalsid, -1, -1, 0, FilterMode.Point, RenderTextureFormat.ARGB2101010, RenderTextureReadWrite.Linear, 1, false);

		cmd.Blit(BuiltinRenderTextureType.GBuffer2, normalsid);

        int depthid = Shader.PropertyToID("_GBufferDepth");
		cmd.GetTemporaryRT(depthid, -1, -1);
		cmd.Blit (Graphics.activeDepthBuffer, depthid);

		cmd.SetRenderTarget(new RenderTargetIdentifier[4]{
		BuiltinRenderTextureType.GBuffer0, BuiltinRenderTextureType.GBuffer1,
		BuiltinRenderTextureType.GBuffer2, BuiltinRenderTextureType.CameraTarget}, BuiltinRenderTextureType.CameraTarget);
        cmd.DrawMesh(cubeMesh, transform.localToWorldMatrix, mat, 0, 0);

if (fillDepthMesh != null && matDepth != null) {
cmd.GetTemporaryRT(Shader.PropertyToID("_DepthMaskTexture"), -1, -1);
cmd.Blit(BuiltinRenderTextureType.GBuffer1, new RenderTargetIdentifier("_DepthMaskTexture"));
cmd.SetRenderTarget(new RenderTargetIdentifier[4]{
BuiltinRenderTextureType.GBuffer0, BuiltinRenderTextureType.GBuffer1,
BuiltinRenderTextureType.GBuffer2, BuiltinRenderTextureType.CameraTarget}, BuiltinRenderTextureType.CameraTarget);

cmd.DrawMesh(fillDepthMesh, transform.localToWorldMatrix, matDepth, 0, 0);
cmd.ReleaseTemporaryRT(Shader.PropertyToID("_DepthMaskTexture"));
        }

cmd.ReleaseTemporaryRT(normalsid);
cmd.ReleaseTemporaryRT(depthid);

        cam.AddCommandBuffer(CameraEvent.BeforeLighting, cmd);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
