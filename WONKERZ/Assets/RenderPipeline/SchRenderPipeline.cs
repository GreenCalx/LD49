using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class CameraRenderer
{
    ScriptableRenderContext context;
    Camera camera;
    ToonPipeline toonPipeline;

    CommandBuffer cmdBuf = new CommandBuffer();

    CullingResults cullingResults;
    static ShaderTagId gbufferPass = new ShaderTagId("Deferred");
    static ShaderTagId transparentPass = new ShaderTagId("Forward");
    static Material lightMaterial = new Material(Shader.Find("Custom/Schnibble-DeferredToonShading"));
    static Material blitDepth = new Material(Shader.Find("Hidden/SchFastDepthBlit"));
    static Material outlineExtract = new Material(Shader.Find("Custom/Outlines"));
    static Material merge = new Material(Shader.Find("Custom/Merge"));
    static Material oitMat = new Material(Shader.Find("Custom/OITBlit"));
    static Material screenSpaceShadows = new Material(Shader.Find("Hidden/Internal-ScreenSpaceShadows"));

    static ComputeShader clearOITBufferCompute;

    static string gbuffer0_str = "_CameraGBufferTexture0";
    static string gbuffer1_str = "_CameraGBufferTexture1";
    static string gbuffer2_str = "_CameraGBufferTexture2";
    static string gbuffer3_str = "_CameraGBufferTexture3";
    static string gbuffer_depth_str = "_GBufferDepth";
    static string depthTexture_str = "_CameraDepthTexture";
    static string outlines_temp_0_str = "_OutlineTemp0";
    static string outlines_temp_1_str = "_OutlineTemp1";
    static string lightPass_str = "_LightPass";
    static string oitPass_str = "_OitPass";
    static string dirShadowAtlas_str = "_DirShadowAtlas";
    static string shadowMapTexture_str = "_ShadowMapTexture";

    static int gbuffer0_id = Shader.PropertyToID(gbuffer0_str);
    static int gbuffer1_id = Shader.PropertyToID(gbuffer1_str);
    static int gbuffer2_id = Shader.PropertyToID(gbuffer2_str);
    static int gbuffer3_id = Shader.PropertyToID(gbuffer3_str);
    static int gbuffer_depth_id = Shader.PropertyToID(gbuffer_depth_str);
    static int depthTexture_id = Shader.PropertyToID(depthTexture_str);
    static int outlines_temp_0_id = Shader.PropertyToID(outlines_temp_0_str);
    static int outlines_temp_1_id = Shader.PropertyToID(outlines_temp_1_str);
    static int lightPass_id = Shader.PropertyToID(lightPass_str);
    static int oitPass_id = Shader.PropertyToID(oitPass_str);
    static int dirShadowAtlas_id = Shader.PropertyToID(dirShadowAtlas_str);
    static int shadowMapTexture_id = Shader.PropertyToID(shadowMapTexture_str);

    static RenderTargetIdentifier gbuffer0_rt = new RenderTargetIdentifier(gbuffer0_id);
    static RenderTargetIdentifier gbuffer1_rt = new RenderTargetIdentifier(gbuffer1_id);
    static RenderTargetIdentifier gbuffer2_rt = new RenderTargetIdentifier(gbuffer2_id);
    static RenderTargetIdentifier gbuffer3_rt = new RenderTargetIdentifier(gbuffer3_id); //BuiltinRenderTextureType.CameraTarget);
    static RenderTargetIdentifier gbuffer_depth_rt = new RenderTargetIdentifier(gbuffer_depth_id); //BuiltinRenderTextureType.CameraTarget);
    static RenderTargetIdentifier depthTexture_rt = new RenderTargetIdentifier(depthTexture_id);
    static RenderTargetIdentifier outlines_temp_0_rt = new RenderTargetIdentifier(outlines_temp_0_id);
    static RenderTargetIdentifier outlines_temp_1_rt = new RenderTargetIdentifier(outlines_temp_1_id);
    static RenderTargetIdentifier lightPass_rt = new RenderTargetIdentifier(lightPass_id);
    static RenderTargetIdentifier oitPass_rt = new RenderTargetIdentifier(oitPass_id);
    static RenderTargetIdentifier dirShadowAtlas_rt = new RenderTargetIdentifier(dirShadowAtlas_id);
    static RenderTargetIdentifier shadowMapTexture_rt = new RenderTargetIdentifier(shadowMapTexture_id);

    static RenderTargetIdentifier[] allGBufferTargets = new RenderTargetIdentifier[4] { gbuffer0_rt, gbuffer1_rt, gbuffer2_rt, gbuffer3_rt };


    public void Render(ScriptableRenderContext context, Camera camera)
    {
        this.camera = camera;
        this.context = context;

        this.toonPipeline = camera.GetComponent<ToonPipeline>();

        cmdBuf.name = "SchnibbleCustomMain";

        if (!Cull())
            return;

        Setup();

        // G-Buffer pass
        DrawGBuffer();
        ExecuteBuffer();
        GrabDepthPass();
        ExecuteBuffer();
        ExtractOutlines();
        ExecuteBuffer();
        DrawLights();
        ExecuteBuffer();
        DrawTransparent();
        ExecuteBuffer();
        DrawSkybox();
        ExecuteBuffer();
        PostProcess();
        ExecuteBuffer();
        BlitToScreen();
        ExecuteBuffer();

        Submit();
    }

    void PostProcess()
    {

    }


    void BlitToScreen()
    {
        Matrix4x4 viewMatrix = camera.worldToCameraMatrix;

        cmdBuf.SetGlobalTexture("_Outlines", outlines_temp_0_rt);
        merge.SetVector("_LightDir", (toonPipeline) ? toonPipeline.mainLight.transform.forward : Vector3.forward);
        merge.SetMatrix("UNITY_MATRIX_I_V", viewMatrix.inverse);
        cmdBuf.Blit(lightPass_id, camera.targetTexture, merge);
    }

    private void ExtractOutlines()
    {
        cmdBuf.name = "extractOutline";

        cmdBuf.GetTemporaryRT(outlines_temp_0_id, camera.pixelWidth, camera.pixelHeight);
        cmdBuf.GetTemporaryRT(outlines_temp_1_id, camera.pixelWidth, camera.pixelHeight);

        cmdBuf.Blit(null, outlines_temp_0_rt, outlineExtract, 0);
        cmdBuf.Blit(outlines_temp_0_rt, outlines_temp_1_rt, outlineExtract, 1);
        cmdBuf.Blit(outlines_temp_1_rt, outlines_temp_0_rt, outlineExtract, 2);
        //cmdBuf.Blit(outlines_temp_0, outlines_temp_1, outlineExtract, 3);
    }

    private RTClearFlags ConvertCameraClearFlag()
    {
        switch (camera.clearFlags)
        {
            case (CameraClearFlags.Skybox):
                {
                    return RTClearFlags.All;
                }
            case (CameraClearFlags.SolidColor):
                {
                    return RTClearFlags.All;
                }
            case (CameraClearFlags.Depth):
                {
                    return RTClearFlags.DepthStencil;
                }
            case (CameraClearFlags.Nothing):
                {
                    return RTClearFlags.None;
                }
        }
        return RTClearFlags.All;
    }


    private void GrabDepthPass()
    {
        cmdBuf.name = "Grab Depth";
        cmdBuf.GetTemporaryRT(depthTexture_id, camera.pixelWidth, camera.pixelHeight, 0, FilterMode.Point, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear);
        CoreUtils.SetRenderTarget(cmdBuf, depthTexture_rt);
        cmdBuf.SetGlobalTexture("_MainTex", gbuffer_depth_rt, RenderTextureSubElement.Depth);
        cmdBuf.DrawProcedural(Matrix4x4.identity, blitDepth, 0, MeshTopology.Triangles, 3);
    }

    private void Setup()
    {
        context.SetupCameraProperties(camera);
        cmdBuf.GetTemporaryRT(gbuffer0_id, camera.pixelWidth, camera.pixelHeight, 0, FilterMode.Point, RenderTextureFormat.DefaultHDR, RenderTextureReadWrite.sRGB);
        cmdBuf.GetTemporaryRT(gbuffer1_id, camera.pixelWidth, camera.pixelHeight, 0, FilterMode.Point, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
        cmdBuf.GetTemporaryRT(gbuffer2_id, camera.pixelWidth, camera.pixelHeight, 0, FilterMode.Point, RenderTextureFormat.ARGB2101010, RenderTextureReadWrite.Linear);


        RenderTextureDescriptor FBO = new RenderTextureDescriptor(camera.pixelWidth, camera.pixelHeight, RenderTextureFormat.DefaultHDR);
        FBO.depthStencilFormat = GraphicsFormat.D24_UNorm_S8_UInt;
        FBO.stencilFormat = GraphicsFormat.R8_UInt;
        FBO.depthBufferBits = 24;


        cmdBuf.GetTemporaryRT(gbuffer3_id, camera.pixelWidth, camera.pixelHeight, 32, FilterMode.Point, RenderTextureFormat.DefaultHDR, RenderTextureReadWrite.Default);
        cmdBuf.GetTemporaryRT(gbuffer_depth_id, FBO, FilterMode.Point);

        CoreUtils.SetRenderTarget(cmdBuf, allGBufferTargets, gbuffer_depth_rt);
        cmdBuf.ClearRenderTarget(ConvertCameraClearFlag(), camera.backgroundColor, 1, 0);
        cmdBuf.BeginSample(cmdBuf.name);
        ExecuteBuffer();
        cmdBuf.EnableKeyword(new GlobalKeyword("UNITY_HDR_ON"));
    }

    private bool Cull()
    {
        if (camera.TryGetCullingParameters(out ScriptableCullingParameters p))
        {
            var maxShadowDistance = 300;
            p.shadowDistance = Mathf.Min(maxShadowDistance, camera.farClipPlane);
            cullingResults = context.Cull(ref p);
            return true;
        }
        return false;
    }

    private void DrawSkybox()
    {

        CoreUtils.SetRenderTarget(cmdBuf, gbuffer3_rt, gbuffer_depth_rt);
        ExecuteBuffer();
        context.DrawSkybox(camera);
    }

    private void DrawGBuffer()
    {
        var sortingSettings = new SortingSettings(camera);
        sortingSettings.criteria = SortingCriteria.CommonOpaque;
        var drawingSettings = new DrawingSettings(gbufferPass, sortingSettings);
        var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);

        context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
    }

    Vector2 SetTileViewport(int idx, int split, float tileSize)
    {
        Vector2 offset = new Vector2(idx % split, idx / split);
        cmdBuf.SetViewport(new Rect(offset.x * tileSize, offset.y * tileSize, tileSize, tileSize));
        return offset;
    }

    // basically going from wpos to atlas space
    Matrix4x4 ConvertToAtlasMatrix(Matrix4x4 m, Vector2 offset, int split)
    {
        if (SystemInfo.usesReversedZBuffer)
        {
            m.m20 = -m.m20;
            m.m21 = -m.m21;
            m.m22 = -m.m22;
            m.m23 = -m.m23;
        }

        float scale = 1f / split;
        m.m00 = (0.5f * (m.m00 + m.m30) + offset.x * m.m30) * scale;
        m.m01 = (0.5f * (m.m01 + m.m31) + offset.x * m.m31) * scale;
        m.m02 = (0.5f * (m.m02 + m.m32) + offset.x * m.m32) * scale;
        m.m03 = (0.5f * (m.m03 + m.m33) + offset.x * m.m33) * scale;
        m.m10 = (0.5f * (m.m10 + m.m30) + offset.y * m.m30) * scale;
        m.m11 = (0.5f * (m.m11 + m.m31) + offset.y * m.m31) * scale;
        m.m12 = (0.5f * (m.m12 + m.m32) + offset.y * m.m32) * scale;
        m.m13 = (0.5f * (m.m13 + m.m33) + offset.y * m.m33) * scale;
        m.m20 = 0.5f * (m.m20 + m.m30);
        m.m21 = 0.5f * (m.m21 + m.m31);
        m.m22 = 0.5f * (m.m22 + m.m32);
        m.m23 = 0.5f * (m.m23 + m.m33);
        return m;
    }

    public struct UnityShadows
    {
        // cant declare on stack fixed array in c#...
        public Vector4 unity_ShadowSplitSpheres0;
        public Vector4 unity_ShadowSplitSpheres1;
        public Vector4 unity_ShadowSplitSpheres2;
        public Vector4 unity_ShadowSplitSpheres3;
        public Vector4 unity_ShadowSplitSqRadii;
        public Vector4 unity_LightShadowBias;
        public Vector4 _LightSplitsNear;
        public Vector4 _LightSplitsFar;
        // cant declare on stack fixed array in c#...
        public Matrix4x4 unity_WorldToShadow0;
        public Matrix4x4 unity_WorldToShadow1;
        public Matrix4x4 unity_WorldToShadow2;
        public Matrix4x4 unity_WorldToShadow3;
        public Vector4 _LightShadowData;
        public Vector4 unity_ShadowFadeCenterAndType;
    };
    static int unityShadowsStride = sizeof(float) * 4 * 10 + sizeof(float) * 16 * 4;

    UnityShadows unityShadowsCPU = new UnityShadows();
    GraphicsBuffer unityShadowsGPU = new GraphicsBuffer(GraphicsBuffer.Target.Constant, 1, unityShadowsStride);


    private void DrawLights()
    {
        cmdBuf.name = "Lighting";

        Mesh m = new Mesh();
        Vector3[] v = new Vector3[4] { new Vector3(-1f, -1f, 0f), new Vector3(-1f, 1f, 0f), new Vector3(1f, 1f, 0f), new Vector3(1f, -1f, 0f) };
        Vector2[] uv = new Vector2[4] { new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(1f, 0f) };
        m.vertices = v;
        m.uv = uv;

        Vector3[] frustumCorners = new Vector3[4];
        camera.CalculateFrustumCorners(new Rect(0, 0, 1, 1), camera.nearClipPlane, Camera.MonoOrStereoscopicEye.Mono, frustumCorners);
        m.normals = frustumCorners;

        camera.CalculateFrustumCorners(new Rect(0, 0, 1, 1), camera.farClipPlane, Camera.MonoOrStereoscopicEye.Mono, frustumCorners);
        m.SetUVs(1, frustumCorners);
        m.triangles = new int[6] { 0, 1, 2, 0, 2, 3 };
        //m.SetIndices(new int[4]{0,1,2,3},MeshTopology.Quads,0);

        int lightIdx = 0;
        foreach (var light in cullingResults.visibleLights)
        {

            if (light.lightType == LightType.Directional)
            {
                cmdBuf.EnableKeyword(new GlobalKeyword("DIRECTIONAL"));
                // render shadows
                if (light.light.shadows != LightShadows.None
                    && light.light.shadowStrength != 0
                    && cullingResults.GetShadowCasterBounds(lightIdx, out Bounds b))
                {
                    int atlasSize = 4096;
                    int cascadeSize = 4;
                    float cascadeRatio1 = 0.07f;
                    float cascadeRatio2 = 0.2f;
                    float cascadeRatio3 = 0.57f;
                    Vector3 cascadeRatios = new Vector3(cascadeRatio1, cascadeRatio2, cascadeRatio3);
                    Vector4[] cascadeCullingSpheres = new Vector4[cascadeSize];
                    Matrix4x4[] dirShadowMatrices = new Matrix4x4[cascadeSize];

                    cmdBuf.GetTemporaryRT(dirShadowAtlas_id, atlasSize, atlasSize, 32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);
                    cmdBuf.SetRenderTarget(dirShadowAtlas_id, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
                    cmdBuf.ClearRenderTarget(true, false, Color.clear);
                    cmdBuf.EnableKeyword(new GlobalKeyword("SHADOWS_SCREEN"));

                    int split = 2;
                    int tileSize = atlasSize / split;

                    ExecuteBuffer();

                    var shadowSettings = new ShadowDrawingSettings(cullingResults, lightIdx);
                    for (int i = 0; i < cascadeSize; ++i)
                    {
                        cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(lightIdx, i, cascadeSize, cascadeRatios, atlasSize, light.light.shadowNearPlane,
                             out Matrix4x4 viewMatrix, out Matrix4x4 projectionMatrix, out ShadowSplitData splitData);
                        shadowSettings.splitData = splitData;

                        dirShadowMatrices[i] = ConvertToAtlasMatrix(projectionMatrix * viewMatrix, SetTileViewport(i, split, tileSize), split);
                        cmdBuf.SetViewProjectionMatrices(viewMatrix, projectionMatrix);
                        cascadeCullingSpheres[i] = splitData.cullingSphere;

                        ExecuteBuffer();

                        context.DrawShadows(ref shadowSettings);
                    }

                    unityShadowsCPU.unity_ShadowSplitSpheres0 = cascadeCullingSpheres[0];
                    unityShadowsCPU.unity_ShadowSplitSpheres0.w *= unityShadowsCPU.unity_ShadowSplitSpheres0.w;
                    unityShadowsCPU.unity_ShadowSplitSpheres1 = cascadeCullingSpheres[1];
                    unityShadowsCPU.unity_ShadowSplitSpheres1.w *= unityShadowsCPU.unity_ShadowSplitSpheres1.w;
                    unityShadowsCPU.unity_ShadowSplitSpheres2 = cascadeCullingSpheres[2];
                    unityShadowsCPU.unity_ShadowSplitSpheres2.w *= unityShadowsCPU.unity_ShadowSplitSpheres2.w;
                    unityShadowsCPU.unity_ShadowSplitSpheres3 = cascadeCullingSpheres[3];
                    var shadowTexelSize = 2f*cascadeCullingSpheres[3].w / tileSize;
                    unityShadowsCPU.unity_ShadowSplitSpheres3.w *= unityShadowsCPU.unity_ShadowSplitSpheres3.w;
                    unityShadowsCPU.unity_ShadowSplitSqRadii = new Vector4(unityShadowsCPU.unity_ShadowSplitSpheres0.w, unityShadowsCPU.unity_ShadowSplitSpheres1.w, unityShadowsCPU.unity_ShadowSplitSpheres2.w, unityShadowsCPU.unity_ShadowSplitSpheres3.w);
                    unityShadowsCPU.unity_LightShadowBias = new Vector4(light.light.shadowBias * -1/(float)tileSize, 1, light.light.shadowNormalBias * shadowTexelSize, 0);
                    unityShadowsCPU._LightSplitsNear = Vector4.zero;
                    unityShadowsCPU._LightSplitsFar = Vector4.zero;
                    unityShadowsCPU.unity_WorldToShadow0 = dirShadowMatrices[0];
                    unityShadowsCPU.unity_WorldToShadow1 = dirShadowMatrices[1];
                    unityShadowsCPU.unity_WorldToShadow2 = dirShadowMatrices[2];
                    unityShadowsCPU.unity_WorldToShadow3 = dirShadowMatrices[3];
                    unityShadowsCPU._LightShadowData = new Vector4(light.light.shadowStrength,0,0,0);
                    unityShadowsCPU.unity_ShadowFadeCenterAndType = Vector4.zero;

                    UnityShadows[] unityShadows = new UnityShadows[1] { unityShadowsCPU };

                    unityShadowsGPU.SetData(unityShadows);

                    cmdBuf.EnableKeyword(new GlobalKeyword("SHADOWS_SPLIT_SPHERES"));
                    cmdBuf.SetGlobalConstantBuffer(unityShadowsGPU, "UnityShadows", 0, unityShadowsStride);
                    cmdBuf.GetTemporaryRT(shadowMapTexture_id, camera.pixelWidth, camera.pixelHeight);
                    cmdBuf.SetGlobalTexture(shadowMapTexture_id, dirShadowAtlas_rt);
                    cmdBuf.SetRenderTarget(shadowMapTexture_rt);
                    cmdBuf.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
                    cmdBuf.DrawMesh(m, Matrix4x4.identity, screenSpaceShadows, 0, 0);
                }

                CoreUtils.SetRenderTarget(cmdBuf, gbuffer3_rt, gbuffer_depth_rt);
                cmdBuf.SetGlobalTexture(dirShadowAtlas_id, dirShadowAtlas_rt);

                cmdBuf.SetGlobalFloat("_LightAsQuad", 1);
                cmdBuf.SetGlobalVector("_LightColor", light.finalColor);
                cmdBuf.SetGlobalVector("_LightDir", -light.localToWorldMatrix.GetColumn(2));
                cmdBuf.SetGlobalVector("_LightPos", new Vector3(light.localToWorldMatrix[0, 3], light.localToWorldMatrix[1, 3], light.localToWorldMatrix[2, 3]));
                cmdBuf.SetGlobalMatrix("unity_WorldToLight", light.localToWorldMatrix.inverse);
                //cmdBuf.SetGlobalMatrix("unity_LightmapFade", ); ??? not used in shader?
                //cmdBuf.SetGlobalTexture("_LightTextureB0", ); ??? should be the attenuation?
                cmdBuf.SetGlobalTexture(shadowMapTexture_id, shadowMapTexture_rt);
                // render quad
                cmdBuf.DrawMesh(m, Matrix4x4.identity, lightMaterial, 0, 0);
            }

            ++lightIdx;
        }
        ExecuteBuffer();

        cmdBuf.name = "Final Light Pass";
        cmdBuf.GetTemporaryRT(lightPass_id, camera.pixelWidth, camera.pixelHeight);
        cmdBuf.Blit(gbuffer3_rt, lightPass_rt, lightMaterial, 2);
    }

    private void DrawTransparent()
    {
        if (toonPipeline)
        {
            MaterialManager mgr = toonPipeline.mgr;
            clearOITBufferCompute = toonPipeline.clearOITBufferCompute;

            cmdBuf.SetComputeBufferParam(clearOITBufferCompute, 0, "_oitFragHeadIdx", mgr.oitFragHeadIdx);
            cmdBuf.SetComputeIntParam(clearOITBufferCompute, "screenWidth", camera.pixelWidth);
            cmdBuf.DispatchCompute(clearOITBufferCompute, 0, Mathf.CeilToInt(camera.pixelWidth / 32.0f), Mathf.CeilToInt(camera.pixelHeight / 32.0f), 1);
            cmdBuf.ClearRandomWriteTargets();
            cmdBuf.SetRandomWriteTarget(1, mgr.oitFragDepth, true);
            cmdBuf.SetRandomWriteTarget(2, mgr.oitFragHeadIdx, true);

            var sortingSettings = new SortingSettings(camera);
            sortingSettings.criteria = SortingCriteria.CommonTransparent;
            var drawingSettings = new DrawingSettings(transparentPass, sortingSettings);
            var filteringSettings = new FilteringSettings(RenderQueueRange.transparent);

            context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);

            cmdBuf.GetTemporaryRT(oitPass_id, camera.pixelWidth, camera.pixelHeight);
            cmdBuf.Blit(lightPass_rt, oitPass_rt);
            cmdBuf.Blit(oitPass_rt, lightPass_rt, oitMat);
            cmdBuf.ClearRandomWriteTargets();
        }
        else
        {
            var sortingSettings = new SortingSettings(camera);
            sortingSettings.criteria = SortingCriteria.CommonTransparent;
            var drawingSettings = new DrawingSettings(transparentPass, sortingSettings);
            var filteringSettings = new FilteringSettings(RenderQueueRange.transparent);

            context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
        }
    }

    private void ExecuteBuffer()
    {
        context.ExecuteCommandBuffer(cmdBuf);
        cmdBuf.Clear();
    }

    private void Submit()
    {
        cmdBuf.EndSample(cmdBuf.name);
        context.Submit();
    }
}

public class SchRenderPipeline : RenderPipeline
{
    CameraRenderer renderer = new CameraRenderer();
    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        foreach (var camera in cameras)
        {
            renderer.Render(context, camera);
        }
    }

}
