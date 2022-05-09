// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/SandNew"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _NormalHigh ("Normal High Dunes", 2D) = "white" {}
        _NormalLow ("Normal Low Dunes", 2D) = "white" {}
        _NormalScale ("Normal Scale", float) = 1
        _NormalStrength ( "Normal Strength", float ) = 1
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

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows vertex:vert

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _NormalLow;
        sampler2D _NoisePattern;

        float _NormalScale;
        float _NormalStrength;

        float _NoiseMul;

        sampler2D _TracesPosition;
        float4 _TracesCenter;
        float _TracesSize;

        sampler2D _MovingSandsPosition;
        float4 _MovingSandsCenter;
        float _MovingSandsSize;
        float _MovingSandsHeightMultiplier;

        struct Input
        {
            float2 uv_MainTex : TEXCOORD0;
            float2 uv_NormalLow : TEXCOORD1;
            float3 worldPos;
            float3 worldNormal;
            INTERNAL_DATA
        };

        void vert (inout appdata_full v) {
            float4 ObjectWorldPos = mul(unity_ObjectToWorld, v.vertex);
            float2 MovingSandsUV = ((ObjectWorldPos.xz - _MovingSandsCenter.xz) / (_MovingSandsSize*2)) + 0.5;
            float heightToMove = tex2Dlod(_MovingSandsPosition, float4(MovingSandsUV,0,1)).x * _MovingSandsHeightMultiplier;
            //float heightToMove = _MovingSandsHeightMultiplier;
            v.vertex.xyz += float3(0,0,1) * heightToMove;
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            //float2 noise = float2(_NoiseMul, tex2D(_NoisePattern, IN.worldPos.xz * (_NoiseMul - _NoiseMul/10* abs(_SinTime.y))).x);
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
            float2 TrailUV = ((IN.worldPos.xz - _TracesCenter.xz) / (_TracesSize*2)) + 0.5;
            float IsTrail = 1-tex2D(_TracesPosition, TrailUV).x;
            o.Albedo = c.rgb*IsTrail;
            o.Normal =  UnpackNormalWithScale(tex2D(_NormalLow, IN.worldPos.xz * (1/(_NormalScale))), _NormalStrength) ;
            // Metallic and smoothness come from slider variables
            o.Metallic = 0;
            o.Smoothness = 0;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
