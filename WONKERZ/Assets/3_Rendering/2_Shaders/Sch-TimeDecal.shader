Shader "Custom/Sch-TimeDecal"
{
    Properties
    {
       _Color("Color", Color) = (1,1,1,1)
       _ColorMaxTime("Color", Color) = (1,1,1,1)
        [NoScaleOffset] _MainTex("Albedo", 2D) = "white" {}

        [Gamma] _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
        [NoScaleOffset] _MetallicGlossMap("Metallic", 2D) = "white" {}

        _Roughness("Roughness", Range(0.0, 1.0)) = 0.5

        _BumpScale("Scale", Float) = 1.0
        [Normal] _BumpMap("Normal Map", 2D) = "bump" {}
		
		_Parallax ("Height Scale", Range (0.005, 0.08)) = 0.02
        _ParallaxMap ("Height Map", 2D) = "black" {}

        _OcclusionStrength("Strength", Range(0.0, 1.0)) = 1.0
        [NoScaleOffset]_OcclusionMap("Occlusion", 2D) = "white" {}

        [HDR]
        _EmissionColor("Emission Color", Color) = (0,0,0)
        [NoScaleOffset]_EmissionMap("Emission", 2D) = "white" {}

        [IntRange]_MaterialID("MaterialID", Range(0,255)) = 0

        [Header(States)]
		_DepthMask("DepthMask", Float) = 0
		[Toggle]_DepthMaskRead("DepthMaskRead", Float) = 0
		[Enum(UnityEngine.Rendering.CompareFunction)] _Ztest("ztest", Float) = 2
        [Enum(UnityEngine.Rendering.BlendOp)] _BlendOp("blend op", Float) = 0
        [Enum(Zero,0,RGBA,15,A,1,R,8,G,4,B,2,RGB,14)] _ColorMask0("color mask0", Float) = 15
        [Enum(Zero,0,RGBA,15,A,1,R,8,G,4,B,2,RGB,14)] _ColorMask1("color mask1", Float) = 15
        [Enum(Zero,0,RGBA,15,A,1,R,8,G,4,B,2,RGB,14)] _ColorMask2("color mask2", Float) = 15
        [Enum(Zero,0,RGBA,15,A,1,R,8,G,4,B,2,RGB,14)] _ColorMask3("color mask3", Float) = 15
		
		// Blending state
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("blendsrc", Float) = 1.0
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("blenddst", Float) = 0.0
        [Toggle] _ZWrite ("zwrite", Float) = 1.0
        [Enum(UnityEngine.Rendering.CullMode)]_CullMode("CullMode", Float) = 2
        [Toggle] _Decal ("decal", Float) = 0
        [IntRange] _DecalAffectOnlyID ("decal id filter", Range(0, 255)) = -1
[Toggle] _DecalWriteNormal ("decalnormal", Float)=0
[Toggle] _DecalWriteColor ("decalcolor", Float)=0
[Toggle] _DecalWriteMaterial ("decalMaterial", Float) = 0
[Toggle] _DecalWriteMaterialID ("decalmatid",Float)=0
[Toggle] _DecalWriteEmission ("decalemission", Float)=0

         _AnimationTime ("animation time", Float)=0
    }

    SubShader
    {
        Pass {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
       
        #define SCHNIBBLE_FRAG surf

        #include "UnityStandardCore.cginc"
		#include "../../SCHNIBBLE/Shaders/Deferred/SchnibbleCustomGBuffer.cginc"
		#include "../../SCHNIBBLE/Shaders/Deferred/SchnibbleCustomGBufferInputs.cginc"
		#include "../../SCHNIBBLE/Shaders/Tools/Functions.cginc"
        #include "../../SCHNIBBLE/Shaders/Deferred/SchnibbleCustomOIT.cginc"
		#include "../../SCHNIBBLE/Shaders/Deferred/SchnibbleCustomGBufferPass.cginc"

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        float _AnimationTime;
        float4 _ColorMaxTime;

        void surf (VertexOutput o, inout SchnibbleGBuffer buffer)
        {
            if (_AnimationTime > o.tex.y)
                buffer.albedo = _Color * _ColorMaxTime * _AnimationTime;
            else
                buffer.albedo = tex2D(_CameraGBufferTexture0Copy, o.screenPos.xy/o.screenPos.w);//float4(0,0,0,1);
        }  
       
        ENDCG
    }
    }
   
    FallBack "Diffuse"
}
