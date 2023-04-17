using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName="Rendering/SchRenderPipeline")]
public class SchRenderPipelineAsset : RenderPipelineAsset
{
    protected override RenderPipeline CreatePipeline() {
        return new SchRenderPipeline();
    }
}
