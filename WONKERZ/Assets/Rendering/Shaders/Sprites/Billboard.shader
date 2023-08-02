Shader "Sprites/Billboard"
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

        AlphaTest Greater 0.5

        Pass
        {

			Tags {
                "LightMode" = "Deferred"
            }

            CGPROGRAM

            #define _ALPHATEST_ON 1

            #define SCHNIBBLE_NO_VERT_TRANSFORM 1
            #define SCHNIBBLE_VERT vert
			#include "../../../SCHNIBBLE/Shaders/Deferred/SchnibbleCustomGBufferPass.cginc"


			#include "../../../SCHNIBBLE/Shaders/Tools/Functions.cginc"
			#include "../../../SCHNIBBLE/Shaders/Deferred/SchnibbleCustomGBufferInputs.cginc"
			#include "../../../SCHNIBBLE/Shaders/Deferred/SchnibbleCustomGBuffer.cginc"

			void vert (inout SchVertexInput v) {

                // transform model origin to world then to view (xy are align with camera plane).
                // now we can add transformed vertex position in view space.
				float3 vpos = mul((float3x3)unity_ObjectToWorld, v.vertex.xyz);
                float4 oPos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, mul(unity_ObjectToWorld, float4(0,0,0,1))) + float4(vpos,0));

				v.vertex = oPos;
            }

            ENDCG
        }
    }
}
