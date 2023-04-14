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
			#include "SchnibbleFunctions.cginc"
			
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
            
			float3 _LightDir;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
				fixed4 outlineCol = tex2D(_Outlines, i.uv);
				if (outlineCol.a > 0.2) {
				    col = outlineCol;
				}
				
				SchnibbleGBuffer schGBuffer;
				GetGBuffer(i.uv, schGBuffer);
				
				SchnibbleCustomGPUData schParams;
				GetSchData(schGBuffer.matId, schParams);
	
				float3 hatch = float3(1,1,1);	
				if (schParams.enableCrossHatch) 
				{ 
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
				    // todo toffa : avoid ifs...
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
