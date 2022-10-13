Shader "Unlit/UnlitCheckPointPortal"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        
        _Color ("Color", Color) = (1,1,1,1)
        _RippleOrigin("Ripple Origin", Vector) = (0,0,0,0)
        _RippleScale("Ripple Scale", Float) = 1
        _RippleSpeed("Ripple Speed", Float) = 1
        _RippleFrequency("Ripple Frequency", Float) = 1
        _EmissionColor("Emission Color", Float) = 1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        LOD 200

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog
            #pragma alpha:blend

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            
            fixed4 _Color;
            fixed4 _RippleOrigin;
            float _RippleScale;
            float _RippleSpeed;
            float _RippleFrequency;
            float _EmissionColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);

                half offsetVert = ((v.vertex.x * v.vertex.x) + (v.vertex.z * v.vertex.z));
                half value = _RippleScale * sin(_Time.w  * _RippleSpeed + offsetVert * _RippleFrequency);
                o.vertex.y += value;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv)  * _Color;

                col.w = _Color.a;

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
