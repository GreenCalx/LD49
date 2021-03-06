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
        _DirectionalLightPosition ("Light Pos", Vector) = (0,0,0,0)
        _MoebiusThickness ("MoeniusEffect", float) = 0
        _SobelLightThreshold ("Sobel Light Threshold", float) = 0
        _SobelLightThickness ("SobelLigjtormal Thickness", float)=0
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
            #include "AutoLight.cginc"

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
            sampler2D _CameraDepthTexture;
            sampler2D _MainTex;
            sampler2D _CameraGBufferTexture0;
            sampler2D _CameraGBufferTexture1;
            sampler2D _CameraGBufferTexture2;
            sampler2D _CameraGBufferTexture3;

            float4 _DirectionalLightPosition;

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
                float2 ResultIn = 0;
                [unroll]
                for (int i=0;i<9;++i) {
                    float4 v = tex2D(_CameraDepthNormalsTexture, uv + SobleNeighborsSamples[i]*Thickness);
                    float depth;
                    float3 normal;
                    DecodeDepthNormal(v, depth,normal);
                    ResultIn += Linear01Depth(depth) * float2(SobelKernelX[i], SobelKernelY[i]);
                }
                Result = length(ResultIn);
            }

            void ComputeSobelLightAtPoint(float2 uv, float Thickness, out float Result, out float angle) {
                float2 ResultIn = 0;
                [unroll]
                for (int i=0;i<9;++i) {
                    float3 lightDir = normalize(_DirectionalLightPosition.xyz);
                    float lightAmount = dot( tex2D(_CameraGBufferTexture2, uv + SobleNeighborsSamples[i] * Thickness), -lightDir);
                    ResultIn += lightAmount * float2(SobelKernelX[i], SobelKernelY[i]);
                }
                Result = length(ResultIn);
                angle = atan2(ResultIn.y, ResultIn.x);
            }


            void ComputeSobelColorAtPoint(float2 uv, float Thickness, out float Result) {
                Result=0;
                float2 SobelR=0;
                float SobelG=0;
                float SobelB=0;
                float3 centerValue = tex2D(_MainTex, uv);
                [unroll]
                for (int i=0;i<9;++i) {
                    float3 col = tex2D(_CameraGBufferTexture0, uv + SobleNeighborsSamples[i]*Thickness);
                    //float3 col = tex2D(_MainTex, uv + SobleNeighborsSamples[i]*Thickness);
                    float Y = 0.2627*col.r + 0.6780*col.g + 0.0593 *col.b;
                    SobelR += Y * float2(SobelKernelX[i], SobelKernelY[i]);
                 //   SobelR += dot(col, centerValue) * float2(SobelKernelX[i], SobelKernelY[i]);
                 //   SobelR += col.r * float2(SobelKernelX[i], SobelKernelY[i]);
                 //   SobelG += col.g * float2(SobelKernelX[i], SobelKernelY[i]);
                 //   SobelB += col.b * float2(SobelKernelX[i], SobelKernelY[i]);
                }
                //Result = max(length(SobelR), max(length(SobelG), length(SobelB)));
                //Result = SobelR;
                Result = length(SobelR);
            }

            void ComputeSobelNormalAtPoint(float2 uv, float Thickness, out float Result) {
                Result=0;
                float2 SobelR=0;
                float2 SobelG=0;
                float2 SobelB=0;
                [unroll]
                float4 centerValue = tex2D(_CameraDepthNormalsTexture, uv );
                for (int i=0;i<9;++i) {
                    float4 v = tex2D(_CameraDepthNormalsTexture, uv + SobleNeighborsSamples[i]*Thickness);
                    float depth;
                    float3 col;
                    DecodeDepthNormal(v, depth, col);

                    SobelR += col.r * float2(SobelKernelX[i], SobelKernelY[i]);
                    SobelG += col.g * float2(SobelKernelX[i], SobelKernelY[i]);
                    SobelB += col.b * float2(SobelKernelX[i], SobelKernelY[i]);

                    //SobelR += dot(normalize(centerValue), normalize(col)) * float2(SobelKernelX[i], SobelKernelY[i]);
                }
                Result = max(length(SobelR), max(length(SobelG), length(SobelB)));
                //Result = SobelR;
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

            float _MoebiusThickness;
            float _SobelLightThreshold;
            float _SobelLightThickness;

            float4x4 _UNITY_MATRIX_I_P;
            float4x4 _UNITY_MATRIX_I_V;
            float4x4 _UNITY_MATRIX_I_VP;

           float3 DepthToWorld(float2 uv, float depth) {
                // float4 cPos = float4(uv*2-1, depth, 1.0);
                // float4 vPos = mul(_UNITY_MATRIX_I_P, cPos);
                // vPos /= vPos.w;
                // float4 wPos = mul(_UNITY_MATRIX_I_V, vPos);
                // return wPos;
                // float4 hpositionWS = mul(_UNITY_MATRIX_I_VP, cPos);
                // return hpositionWS.xyz/hpositionWS.w;

                 const float2 p11_22 = float2(unity_CameraProjection._11, unity_CameraProjection._22);
                 const float2 p13_31 = float2(unity_CameraProjection._13, unity_CameraProjection._23);
                 const float isOrtho = unity_OrthoParams.w;
                 const float near = _ProjectionParams.y;
                 const float far = _ProjectionParams.z;

                 #if defined(UNITY_REVERSED_Z)
                     depth = 1 - depth;
                 #endif
                 float vz = near * far / lerp(far, near, depth);

                 float3 vpos = float3((uv * 2 - 1 - p13_31) / p11_22 * vz, -vz);
                 return mul(_UNITY_MATRIX_I_V, float4(vpos, 1));
             }

            fixed4 frag (v2f i) : SV_Target
            {
                float D = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);

                fixed4 col = tex2D(_MainTex, i.uv);
                float Result;
                float ResultDepth;

                float ThicknessFallOff = (1-Linear01Depth(D));

                ComputeSobelAtPoint(i.uv, _Thickness* ThicknessFallOff, ResultDepth);
                if (ResultDepth > _SobelThreshold) {
                    return fixed4(0,0,0,1);
                }

                ComputeSobelColorAtPoint(i.uv, _ColorThickness * ThicknessFallOff, Result);
                if (Result > _SobelColorThreshold) {
                    return fixed4(0,0,0,1);
                }

                ComputeSobelNormalAtPoint(i.uv, _NormalThickness , Result);
                if (Result > _SobelNormalThreshold) {
                    return fixed4(0,0,0,1);
                }

                static const float PI = 3.14159265f;
                float angle;
                ComputeSobelLightAtPoint(i.uv, _SobelLightThickness, Result, angle);
                if (Result > _SobelLightThreshold ) {
                    float4 v = tex2D(_CameraDepthNormalsTexture, i.uv );
                    float depth;
                    float3 normal;
                    DecodeDepthNormal(v, depth,normal);

                    float3 WSPosition = DepthToWorld(i.vertex, depth);
                    float Thickness = _MoebiusThickness * Result;
                    //return col *  0;
                }

                    float3 lightDir = normalize(_DirectionalLightPosition.xyz);
                    float4 normal2 = tex2D(_CameraGBufferTexture2, i.uv);
                    float lightAmount = dot( normal2 , -lightDir);

                    if (D <= 0) return col;
                    float3 WSPosition = DepthToWorld(i.uv, D);
                    if (lightAmount < 0.05) return col *0.5;
                    if (lightAmount > 0.1) {
                        return col;
                    } else {
                      if (lightAmount != 0) {
                          float Thickness = _MoebiusThickness * max(trunc((lightAmount/0.1)*10)/10, 0.5);
                          return col * ((abs(WSPosition.y)%Thickness) > (Thickness/2) ? 1 : 0.5) ;
                      } else {
                          return col;
                      }
                    }
            }
            ENDCG
        }
    }
}
