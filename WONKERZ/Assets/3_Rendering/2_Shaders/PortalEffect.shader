Shader "Custom/PortalEffect"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Roughness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0

        _RippleOrigin("Ripple Origin", Vector) = (0,0,0,0)
        _RippleScale("Ripple Scale", Float) = 1
        _RippleSpeed("Ripple Speed", Float) = 1
        _RippleFrequency("Ripple Frequency", Float) = 1

    }
    SubShader
    {
    Pass {
        Tags{ "RenderType"="Transparent" "Queue"="Transparent" "PreviewType"="Plane"}
        LOD 200

        ColorMask 0

        CGPROGRAM

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        fixed4 _RippleOrigin;
        float _RippleScale;
        float _RippleSpeed;
        float _RippleFrequency;

        #define SCHNIBBLE_FRAG_FW surf
        #define SCHNIBBLE_VERT_FW vert

        #include "UnityStandardCore.cginc"
		#include "../../SCHNIBBLE/Shaders/Deferred/SchnibbleCustomGBuffer.cginc"
		#include "../../SCHNIBBLE/Shaders/Deferred/SchnibbleCustomGBufferInputs.cginc"
		#include "../../SCHNIBBLE/Shaders/Tools/Functions.cginc"
        #include "../../SCHNIBBLE/Shaders/Deferred/SchnibbleCustomOIT.cginc"
		#include "../../SCHNIBBLE/Shaders/Deferred/SchnibbleCustomGBufferForwardPass.cginc"

		void vert (inout VertexInput v) {
            half offsetVert = ((v.vertex.x * v.vertex.x) + (v.vertex.z * v.vertex.z));
            half value = _RippleScale * sin(_Time.w  * _RippleSpeed + offsetVert * _RippleFrequency);
            v.vertex.y += value;
        }

        void surf (inout SchInputForwardBase i, inout SchnibbleGBufferForward o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, i.uv) * _Color;
            o.albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.metallic = _Metallic;
            o.roughness = _Roughness;
            o.alpha = c.a;
            o.writeOIT = 1;
        }
        ENDCG
        }
    }
    FallBack "Diffuse"
}
