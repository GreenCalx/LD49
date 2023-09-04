Shader "Custom/SandTrap"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _AngleAnimationStart ("AngleAnimationStart", Float) = 0
        _AngleAnimationStop ("AngleAnimationStop", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

Pass {
			Tags {
                "LightMode" = "Deferred"
            }

        CGPROGRAM

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0


        #define SCHNIBBLE_NEED_MODEL_POS 1
        #define SCHNIBBLE_FRAG surf
        #include "../../SCHNIBBLE/Shaders/Deferred/SchnibbleCustomGBufferPass.cginc"

        float _AngleAnimationStart;
        float _AngleAnimationStop;


			void surf (VertexOutput o, inout SchnibbleGBuffer buffer){
                float l = length(o.vertex);

                float angle = atan2(o.vertex.x, o.vertex.z) + 3.14;
                // animation swirl
                //buffer.albedo = tex2D(_MainTex, float2(cos(l+angle+_Time.y), sin(l+angle+_Time.y)));
                // animation 2
                //buffer.albedo = tex2D(_MainTex, float2(angle, l+_Time.y));
                buffer.albedo = tex2D(_MainTex, float2(angle, l));
                if (_AngleAnimationStop - _AngleAnimationStart < 0) {
                    if (angle > _AngleAnimationStart && angle < 6.28 || angle < _AngleAnimationStop && angle > 0){
                        buffer.albedo = tex2D(_MainTex, float2(angle, l+_Time.y));
                    }
                }
                else{
                    if (angle > _AngleAnimationStart && angle < _AngleAnimationStop){
                        buffer.albedo = tex2D(_MainTex, float2(angle, l+_Time.y));
                    }
                }
            }
        ENDCG
    }
    }
    FallBack "Diffuse"
}
