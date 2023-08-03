Shader "Custom/Billboard"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Cutoff("Cutoff", Range(0.0, 1.0)) = 0
        [IntRange]_MaterialID("MaterialID", Range(0,255)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque"  "DisableBatching" = "True"}

        Pass
        {

			Tags {
                "LightMode" = "Deferred"
            }

Cull Off

            CGPROGRAM

            #define _ALPHATEST_ON 1

            #define SCHNIBBLE_NO_VERT_TRANSFORM 1
            #define SCHNIBBLE_VERT vert
			#include "../../../SCHNIBBLE/Shaders/Deferred/SchnibbleCustomGBufferPass.cginc"


			#include "../../../SCHNIBBLE/Shaders/Tools/Functions.cginc"
			#include "../../../SCHNIBBLE/Shaders/Deferred/SchnibbleCustomGBufferInputs.cginc"
			#include "../../../SCHNIBBLE/Shaders/Deferred/SchnibbleCustomGBuffer.cginc"

			void vert (inout SchVertexInput v) {


                float3 mpos = v.vertex.xyz;
                float3 wpos = mul(unity_ObjectToWorld, mpos);

                float3 origin_wpos = mul(unity_ObjectToWorld, float4(0,0,0,1));

//fix up vector as anchor
                float3 n_wpos = normalize(_WorldSpaceCameraPos - origin_wpos);
                float3 u_mpos = float3(0,1,0);
                float3 u_wpos = normalize(mul(unity_ObjectToWorld, float4(u_mpos,0)));
                float3 r_wpos = cross(u_wpos, n_wpos);
                n_wpos = cross(r_wpos,u_wpos);

// get model matrix rotation
// carefull might not work with opengl
                float3 r_w = normalize(unity_ObjectToWorld._m00_m01_m02);
                float3 u_w = normalize(unity_ObjectToWorld._m10_m11_m12);
                float3 f_w = normalize(unity_ObjectToWorld._m20_m21_m22);
                float4x4 rot_w = float4x4(r_w, 0, u_w, 0, f_w, 0, 0, 0, 0, 0);

                float4x4 rot_wpos = float4x4( float4(r_wpos,0), float4(u_wpos,0), float4(-n_wpos,0), float4(0,0,0,1));

                float3 wpos_f = mul(rot_w, mul(rot_wpos, wpos)) + origin_wpos;
                // go into clip space
                float4 opos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(wpos_f,1) ));

// save almost working as intended
//
//
                //// transform model origin to world then to view (xy are align with camera plane).
                //// now we can add transformed vertex position in view space.
                //float3 vpos = v.vertex.xyz;
//
                //float3 origin_wpos = mul(unity_ObjectToWorld, float4(0,0,0,1));
//
                //float3 n = normalize(origin_wpos - _WorldSpaceCameraPos);
//
                //float3 u = float3(0,1,0);
                //float3 u_wpos = normalize(mul(unity_ObjectToWorld, float4(u,0)));
//
                //float3 r = cross(u_wpos, n);
//
                //n = cross(u_wpos, r);
//
                //float4x4 rot = float4x4(float4(r,0),float4(u,0),float4(n,0), float4(0,0,0,1));
//
//// fonctionne pour fire et first coin
				////float3 wpos = (mul((float3x3)unity_ObjectToWorld, vpos));
                ////float3 wwpos = mul(rot, float4(wpos,1)) + float4(origin_wpos, 1);
//
                //float3x3 otw = float3x3( normalize(unity_ObjectToWorld._m00_m10_m20), normalize(unity_ObjectToWorld._m10_m11_m12), normalize(unity_ObjectToWorld._m20_m21_m22));
//// fonctionne pour coin
				////float3 wpos = normalize(mul((float3x3)unity_ObjectToWorld), vpos);
                //// new test
				//float3 wpos = mul(otw, vpos);
                //float3 wwpos = mul(unity_ObjectToWorld, mul(rot, float4(wpos,0))) + float4(origin_wpos, 0);
//
                //float4 opos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(wwpos,1) ));

				v.vertex = opos;
            }

            ENDCG
        }
    }
}
