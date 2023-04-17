// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Custom/Schnibble-DeferredToonShading" {
	Properties {
		_LightTexture0 ("", any) = "" {}
		_LightTextureB0 ("", 2D) = "" {}
		_ShadowMapTexture ("", any) = "" {}
		_SrcBlend ("", Float) = 1
		_DstBlend ("", Float) = 1
		_MainTex("",2D) = "" {}
	}
	SubShader {
		// Pass 1: Lighting pass
		//  LDR case - Lighting encoded into a subtractive ARGB8 buffer
		//  HDR case - Lighting additively blended into floating point buffer
		Pass {
            Cull Off
			ZWrite Off
            ZTest Always

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex sch_vert_deferred
			#pragma fragment frag
			#pragma multi_compile_lightpass
			#pragma multi_compile ___ UNITY_HDR_ON

			#pragma exclude_renderers nomrt


#include "UnityCG.cginc"
#include "UnityDeferredLibrary.cginc"
#include "UnityPBSLighting.cginc"
#include "UnityStandardUtils.cginc"

#include "UnityStandardBRDF.cginc"

			#include "SchnibbleCustomGBuffer.cginc"
			#include "../SchnibbleTools/SchnibbleFunctions.cginc"

#include "SchnibbleCustomBRDF.cginc"

#ifdef DIRECTIONAL
unity_v2f_deferred sch_vert_deferred (float4 vertex : POSITION, float3 normal : NORMAL,uint idx : SV_VertexID)
{
    float2 vertices[3] = {float2(-1,-1), float2(3,-1), float2(-1, 3)};

    unity_v2f_deferred o;

    o.pos = float4(vertices[idx],0,1);
    o.uv  = o.pos * float4(0.5,-0.5,1,1) + 0.5;
    o.pos = vertex;//UnityObjectToClipPos(vertex);
    o.pos.a = 1;
    o.uv  = ComputeScreenPos(o.pos);
    o.ray = normalize(normal);

    return o;
}
#else
unity_v2f_deferred sch_vert_deferred (float4 vertex : POSITION, float3 normal : NORMAL)
{
    unity_v2f_deferred o;
    o.pos = UnityObjectToClipPos(vertex);
    o.uv  = ComputeScreenPos(o.pos);
    o.ray = UnityObjectToViewPos(vertex) * float3(-1,-1,1);

    return o;
}
#endif
			half4 CalculateLight (unity_v2f_deferred i)
			{
				float3 wpos;
				float2 uv;
				float atten, fadeDist;
				UnityLight light;
				UNITY_INITIALIZE_OUTPUT(UnityLight, light);
				UnityDeferredCalculateLightParams (i, wpos, uv, light.dir, atten, fadeDist);

				light.color = _LightColor.rgb;

				// unpack Gbuffer
				GBuffer gbuffer;
				gbuffer.gBuffer0 = tex2D (_CameraGBufferTexture0, uv);
				gbuffer.gBuffer1 = tex2D (_CameraGBufferTexture1, uv);
				gbuffer.gBuffer2 = tex2D (_CameraGBufferTexture2, uv);
				
				SchnibbleGBuffer schGBuffer;
				UnpackSchnibbleGBuffer(gbuffer, schGBuffer);
				
				float3 eyeVec = normalize(wpos-_WorldSpaceCameraPos);
				half oneMinusReflectivity = 1 - SpecularStrength(schGBuffer.specular.rgb);

				UnityIndirect ind;
				UNITY_INITIALIZE_OUTPUT(UnityIndirect, ind);
				ind.diffuse = 0;
				ind.specular = 0;

				half4 res = BRDF_Toon(schGBuffer.albedo, schGBuffer.specular, schGBuffer.roughness, schGBuffer.normalWorld, -eyeVec, oneMinusReflectivity, light, atten, schGBuffer.matId);
				return res;
			}

			#ifdef UNITY_HDR_ON
			half4
			#else
			fixed4
			#endif
			frag (unity_v2f_deferred i) : SV_Target
			{
				half4 c = CalculateLight(i);
				#ifdef UNITY_HDR_ON
				return c;
				#else
				return exp2(-c);
				#endif
			}

			ENDCG
		}


		// Pass 2: Final decode pass.
		Pass {
			ZTest Always Cull Off ZWrite Off
			Stencil {
				ref [_StencilNonBackground]
				readmask [_StencilNonBackground]
				// Normally just comp would be sufficient, but there's a bug and only front face stencil state is set (case 583207)
				compback equal
				compfront equal
			}

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			#pragma exclude_renderers nomrt

			#include "UnityCG.cginc"
			#include "../SchnibbleTools/SchnibbleFunctions.cginc"

			sampler2D _LightBuffer;
			struct v2f {
				float4 vertex : SV_POSITION;
				float2 texcoord : TEXCOORD0;
			};

			v2f vert (float4 vertex : POSITION, float2 texcoord : TEXCOORD0)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(vertex);
				o.texcoord = texcoord.xy;
			#ifdef UNITY_SINGLE_PASS_STEREO
				o.texcoord = TransformStereoScreenSpaceTex(o.texcoord, 1.0f);
			#endif
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				return -log2(tex2D(_LightBuffer, i.texcoord));
			}
			ENDCG
		}

		// Pass 3: Final ramping pass.
		Pass {
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			#pragma exclude_renderers nomrt

			#include "UnityCG.cginc"
			#include "SchnibbleCustomGBuffer.cginc"
			#include "../SchnibbleTools/SchnibbleFunctions.cginc"

			sampler2D	_MainTex;

			struct v2f {
				float4 vertex : SV_POSITION;
				float2 texcoord : TEXCOORD0;
			};

			v2f vert (float4 vertex : POSITION, float2 texcoord : TEXCOORD0)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(vertex);
				o.texcoord = texcoord.xy;
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				float4 lightBuffer = tex2D(_MainTex, i.texcoord) + float4(0.001,0.001,0.001,0); // avoid black
				float3 lightColorNormalized = NormalizeColor(lightBuffer);
				float3 lightColorHSV = RGBToHSV(NormalizeColor(lightBuffer));
				lightColorNormalized = saturate(HSVToRGB(float3(lightColorHSV.xy, 1)));
				//lightColorNormalized = RGBToHSV(lightBuffer);
				 // unpack Gbuffer
				GBuffer gbuffer;
				gbuffer.gBuffer0 = tex2D (_CameraGBufferTexture0, i.texcoord);
				gbuffer.gBuffer1 = tex2D (_CameraGBufferTexture1, i.texcoord);
				gbuffer.gBuffer2 = tex2D (_CameraGBufferTexture2, i.texcoord);
				
				SchnibbleGBuffer schGBuffer;
				UnpackSchnibbleGBuffer(gbuffer, schGBuffer);
				float3 colorRamp;
				if (lightBuffer.a > 1) colorRamp = lightBuffer.a;
				else 
					colorRamp = SampleTextureRGB(float2(lightBuffer.a, RGBToHSV(schGBuffer.albedo).x),
												schGBuffer.matId,
												SchTextureToonRampOffset);
				return half4(lightColorNormalized * colorRamp * (schGBuffer.albedo+schGBuffer.specular), lightBuffer.a);
			}
			ENDCG
		}
	}
	Fallback Off
}
