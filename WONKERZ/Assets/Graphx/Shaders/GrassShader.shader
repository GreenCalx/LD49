Shader "Custom/GrassFSTest"
{
    Properties
    {
        _BaseColor ("Color", Color) = (1,1,1,1)
        _TipColor ("TipColor", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Cull off
        Pass {

            CGPROGRAM
            #pragma target 3.5
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct DrawVertex {
                float3 positionWS;
                float height;
            };

            struct DrawTriangle {
                float3 lightingNormalWS;
                DrawVertex vertices[3];
            };

            StructuredBuffer<DrawTriangle> _DrawTriangles;


            struct VertexOutput {
                float uv :TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;

                float4 positionCS : SV_POSITION;
            };

            float4 _BaseColor;
            float4 _TipColor;


            VertexOutput vert (uint vertexID : SV_VertexID) {
                VertexOutput output = (VertexOutput)0;

                DrawTriangle tri = _DrawTriangles[vertexID / 3];
                DrawVertex input = tri.vertices[vertexID % 3];

                output.positionWS = input.positionWS;
                output.normalWS = tri.lightingNormalWS;
                output.uv = input.height;
                output.positionCS = mul(UNITY_MATRIX_VP, float4(input.positionWS, 1));

                return output;
            }

            fixed4 frag (VertexOutput v) : SV_Target {
                return fixed4(lerp(_BaseColor.rgb, _TipColor.rgb, v.uv), 1);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
