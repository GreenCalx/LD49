Shader "Custom/ImpactDamage"
{
    Properties
    {
	    _Color("Color", Color) = (1,1,1,1)

		_MainTex("Main Texture", 2D) = "white" {}

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

		// Impact Damage specs
		_DamageTexAtlas ("Damage Texture Atlas", 2D) = "white" {}
		_DamageTexID("Row's Texture ID", Int) = 0

        [IntRange]_MaterialID("MaterialID", Range(0,255)) = 0
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
			
			//sampler2D _MainTex;
			fixed4 _BumpMap_ST;
			
			sampler2D _DamageTexAtlas;
			float4 _DamageTexAtlas_TexelSize;
			fixed4 _DamageTexAtlas_ST;
			int _DamageTexID;
			
			#define UNITY_REQUIRE_FRAG_WORLDPOS 1

            #define SCHNIBBLE_FRAG surf

			#include "../../SCHNIBBLE/Shaders/Deferred/SchnibbleCustomGBufferPass.cginc"

			
			#include "../../SCHNIBBLE/Shaders/Tools/Functions.cginc"
			#include "../../SCHNIBBLE/Shaders/Deferred/SchnibbleCustomGBufferInputs.cginc"
			#include "../../SCHNIBBLE/Shaders/Deferred/SchnibbleCustomGBuffer.cginc"

			void surf (VertexOutput o, inout SchnibbleGBuffer buffer)
			{		
				half2 uv = o.tex.xy ;

				half2 uv_impact = uv *_DamageTexAtlas_ST.xy + (1-_DamageTexAtlas_ST.xy) * 0.5 + _DamageTexAtlas_ST.zw;
				//half2 uv_impact = uv *_DamageTexAtlas_ST.xy;

				float4 textureWoodColor = tex2D(_MainTex, uv );

				float atlas_count= _DamageTexAtlas_TexelSize.z / _DamageTexAtlas_TexelSize.w;

				float inv_atlas_count = 1 / atlas_count;
				float min = _DamageTexID * inv_atlas_count;
				float max = min + inv_atlas_count;

				uv_impact.x = uv_impact.x * (max-min)+min;

				float4 textureDamage = tex2D(_DamageTexAtlas, uv_impact);

				buffer.albedo = lerp(textureWoodColor, textureDamage, textureDamage.a);

				// float3 worldPos = IN_WORLDPOS(o);
				// half3 normalMap = UnpackNormalWithScale(tex2D(_BumpMap,TRANSFORM_TEX(uv, _BumpMap)), _BumpScale);
				// buffer.normalWorld = normalMap;
			}
			

			
			ENDCG
		}
	}
    FallBack "Diffuse"
}
