Shader "Custom/ProjectOnDepth"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
		[Enum(UnityEngine.Rendering.CompareFunction)] _Ztest("ztest", Float) = 2
        [Enum(UnityEngine.Rendering.BlendOp)] _BlendOp("blend op", Float) = 0
        [Enum(Zero,0,RGBA,15,A,1,R,2,G,4,B,8,RGB,14)] _ColorMask("color mask", Float) = 15

		// Blending state
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("blendsrc", Float) = 1.0
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("blenddst", Float) = 0.0
        [Toggle] _ZWrite ("zwrite", Float) = 1.0
        [Enum(UnityEngine.Rendering.CullMode)]_CullMode("CullMode", Float) = 2
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
            Blend [_SrcBlend] [_DstBlend]
            BlendOp [_BlendOp]
            ZWrite [_ZWrite]
            ZTest [_Ztest]
            Cull [_CullMode]
            ColorMask [_ColorMask]

        CGPROGRAM

        #define UNITY_REQUIRE_FRAG_WORLDPOS 1
        #define SCHNIBBLE_FRAG surf


        #include "../Deferred/SchnibbleCustomGBufferInputs.cginc"
        #include "../Deferred/SchnibbleCustomGBuffer.cginc"
        #include "SchnibbleFunctions.cginc"

        #include "../Deferred/SchnibbleCustomGBufferPass.cginc"

        void surf (VertexOutput o, inout SchnibbleGBuffer buffer)
        {
            float2 uv = o.screenPos.xy/o.screenPos.w;
            float3 wpos = DepthToWorld(uv, tex2D(_CameraDepthTexture, uv));
            float3 opos = mul(unity_WorldToObject, float4(wpos,1)).xyz;

			clip (0.5 - abs(opos.xyz));

			o.tex.xy = opos.xz+0.5;

			float4 normal = tex2D(_GBufferNormals, uv);
			fixed3 wnormal = normal.rgb * 2 - 1;
			clip (dot(wnormal, mul((float3x3)unity_ObjectToWorld, o.normalWorld) - 0.3));

			fixed4 col = tex2D(_MainTex, o.tex);

            buffer.albedo = opos;//o.color * _Color;
            buffer.normalWorld = wnormal;
        }


        ENDCG
        }
    }
    FallBack "Diffuse"
}
