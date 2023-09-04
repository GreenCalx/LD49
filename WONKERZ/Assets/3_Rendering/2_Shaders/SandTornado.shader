Shader "Custom/SandTornado"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _NoisePattern ("Noise Pattern", 2D) = "white" {}
        _NoiseMul ("Noise Mul", float) = 1
        _NormalScale ("Normal Scale", float) = 1
        _RotationSpeed ("RotationSpeed", float) = 1
        _CenterWS ("Center", Vector ) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows alpha:auto vertex:vert

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _NormalLow;
        sampler2D _NoisePattern;

        float _NormalScale;

        float _NoiseMul;

        float4 _CenterWS;

        float _RotationSpeed;
        struct Input
        {
            float2 uv_NoisePattern;
            float3 worldPos;
        };

        fixed4 _Color;

        void vert (inout appdata_full v) {
            float height = (1-v.vertex.y/80);
            v.vertex.x += 15 * pow(height,2) * sin(_Time.y + pow(height,3)*5);
            v.vertex.z += 15 * pow(height,2) * sin(_Time.z + pow(height,3)*5);
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // compute uv by hand, this is not good but a quick way to do it
            float radius = 40;
            float3 center = float3(_CenterWS.x - radius, _CenterWS.y, _CenterWS.z);
            float height = 80;
            float3 centerPos = float3(center.x, IN.worldPos.y, center.z);

            float3 A = normalize(IN.worldPos-centerPos);
            float3 B = float3(1,0,0);
            float3 N = float3(0,1,0);
            float angle = atan2(dot(cross(A,B), N), dot(A,B))/1.57;

            float v = (center.y - IN.worldPos.y)/height;
            float u = angle;

            float noise = tex2D(_NoisePattern, float2(u,v) * 1/_NormalScale + float2((pow(v,4)*10)+_Time.y,_Time.y+v*3)*_RotationSpeed).x;
            o.Albedo = noise * _Color.rgb;
            o.Metallic = 0;
            o.Smoothness = 0;
            o.Alpha = _Color.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
