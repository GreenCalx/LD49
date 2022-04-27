Shader "Custom/GrassFSTest"
{
    Properties
    {
        _BaseColor ("Color", Color) = (1,1,1,1)
        _TipColor ("TipColor", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "LightMode" = "ForwardBase"}

        Pass {

            CGPROGRAM
            #pragma target 3.5
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"

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

                LIGHTING_COORDS(3,4)
            };

            float4 _BaseColor;
            float4 _TipColor;

            float4 _LightColor0;

            struct LightingData {
                float3 normal;
                float3 lightDir;
                float3 albedo;
                float atten;
                };
            half4 LightingSimpleLambert (LightingData s) {
                half NdotL = dot (s.normal, s.lightDir);
                half4 c;
                c.rgb = s.albedo * _LightColor0.rgb * 3 * (NdotL*s.atten) + s.albedo*0.5;
                c.a = 1;
                return c;
            }

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
                LightingData LD;
                LD.atten = LIGHT_ATTENUATION(IN);
                LD.normal = normalize(v.normalWS);
                LD.lightDir = normalize(_WorldSpaceLightPos0.xyz);
                LD.albedo = lerp(_BaseColor.rgb, _TipColor.rgb, v.uv);
                return LightingSimpleLambert(LD);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
