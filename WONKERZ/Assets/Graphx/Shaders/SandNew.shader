Shader "Custom/SandNew"
{
    Properties
    {
	    _Color("Color", Color) = (1,1,1,1)

        [Gamma] _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
        _MetallicGlossMap("Metallic", 2D) = "white" {}

        _Roughness("Roughness", Range(0.0, 1.0)) = 0.5

        _BumpScale("Scale", Float) = 1.0
        [Normal] _BumpMap("Normal Map", 2D) = "bump" {}
		
		_Parallax ("Height Scale", Range (0.005, 0.08)) = 0.02
        _ParallaxMap ("Height Map", 2D) = "black" {}

        _MaterialID("MaterialID", Int) = 0
		
		_DepthMask("DepthMask", Float) = 0
		_DepthMaskRead("DepthMaskRead", Float) = 0
		_Ztest("ztest", Float) = 2
		
		// Blending state
        [HideInInspector] _Mode ("__mode", Float) = 0.0
        [HideInInspector] _SrcBlend ("__src", Float) = 1.0
        [HideInInspector] _DstBlend ("__dst", Float) = 0.0
        [HideInInspector] _ZWrite ("__zw", Float) = 1.0
      
        _NoisePattern ("Noise Pattern", 2D) = "white" {}
        _NoiseMul ("Noise Mul", float) = 1

        _TracesPosition ("Traces Position", 2D) = "white" {}

        _MovingSandsPosition ("Moving Sands Position", 2D) = "white" {}
        _MovingSandsCenter ("Moving Sands center", Vector) = (1,1,1,1)
        _MovingSandsSize ("Moving Sands Size", float) = 1
        _MovingSandsHeightMultiplier ("Moving Sands Height Multiplier", float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
	   
		Pass
        {
			Tags {
                "LightMode" = "Deferred"
            }
			
            Stencil {
			    Ref 192
				WriteMask 207
				Comp Always
				Pass Replace
			}
			
			ZTest Less
			Cull Back
			
            CGPROGRAM
			
			fixed4 _BumpMap_ST;
			sampler2D _NoisePattern;

			float _NoiseMul;

			sampler2D _TracesPosition;
			float4 _TracesCenter;
			float _TracesSize;

			sampler2D _MovingSandsPosition;
			float4 _MovingSandsCenter;
			float _MovingSandsSize;
			float _MovingSandsHeightMultiplier;
			
			#define UNITY_REQUIRE_FRAG_WORLDPOS 1
			
			#include "SchnibbleTools/SchnibbleFunctions.cginc"
			#include "Deferred/SchnibbleCustomGBufferInputs.cginc"
			#include "Deferred/SchnibbleCustomGBuffer.cginc"
			
			void vert (inout SchVertexInput v) {
			    float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
				float2 movingSandsUV = ((worldPos.xz - _MovingSandsCenter.xz) / (_MovingSandsSize*2)) + 0.5;
				float  heightToMove = tex2Dlod(_MovingSandsPosition, float4(movingSandsUV,0,1)).x * _MovingSandsHeightMultiplier;
				
				v.vertex.xyz += float3(0,0,1) * heightToMove;
			}

			void surf (VertexOutput o, inout SchnibbleGBuffer buffer)
			{		
				//float2 noise = float2(_NoiseMul, tex2D(_NoisePattern, o.worldPos.xz * (_NoiseMul - _NoiseMul/10* abs(_SinTime.y))).x);
				
				fixed4 color = _Color;
				float3 worldPos = IN_WORLDPOS(o);
				
				float2 TrailUV = ((worldPos.xz - _TracesCenter.xz) / (_TracesSize*2)) + 0.5;
				float IsTrail = 1-tex2D(_TracesPosition, TrailUV).x;
				
				buffer.albedo = color.rgb * IsTrail;
				
				half3 normalMap = UnpackNormalWithScale(tex2D(_BumpMap,TRANSFORM_TEX(worldPos.xz, _BumpMap)), _BumpScale);
				buffer.normalWorld = SchNormalTangentToWorld(normalMap, o.tangentToWorldAndPackedData) ;
			}
			
			#define CUSTOM_SHADER_FUNCTION_VERT(input) vert(input)
			#define CUSTOM_SHADER_FUNCTION_FRAG(output, gbuffer) surf(output, gbuffer)
			#include "Deferred/SchnibbleCustomGBufferPass.cginc"
			
			
			ENDCG
		}
	}
    FallBack "Diffuse"
}
