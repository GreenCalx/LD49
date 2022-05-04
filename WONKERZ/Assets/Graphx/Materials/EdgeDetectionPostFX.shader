Shader "Hidden/EdgeDetectionPostFX"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _SobelThreshold ( "Threshold", float ) = 0
        _SobelColorThreshold ( "Color Threshold", float ) = 0
        _SobelNormalThreshold("Normal Threshold", float) = 0
        _Thickness ("Thickness", float) =0
        _ColorThickness ("Color Thickness", float) =0
        _NormalThickness ("Normal Thickness", float)=0
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            static float2 SobleNeighborsSamples[9] = {
                float2(-1,1), float2(0,1), float2(1,1),
                float2(-1,0), float2(0,0), float2(1,0),
                float2(-1,-1), float2(0,-1), float2(1,-1)
            };

            sampler2D _CameraDepthNormalsTexture;
            sampler2D _MainTex;
            static float SobelKernelX[9]= {
                1,0,-1,
                2,0,-2,
                1,0,-1
            };

            static float SobelKernelY[9] = {
                1,2,1,
                0,0,0,
                -1,-2,-1
            };

            void ComputeSobelAtPoint(float2 uv, float Thickness, out float Result) {
                Result = 0;
                [unroll]
                for (int i=0;i<9;++i) {
                    float4 v = tex2D(_CameraDepthNormalsTexture, uv + SobleNeighborsSamples[i]*Thickness);
                    float depth;
                    float3 normal;
                    DecodeDepthNormal(v, depth,normal);
                    Result += depth * float2(SobelKernelX[i], SobelKernelY[i]);
                }
                Result = length(Result);
            }

            void ComputeSobelColorAtPoint(float2 uv, float Thickness, out float Result) {
                Result=0;
                float SobelR=0;
                float SobelG=0;
                float SobelB=0;
                [unroll]
                for (int i=0;i<9;++i) {
                    float3 col = tex2D(_MainTex, uv + SobleNeighborsSamples[i]*Thickness);
                    SobelR += col.r * float2(SobelKernelX[i], SobelKernelY[i]);
                    SobelG += col.g * float2(SobelKernelX[i], SobelKernelY[i]);
                    SobelB += col.b * float2(SobelKernelX[i], SobelKernelY[i]);
                }
                Result = max(length(SobelR), max(length(SobelG), length(SobelB)));
            }

            void ComputeSobelNormalAtPoint(float2 uv, float Thickness, out float Result) {
                Result=0;
                float SobelR=0;
                float SobelG=0;
                float SobelB=0;
                [unroll]
                float4 centerValue = tex2D(_CameraDepthNormalsTexture, uv );
                for (int i=0;i<9;++i) {
                    float4 v = tex2D(_CameraDepthNormalsTexture, uv + SobleNeighborsSamples[i]*Thickness);
                    float depth;
                    float3 col;
                    DecodeDepthNormal(v, depth, col);
                    SobelR += sin(dot(normalize(centerValue), normalize(col))) * float2(SobelKernelX[i], SobelKernelY[i]);

                }
                Result = SobelR;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float _SobelThreshold;
            float _Thickness;
            float _SobelColorThreshold;
            float _ColorThickness;
            float _SobelNormalThreshold;
            float _NormalThickness;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                float Result;
                ComputeSobelAtPoint(i.uv, _Thickness, Result);
                if (Result > _SobelThreshold) {
                    return fixed4(0,0,0,1);
                }

                ComputeSobelColorAtPoint(i.uv, _ColorThickness, Result);
                if (Result > _SobelColorThreshold) {
                    return fixed4(0,0,0,1);
                }

                ComputeSobelNormalAtPoint(i.uv, _NormalThickness, Result);
                if (Result > _SobelNormalThreshold) {
                    return fixed4(0,0,0,1);
                }
                    float4 v = tex2D(_CameraDepthNormalsTexture, i.uv );
                    float depth;
                    float3 normal;
                    DecodeDepthNormal(v, depth, normal);


                return col;
            }
            ENDCG
        }
    }
}
