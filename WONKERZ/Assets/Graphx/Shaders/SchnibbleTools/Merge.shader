Shader "Custom/Merge"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_LightPass ("Texture", 2D) = "white" {}
		_Outlines("Texture", 2D) = "white" {}
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
			#include "../Deferred/SchnibbleCustomGBuffer.cginc"
			
			sampler2D _CameraDepthTexture;

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
			sampler2D _Outlines;
            float4x4 _UNITY_MATRIX_I_V;
			float3 _LightDir;

           float3 DepthToWorld(float2 uv, float depth) {
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
			 
		float3x3 GetBasis(float3 normal)
        {
            float3 Normal = normal;
            float3 Tangent = float3(0,0,0);
            float3 BiTangent = float3(0,0,0);

            if (abs(Normal.x) > abs(Normal.y))
            {
                // The new X-axis is at right angles to the world Y-axis.
                Tangent = float3(Normal.z, 0, -Normal.x);
                // The new Y-axis is at right angles to the new X- and Z-axes.
                BiTangent = float3(Normal.y * Tangent.x,
                                        Normal.z * Tangent.x - Normal.x * Tangent.z,
                                        -Normal.y * Tangent.x);
            }
            else
            {
                // The new X-axis is at right angles to the world X-axis.
                Tangent = float3(0, -Normal.z, Normal.y);
                // The new Y-axis is at right angles to the new X- and Z-axes.
                BiTangent = float3(Normal.y * Tangent.z - Normal.z * Tangent.y,
                                         -Normal.x * Tangent.z,
                                         Normal.x * Tangent.y);
            }

            return float3x3(BiTangent, Normal, Tangent);
        }
			 
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
				fixed4 outlineCol = tex2D(_Outlines, i.uv);
				if (outlineCol.a > 0.2) {
				    col = outlineCol;
				}
				
				// unpack Gbuffer
	GBuffer gbuffer;
    gbuffer.gBuffer0 = tex2D (_CameraGBufferTexture0, i.uv);
    gbuffer.gBuffer1 = tex2D (_CameraGBufferTexture1, i.uv);
    gbuffer.gBuffer2 = tex2D (_CameraGBufferTexture2, i.uv);
    
	SchnibbleGBuffer schGBuffer;
	UnpackSchnibbleGBuffer(gbuffer, schGBuffer);
                
    SchnibbleCustomGPUData schParams = _SchnibbleMaterialData[schGBuffer.matId];
	
	float3 hatch = float3(1,1,1);
	if (schParams.enableCrossHatch) { 
	 
	
	float3 normalWorld = schGBuffer.normalWorld;
	
	float4 lightBuffer = tex2D(_MainTex, i.uv);

    // todo toffa : compute this outside the shader come on...
	float3x3 lightBasis = GetBasis(_LightDir*-1);
	
	float3 worldPos = mul( lightBasis, DepthToWorld(i.uv, SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv)));
	
            float hatchBlendPower = schParams.hatch.blendPower;
			float hatchLightPower = schParams.hatch.lightPower;
			
			float hatchMaxSize  = schParams.hatch.maxSize;
			float hatchMinSize  = schParams.hatch.minSize;
			
            float spaceSize = schParams.hatch.spaceSize;
			
			float totalBaseSize = hatchMaxSize + spaceSize;
			
			float currentScale = lerp(hatchMaxSize, hatchMinSize, saturate(lightBuffer.a) * hatchLightPower);
			currentScale *= 0.5;
			
			float hatchMaxSizeMidPoint = hatchMaxSize * 0.5;
			
			float x = abs(worldPos.x) % (totalBaseSize);
			float y = abs(worldPos.y) % (totalBaseSize);
			float z = abs(worldPos.z) % (totalBaseSize);
			float hatchMax = hatchMaxSizeMidPoint + (currentScale);
            float hatchMin = hatchMaxSizeMidPoint - (currentScale);
			
		    hatch = float3( z <= hatchMax && z >= hatchMin,
                 		    0,
							x <= hatchMax && x >= hatchMin);
			hatch.y = saturate(hatch.x + hatch.z);
			float3 blendWeights = normalize(pow(abs(mul(lightBasis, normalWorld)), hatchBlendPower));
			hatch = hatch.xxx * blendWeights.x + hatch.yyy * blendWeights.y + hatch.zzz * blendWeights.z;
	        

				
			    float3 colorRamp = SampleTextureRGB(float2(lightBuffer.a, RGBToHSV(schGBuffer.albedo).x),
                                        schGBuffer.matId,
										SchTextureToonRampOffset);
				
                if (hatch.x > 0.9)
				    hatch = colorRamp;
				else 
				    hatch = float3(1,1,1);
				  
			}
                return fixed4(hatch * col,1);
            }
            ENDCG
        }
    }
}
