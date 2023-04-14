Shader "Custom/Schnibble-Gbuffer"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
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
        [Enum(Zero,0,RGBA,15,A,1,R,2,G,4,B,8,RGB,14)] _ColorMask("color mask", Float) = 15
		
		// Blending state
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("blendsrc", Float) = 1.0
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("blenddst", Float) = 0.0
        [Toggle] _ZWrite ("zwrite", Float) = 1.0
        [Enum(UnityEngine.Rendering.CullMode)]_CullMode("CullMode", Float) = 2
        [Toggle] _Decal ("decal", Float) = 0
    }
	
	CGINCLUDE
        #define UNITY_SETUP_BRDF_INPUT MetallicSetup
    ENDCG
	
    SubShader
    {   
	    Name "DEFERRED"
        Tags { "RenderType"="Opaque" }
        LOD 100
		
        Pass
        {
            Tags {
                "LightMode" = "Deferred"
            }
			
            Stencil {
			    Ref 192
				WriteMask 207
				Comp Always
				Pass Replace
			}


            Blend [_SrcBlend] [_DstBlend]
            BlendOp [_BlendOp]
            ZWrite [_ZWrite]
            ZTest [_Ztest]
            Cull [_CullMode]
            ColorMask [_ColorMask]
            CGPROGRAM

            #include "SchnibbleCustomGBufferInputs.cginc"
			#include "SchnibbleCustomGBufferPass.cginc"

            ENDCG

        }
		// ------------------------------------------------------------------
        //  Base forward pass (directional light, emission, lightmaps, ...)
        Pass
        {
            Name "FORWARD"
            Tags { "LightMode" = "ForwardBase" }

            Blend [_SrcBlend] [_DstBlend]
            BlendOp [_BlendOp]
            ZWrite [_ZWrite]
            ZTest [_Ztest]
            Cull [_CullMode]
            ColorMask [_ColorMask]
            CGPROGRAM

            #include "UnityStandardConfig.cginc"
            #include "SchnibbleCustomGBufferForwardPass.cginc"

            ENDCG
        }
        // ------------------------------------------------------------------
        //  Additive forward pass (one light per pass)
        Pass
        {
            Name "FORWARD_DELTA"
            Tags { "LightMode" = "ForwardAdd" }
            Blend [_SrcBlend] One
            Fog { Color (0,0,0,0) } // in additive pass fog should be black
            ZWrite Off
            ZTest LEqual
            ColorMask [_ColorMask]

            CGPROGRAM
            #pragma target 3.0

            // -------------------------------------


            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature_local _METALLICGLOSSMAP
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature_local_fragment _DETAIL_MULX2
            #pragma shader_feature_local _PARALLAXMAP

            #pragma multi_compile_fwdadd_fullshadows
            #pragma multi_compile_fog
            // Uncomment the following line to enable dithering LOD crossfade. Note: there are more in the file to uncomment for other passes.
            //#pragma multi_compile _ LOD_FADE_CROSSFADE

            #pragma vertex vertAdd
            #pragma fragment fragAdd
            #include "UnityStandardCoreForward.cginc"

            ENDCG
        }
		// ------------------------------------------------------------------
        //  Shadow rendering pass
        Pass {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On ZTest LEqual

            CGPROGRAM
            #pragma target 3.0

            // -------------------------------------


            #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature_local _METALLICGLOSSMAP
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature_local _PARALLAXMAP
            #pragma multi_compile_shadowcaster
            #pragma multi_compile_instancing
            // Uncomment the following line to enable dithering LOD crossfade. Note: there are more in the file to uncomment for other passes.
            //#pragma multi_compile _ LOD_FADE_CROSSFADE

            #pragma vertex vertShadowCaster
            #pragma fragment fragShadowCaster

            #include "UnityStandardShadow.cginc"

            ENDCG
        }
    }
}
