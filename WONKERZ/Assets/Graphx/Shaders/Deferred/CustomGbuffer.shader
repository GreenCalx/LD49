Shader "Custom/Schnibble-Gbuffer"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("Albedo", 2D) = "white" {}

        [Gamma] _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
        _MetallicGlossMap("Metallic", 2D) = "white" {}

        _Roughness("Roughness", Range(0.0, 1.0)) = 0.5

        _BumpScale("Scale", Float) = 1.0
        [Normal] _BumpMap("Normal Map", 2D) = "bump" {}
		
		_Parallax ("Height Scale", Range (0.005, 0.08)) = 0.02
        _ParallaxMap ("Height Map", 2D) = "black" {}

        _OcclusionStrength("Strength", Range(0.0, 1.0)) = 1.0
        _OcclusionMap("Occlusion", 2D) = "white" {}

        [HDR]
        _EmissionColor("Emission Color", Color) = (0,0,0)
        _EmissionMap("Emission", 2D) = "white" {}

        _MaterialID("MaterialID", Int) = 0
		
		_DepthMask("DepthMask", Float) = 0
		_DepthMaskRead("DepthMaskRead", Float) = 0
		_Ztest("ztest", Float) = 2
		
		_Tex("tux", Vector) = (0,0,0,0)
		// Blending state
        [HideInInspector] _Mode ("__mode", Float) = 0.0
        [HideInInspector] _SrcBlend ("__src", Float) = 1.0
        [HideInInspector] _DstBlend ("__dst", Float) = 0.0
        [HideInInspector] _ZWrite ("__zw", Float) = 1.0
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
			
			ZTest [_Ztest]
			Cull Back
            CGPROGRAM

            #pragma target 3.0
            #pragma exclude_renderers nomrt

            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature_fragment _EMISSION
            #pragma shader_feature_local _METALLICGLOSSMAP
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature_local_fragment _DETAIL_MULX2
            #pragma shader_feature_local _PARALLAXMAP

            #pragma multi_compile_prepassfinal
            #pragma multi_compile_instancing
            #pragma vertex vert
            #pragma fragment frag

            #define DEFERRED_PASS
			
			#include "SchnibbleCustomGBuffer.cginc"
			#include "UnityCG.cginc"
			#include "UnityStandardInput.cginc"
			#include "UnityStandardCore.cginc"

float       _Roughness;

int _MaterialID;

float _DepthMask;
float _DepthMaskRead;
sampler2D _DepthMaskTexture;

float4 _Tex;

            struct VertexOutput
            {
                UNITY_POSITION(pos);
                float4 tex                            : TEXCOORD0;
                half3 normalWorld : NORMAL;
				float4 screenPos : TEXCOORD1;
				float4 tangentToWorldAndPackedData[3] : TEXCOORD2;    // [3x3:tangentToWorld | 1x3:viewDirForParallax or worldPos]
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            VertexOutput vert (VertexInput input) {
                VertexOutput o;
				UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, o);
                o.pos = UnityObjectToClipPos(input.vertex);
                o.tex = TexCoords(input);
                o.normalWorld = UnityObjectToWorldNormal(input.normal);
				o.screenPos = ComputeScreenPos(o.pos);
				 #ifdef _TANGENT_TO_WORLD
        float4 tangentWorld = float4(UnityObjectToWorldDir(input.tangent.xyz), input.tangent.w);

        float3x3 tangentToWorld = CreateTangentToWorldPerVertex(o.normalWorld, tangentWorld.xyz, tangentWorld.w);
        o.tangentToWorldAndPackedData[0].xyz = tangentToWorld[0];
        o.tangentToWorldAndPackedData[1].xyz = tangentToWorld[1];
        o.tangentToWorldAndPackedData[2].xyz = tangentToWorld[2];
    #else
        o.tangentToWorldAndPackedData[0].xyz = 0;
        o.tangentToWorldAndPackedData[1].xyz = 0;
        o.tangentToWorldAndPackedData[2].xyz = o.normalWorld;
    #endif
	
	#ifdef _PARALLAXMAP
        //TANGENT_SPACE_ROTATION;
		float3 binormal = cross( normalize(input.normal), normalize(input.tangent.xyz) ) * input.tangent.w; \
        float3x3 rotation = float3x3( input.tangent.xyz, binormal, input.normal );
        half3 viewDirForParallax = mul (rotation, ObjSpaceViewDir(input.vertex));
        o.tangentToWorldAndPackedData[0].w = viewDirForParallax.x;
        o.tangentToWorldAndPackedData[1].w = viewDirForParallax.y;
        o.tangentToWorldAndPackedData[2].w = viewDirForParallax.z;
    #endif
                return o;
            }

half2 MetallicRoughCustom(float2 uv)
{
    half2 mg;
    // mg.r = tex2D(_MetallicGlossMap, uv).r;
    mg.r = _Metallic;
    // mg.g = tex2D(_SpecGlossMap, uv).r;
    mg.g = _Roughness;
    return mg;
}
            GBuffer frag (VertexOutput input){
			    UNITY_SETUP_INSTANCE_ID(input);
				input.tex = Parallax(input.tex, IN_VIEWDIR4PARALLAX(input));
			    GBuffer output;
			    if (_DepthMask != 0) {
				    // write depth mask
					output.gBuffer0 = float4(0,0,0,0);
					output.gBuffer1 = float4(0,0,0,0);
					output.gBuffer2 = float4(0,0,0,0);
					output.gBuffer3	= float4(0,0,0,0);			 
					output.gBuffer1.b = _DepthMask;
				}
			    else {
				float4 screenPos = ComputeScreenPos(input.pos);
				float depthMask = tex2D(_DepthMaskTexture, input.screenPos.xy/input.screenPos.w).b;
				    if (_DepthMaskRead != 0) {
					    // read depth mask, only render inside depth mask value
						if(depthMask < 0.9)
						    discard;
					}
			    
			    SchnibbleGBuffer gbuffer;
				float2 metRough = MetallicRoughCustom(input.tex);
				gbuffer.albedo = _Color*tex2D (_MainTex, input.tex);
				gbuffer.metallic = metRough.r;
				gbuffer.roughness = metRough.g;
				gbuffer.normalWorld = PerPixelWorldNormal(input.tex, input.tangentToWorldAndPackedData); //input.normalWorld;
				gbuffer.occlusion = Occlusion(input.tex);
				gbuffer.matId = _MaterialID;
								
			    PackSchnibbleGBuffer(gbuffer, output);
                output.gBuffer3 = float4(_EmissionColor.rgb, RGBToHSV(_EmissionColor).b);
                }
                return output;
            }

            ENDCG

        }
		// ------------------------------------------------------------------
        //  Base forward pass (directional light, emission, lightmaps, ...)
        Pass
        {
            Name "FORWARD"
            Tags { "LightMode" = "ForwardBase" }

            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]

            CGPROGRAM
            #pragma target 3.0

            // -------------------------------------

            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature_fragment _EMISSION
            #pragma shader_feature_local _METALLICGLOSSMAP
            #pragma shader_feature_local_fragment _DETAIL_MULX2
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature_local_fragment _GLOSSYREFLECTIONS_OFF
            #pragma shader_feature_local _PARALLAXMAP

            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            // Uncomment the following line to enable dithering LOD crossfade. Note: there are more in the file to uncomment for other passes.
            //#pragma multi_compile _ LOD_FADE_CROSSFADE

            #pragma vertex vertBase
            #pragma fragment fragBase
            #include "UnityStandardCoreForward.cginc"

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
