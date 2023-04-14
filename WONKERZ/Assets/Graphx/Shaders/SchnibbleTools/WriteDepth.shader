Shader "Hidden/WriteDepth"
{
    SubShader
    {
        // No culling or depth
        Cull Back ZWrite On ZTest Less ColorMask 0

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct vf
            {
                float4 vertex : SV_POSITION;
            };

            vf vert (appdata v)
            {
                vf o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (vf i) : SV_Target
            {
                return fixed4(0,0,0,0);
            }
            ENDCG
        }
    }
}
