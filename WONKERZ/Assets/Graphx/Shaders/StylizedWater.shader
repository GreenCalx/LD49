Shader "Custom/StylizedWater"
{
    Properties
    {
        [Header(Colors)]
        _Color ("Color", Color) = (1,1,1,1)
        _FogColor("Fog Color", Color) = (1,1,1,1)
        _IntersectionColor("Intersection color", Color) = (1,1,1,1)
 
        [Header(Thresholds)]
        _IntersectionThreshold("Intersction threshold", float) = 0
        _FogThreshold("Fog threshold", float) = 0
        _FoamThreshold("Foam threshold", float) = 0
 
        [Header(Normal maps)]
        [Normal]_NormalA("Normal A", 2D) = "bump" {} 
        [Normal]_NormalB("Normal B", 2D) = "bump" {}
        _NormalStrength("Normal strength", float) = 1
        _NormalPanningSpeeds("Normal panning speeds", Vector) = (0,0,0,0)
 
        [Header(Foam)]
        _FoamTexture("Foam texture", 2D) = "white" {} 
        _FoamTextureSpeedX("Foam texture speed X", float) = 0
        _FoamTextureSpeedY("Foam texture speed Y", float) = 0
        _FoamLinesSpeed("Foam lines speed", float) = 0
        _FoamIntensity("Foam intensity", float) = 1
 
        [Header(Misc)]
        _RenderTexture("Render texture", 2D) = "black" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _FresnelPower("Fresnel power", float) = 1
    }
    SubShader
    {
        Pass {
        Tags { "LightMode"="ForwardBase" "RenderType"="Transparent" "Queue"="Transparent" }

        Blend One OneMinusSrcAlpha
        ZWrite Off
        ColorMask 0
        LOD 200
 
        CGPROGRAM

        fixed4 _FogColor;
        fixed4 _IntersectionColor;
 
        float _IntersectionThreshold;
        float _FogThreshold;
        float _FoamThreshold;
 
        sampler2D _NormalA;
        sampler2D _NormalB;
        float4 _NormalA_ST;
        float4 _NormalB_ST;
        float _NormalStrength;
        float4 _NormalPanningSpeeds;
 
        sampler2D _FoamTexture;
        float4 _FoamTexture_ST;
        float _FoamTextureSpeedX;
        float _FoamTextureSpeedY;
        float _FoamLinesSpeed;
        float _FoamIntensity;
 
        sampler2D _RenderTexture;
        float _FresnelPower;
 
        sampler2D _CameraDepthTexture;
        float3 _CamPosition;
        float _OrthographicCamSize;
		
		#define UNITY_REQUIRE_FRAG_WORLDPOS 1
			
			#include "SchnibbleTools/SchnibbleFunctions.cginc"
			#include "Deferred/SchnibbleCustomGBufferInputs.cginc"
			#include "Deferred/SchnibbleCustomGBuffer.cginc"
            #include "Deferred/SchnibbleCustomOIT.cginc"
 
        void surf (inout SchInputForwardBase i, inout SchnibbleGBufferForward o)
        {
            uint fragIdx = GetFragIdx(i.screenPos.xy/i.screenPos.w);
            uint headIdx = _oitFragHeadIdx[fragIdx];

            float3 worldPos = i.worldPos;
            float2 rtUV = worldPos.xz - _CamPosition.xz;
            rtUV = rtUV/(_OrthographicCamSize *2);
            rtUV += 0.5;
            fixed4 rt = tex2D(_RenderTexture, rtUV);
 
            //float depth = tex2Dproj(_CameraDepthTexture, i.screenPos);

            float depth = _oitFragDepth[headIdx].depth;
            if(headIdx == 0) depth = LinearEyeDepth(0);
            //depth = LinearEyeDepth(depth);
 
            //float diff = (depth - Linear01Depth(i.screenPos.w));
            float waterSurface = LinearEyeDepth(i.pos.z);
            float diff = (depth - waterSurface);
            float intersectionDiff = 0;
            float fogDiff = 1;
            // TODO toffa : Remlove this if by mul and divide if perf bottleneck
            // This is done to avoid misinterpretation of object above the water level
            if(diff >= 0) { 
                intersectionDiff = saturate(diff / _IntersectionThreshold);
                fogDiff = saturate(diff / _FogThreshold);
            }


            float foamDiff = saturate(diff / _FoamThreshold);;
            foamDiff *= (1.0 - rt.b);
             
            fixed4 c = lerp(lerp(_IntersectionColor, _Color, intersectionDiff), _FogColor, fogDiff);
             
            float foamTex = tex2D(_FoamTexture, worldPos.xz * _FoamTexture_ST.xy + _Time.y * float2(_FoamTextureSpeedX, _FoamTextureSpeedY));
            float foam = step(foamDiff - (saturate(sin((foamDiff - _Time.y * _FoamLinesSpeed) * 8 * UNITY_PI)) * (1.0 - foamDiff)), foamTex) * (diff>0);
 
            float fresnel = pow(1.0 - saturate(dot(o.normalWorld, normalize(i.viewDir))), _FresnelPower);
 
            o.albedo = c;
            float3 normalA = UnpackNormalWithScale(tex2D(_NormalA, worldPos.xz * _NormalA_ST.xy + _Time.y * _NormalPanningSpeeds.xy + rt.rg), _NormalStrength);
            float3 normalB = UnpackNormalWithScale(tex2D(_NormalB, worldPos.xz * _NormalB_ST.xy + _Time.y * _NormalPanningSpeeds.zw + rt.rg), _NormalStrength);
            o.normalWorld = normalA + normalB;
            o.roughness = _Glossiness * saturate(1/pow(i.screenPos.w,0.2));
            o.alpha = lerp(lerp(c.a * fresnel, 1.0, foam), _FogColor.a, fogDiff);
            //o.alpha = (diff>0);
            o.emission = foam * _FoamIntensity * saturate(1/ pow(i.screenPos.w, 1));
            o.writeOIT = 1;
        }
		
		#define CUSTOM_SHADER_FUNCTION_FRAG(input, output) surf(input, output)
		#include "Deferred/SchnibbleCustomGBufferForwardPass.cginc"

        ENDCG
        }
    }
    FallBack "Diffuse"
}
