Shader "Custom/Outlines"
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
        _ToonShadowMultiplier("Toon Shading Mul", Range(0,1))=0
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
			
			#include "../Deferred/SchnibbleCustomGBuffer.cginc"

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
            float4 _CameraDepthTexture_TexelSize;
            sampler2D _MainTex;
			float4 _MainTex_TexelSize;

            float4 _DirectionalLightPosition;
            float _ToonShadowMultiplier;

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

            static float LaplacianKernel[9] = {
                -1, -1, -1,
                -1,  8, -1,
                -1, -1, -1
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

            void ComputeLaplacianAtPoint(float2 uv, float Thickness, out float Result) {
                Result = 0;
                [unroll]
                for (int i=0;i<9;++i) {
                    float4 v = tex2D(_CameraDepthNormalsTexture, uv + SobleNeighborsSamples[i]*Thickness);
                    float depth;
                    float3 normal;
                    DecodeDepthNormal(v, depth,normal);
                    Result += Linear01Depth(depth) * LaplacianKernel[i];
                }
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


#include "SchnibbleFunctions.cginc"

            fixed4 frag (v2f i) : SV_Target
            {
                float D = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);

                fixed4 col = tex2D(_MainTex, i.uv);
				
                float Result;
                float ResultDepth;

                float2 texSize = _CameraDepthTexture_TexelSize.xy;

                for (int j=0; j<9; ++j)
                    SobleNeighborsSamples[j] *= texSize;

                float ThicknessFallOff = (1-Linear01Depth(D));
                ThicknessFallOff = 1;
				
				int matid = GetMaterialId(i.uv);
                
                SchnibbleCustomGPUData schParams = _SchnibbleMaterialData[matid];
				if (schParams.enableOutlineDepth) {
					ComputeLaplacianAtPoint(i.uv, schParams.outlineDepth.width * ThicknessFallOff, ResultDepth);
					if (ResultDepth > schParams.outlineDepth.threshold) {
						return schParams.outlineDepth.lightColor;
					}
				}

                if(schParams.enableOutlineColor) {
       			   ComputeSobelColorAtPoint(i.uv, schParams.outlineColor.width * ThicknessFallOff, Result);
                   if (Result > schParams.outlineColor.threshold) {
                       return schParams.outlineColor.lightColor;
                   }
				}

                if(schParams.enableOutlineNormal){
                    ComputeSobelNormalAtPoint(i.uv, schParams.outlineNormal.width , Result);
                    if (Result > schParams.outlineNormal.threshold) {
                        return schParams.outlineNormal.lightColor;
                    }
				}

                return float4(0,0,0,0);
                
				
				
				// static const float PI = 3.14159265f;
                // float angle;
                // ComputeSobelLightAtPoint(i.uv, _SobelLightThickness, Result, angle);
                // if (Result > _SobelLightThreshold ) {
                //     float4 v = tex2D(_CameraDepthNormalsTexture, i.uv );
                //     float depth;
                //     float3 normal;
                //     DecodeDepthNormal(v, depth,normal);

                //     float3 WSPosition = DepthToWorld(i.vertex, depth);
                //     float Thickness = _MoebiusThickness * Result;
                //     //return col *  0;
                // }

/*
                    float3 lightDir = normalize(_DirectionalLightPosition.xyz);
                    float4 normal2 = tex2D(_CameraGBufferTexture2, i.uv);
                    float lightAmount = dot( normal2 , -lightDir);
                float lightEffect = 0;

                    if (D <= 0) return col;
                    float3 WSPosition = DepthToWorld(i.uv, D);
                if (lightAmount < 0.05) {
                    lightEffect = 0.5;
                }
                else if (lightAmount > 0.1) {
                    lightEffect = 1;
                } else {
                    float Thickness = _MoebiusThickness * max(trunc((lightAmount/0.1)*10)/10, 0.5);
                    lightEffect =((abs(WSPosition.y)%Thickness) > (Thickness/2) ? 1 : 0.5);
                }

                return lerp(col, col * lightEffect, _ToonShadowMultiplier);
*/
            }
            ENDCG
		}
			
			Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
			
			#include "../Deferred/SchnibbleCustomGBuffer.cginc"

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

            sampler2D _MainTex;
			float4 _MainTex_TexelSize;

            // this kernel is the same when used in x or y dimension			
            static float BlurKernel1D[7]= {
                0.003, 0.048, 0.242, 0.415, 0.242, 0.048, 0.003
            };
			static float BlurNeighbors1D[7]= {
			    -3, -2, -1, 0, 1, 2, 3  
			};
			
			float4 Convolute(float2 uv) {
                float4 Result = float4(0,0,0,0);
                [unroll]
                for (int i=0;i<7;++i) {
                    float4 v = tex2D(_MainTex, uv + float2(BlurNeighbors1D[i], 0)*_MainTex_TexelSize.xy);
                    Result += v * BlurKernel1D[i];
                }
                return Result;
			}

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return Convolute(i.uv);
            }
            ENDCG
        }
		
			Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
			
			#include "../Deferred/SchnibbleCustomGBuffer.cginc"

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

            sampler2D _MainTex; 
            float4 _MainTex_TexelSize;
            // this kernel is the same when used in x or y dimension			
            static float BlurKernel1D[7]= {
                0.003, 0.048, 0.242, 0.415, 0.242, 0.048, 0.003
            };
			
			static float BlurNeighbors1D[7] ={
			    -3, -2, -1, 0, 1, 2, 3  
			};
			
			float4 Convolute(float2 uv) {
                float4 Result = float4(0,0,0,0);
                [unroll]
                for (int i=0;i<7;++i) {
                    float4 v = tex2D(_MainTex, uv + float2(0, BlurNeighbors1D[i])*_MainTex_TexelSize.xy);
                    Result += v * BlurKernel1D[i];
                }
                return Result;
			}

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return Convolute(i.uv);
            }
            ENDCG
        }
		
		Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
			
			#include "../Deferred/SchnibbleCustomGBuffer.cginc"

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

            sampler2D _MainTex; 
            float4 _MainTex_TexelSize;
			
			static float Dilate1D[7] ={
			    -3, -2, -1, 0, 1, 2, 3
			};
			
			float4 Convolute(float2 uv) {
                float4 Result = float4(0,0,0,0);
                [unroll]
                for (int i=0;i<7;++i) {
                    float4 v = tex2D(_MainTex, uv + float2(0, Dilate1D[i])*_MainTex_TexelSize.xy);
                    Result = float4(max(v.r, Result.r), max(v.g, Result.g), max(v.b, Result.b), max(v.a, Result.a));
                }

				[unroll]
                for (int j=0;j<7;++j) {
                    float4 v = tex2D(_MainTex, uv + float2( Dilate1D[j], 0)*_MainTex_TexelSize.xy);
                    Result = float4(max(v.r, Result.r), max(v.g, Result.g), max(v.b, Result.b), max(v.a, Result.a));
                }
                return Result;
			}

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return Convolute(i.uv);
            }
            ENDCG
        }
    }
}
