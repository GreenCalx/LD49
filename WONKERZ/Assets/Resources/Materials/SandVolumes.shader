Shader "Custom/SandVolumes"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _NoisePattern ("Noise Pattern", 2D) = "white" {}
        _NoiseMul ("Noise Mul", float) = 1
        _NormalScale ("Normal Scale", float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows alpha:fade

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _NormalLow;
        sampler2D _NoisePattern;

        float _NormalScale;

        float _NoiseMul;
        struct Input
        {
            float3 worldPos;
        };

        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float noise = tex2D(_NoisePattern, IN.worldPos.xz * (1/_NormalScale) + float2(-_Time.y,0)).x;
            o.Albedo = noise * _Color.rgb;
            o.Metallic = 0;
            o.Smoothness = 0;
            o.Alpha = _Color.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
