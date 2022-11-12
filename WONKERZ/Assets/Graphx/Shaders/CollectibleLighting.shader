Shader "Custom/CollectibleLighting"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _AmbientLightColor ("Ambient Color", Color) = (0.5,0.5,0.5,1)
        _DirLightColor ("Light Color", Color) = (1,1,1,1)
        _DirLightVec("Light Direction", Vector) = (-0.629,-0.629,-0.629,0)
    }
    SubShader
    {
        //Tags { "RenderType"="Opaque" }
        //LOD 200

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            // Use shader model 3.0 target, to get nicer looking lighting
            //#pragma target 3.0
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed3 normal : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4  _MainTex_ST;

            fixed3 _AmbientLightColor, _DirLightColor;
            half3 _DirLightVec;

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                fixed3 lighting : TEXCOORD1;
            };

            v2f vert(appdata i)
            {
                v2f output;
                output.pos = UnityObjectToClipPos(i.vertex);
                output.uv = TRANSFORM_TEX(i.uv, _MainTex);

                half3 worldNormal = UnityObjectToWorldNormal(i.normal);
                // projected 
                half3 viewNormal = mul((float3x3)UNITY_MATRIX_V, worldNormal);

                half3 directionalShading = saturate(dot(viewNormal, -_DirLightColor));
                output.lighting = saturate( _AmbientLightColor + directionalShading + _DirLightColor);

                return output;
            }

            fixed4 frag(v2f i) : SV_TARGET
            {
                fixed4 color = tex2D(_MainTex, i.uv);
                color.rgb *= i.lighting;
                return color;
            }

            //UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
            //UNITY_INSTANCING_BUFFER_END(Props)

            ENDCG
        }
    }
    FallBack "Diffuse"
}
