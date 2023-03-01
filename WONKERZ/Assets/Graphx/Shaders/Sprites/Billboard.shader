Shader "Sprites/Billboard"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "IgnoreProjector"="True" "DisableBatching" = "True"}

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                //copy them so we can change them (demonstration purpos only)
                float4x4 m = UNITY_MATRIX_M;
                float4x4 vv = UNITY_MATRIX_V;
                float4x4 p = UNITY_MATRIX_P;
                //break out the axis
                float3 right = normalize(vv._m00_m01_m02);
                float3 up = normalize(vv._m10_m11_m12);;
                float3 forward = normalize(vv._m20_m21_m22);
                //get the rotation parts of the matrix
                float4x4 rotationMatrix = float4x4(
                    right, 0,
                    up, 0,
                    forward, 0,
                    0, 0, 0, 1);

                float4x4 rotationMatrixInverse = transpose(rotationMatrix);
                float4 pos = v.vertex;
                pos = mul(rotationMatrixInverse, pos);
                pos = mul(m, pos);
                pos = mul(vv, pos);
                pos = mul(p, pos);
                // o.vertex = pos;
                o.vertex = mul(UNITY_MATRIX_P,
              mul(UNITY_MATRIX_MV, float4(0.0, 0.0, 0.0, 1.0))
              + float4(v.vertex.x, v.vertex.y, 0.0, 0.0)
                );

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                return col * _Color;
            }
            ENDCG
        }
    }
}
