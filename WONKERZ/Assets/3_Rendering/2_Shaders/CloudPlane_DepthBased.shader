Shader "Custom/CloudPlane_DepthBased"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0

        _NoiseTexture("Noise", 2D) = "white" {}
        _MaterialID("MaterialID", Int) = 0

        _FogStrength("FogStrength", Float) = 1
    }
    SubShader
    {
        Pass
        {
            Tags { "LightMode"="ForwardBase" "RenderType"="Transparent" "Queue"="Transparent" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZTest LEqual
            ZClip On
            ZWrite Off
            Cull Off
            LOD 200

            CGPROGRAM

            sampler2D _NoiseTexture;
            float4 _NoiseTexture_ST;

            float _FogStrength;

            #define _ALPHABLEND_ON 1
            #define UNITY_REQUIRE_FRAG_WORLDPOS 1
            #define SCHNIBBLE_FRAG_FW surf

            #include "../../SCHNIBBLE/Shaders/Deferred/SchnibbleCustomGBuffer.cginc"
            #include "../../SCHNIBBLE/Shaders/Deferred/SchnibbleCustomGBufferInputs.cginc"
            #include "../../SCHNIBBLE/Shaders/Tools/Functions.cginc"
            
		    #include "../../SCHNIBBLE/Shaders/Deferred/SchnibbleCustomGBufferForwardPass.cginc"

            void surf (inout SchInputForwardBase i, inout SchnibbleGBufferForward buffer)
            {		
                float3 planeWorldPos = i.worldPos;

                // world pos to screen pos
                float4 proj = mul(UNITY_MATRIX_VP, float4(planeWorldPos, 1.0));
                proj.y *= -1;
                float2 uv = (proj.xy / proj.w) * 0.5 + 0.5;

                float depth = (tex2D(_CameraDepthTexture, uv));

                // depth to world pos
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

	            float3 depthFragWorldPos = mul(UNITY_MATRIX_I_V, float4(vpos, 1));

                buffer.writeOIT = 0;

                // vertical fog :
                // based on vertical distance between plane and frag.
                float deltaY = abs(planeWorldPos.y - depthFragWorldPos.y);
                // depth fog :
                // based on depth distance between plane and frag.
                float deltaDepth = abs(planeWorldPos.z - depthFragWorldPos.z);
                // transmissive fog :
                // base on ray distance between eye plane and frag.
                // aka ray marching, but with only one slice for now.
                float3 cameraRayDir = normalize(depthFragWorldPos - _WorldSpaceCameraPos);
                //float deltaRay = (LinearEyeDepth(i.pos.z) - LinearEyeDepth(depth)) * (far - near);
                float deltaRay = mul(unity_CameraInvProjection, i.pos).z - vpos.z;

                float final = max(deltaY, deltaDepth);                
                final = deltaRay;
                float noise_ = tex2D(_NoiseTexture, planeWorldPos.xz * _NoiseTexture_ST.xy + _NoiseTexture_ST.zw + float2(_Time.x, 0));
                final *= _FogStrength * noise_;
                final = sqrt(final) + (deltaY * 0.002);

                buffer.albedo = _Color * clamp(noise_, 0.8, 1.0);
                buffer.alpha = clamp(final, 0, 1);

            }

            ENDCG
            
        }


        Pass {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite Off ZTest LEqual

            CGPROGRAM
            #pragma target 3.0

            // -------------------------------------


            #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature_local _METALLICGLOSSMAP
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature_local _PARALLAXMAP
            #pragma multi_compile_shadowcaster
            #pragma multi_compile_instancing
            // Uncomment the following line to enable dithering LOD crossfade. Note: there are more in the file to uncomment for other passes.
            //#pragma multi_compile _ LOD_FADE_CROSSFADE

            #pragma vertex vertShadowCaster
            #pragma fragment fragShadowCaster

            #include "UnityStandardShadow.cginc"

            ENDCG
        }
    }
    FallBack "Diffuse"
}
