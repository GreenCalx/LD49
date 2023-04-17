Shader "Hidden/SchFastDepthBlit"
{
    Properties
    {
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always ColorMask R

        Pass
        {
            ZWrite Off
            Cull Off
            Blend Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (uint idx : SV_VertexID)
            {
                const float2 vertices[3] = {float2(1,1), float2(-3,1), float2(1, -3)};
                v2f o;

                o.vertex = float4(vertices[idx],0,1);
                o.uv  = o.vertex * float2(0.5f,-0.5f) + 0.5f;

                return o;
            }

                    #if SHADER_API_D3D11
                    #define STENCIL_CHANNEL g
                    #else
                    #define STENCIL_CHANNEL r
                    #endif

            //Texture2D<uint2> _MainTex;
            sampler2D _MainTex;

            half frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                //uint stencil = _MainTex.Load(int3(floor(i.uv.x * 1920), floor(i.uv.y * 1024), 0)).STENCIL_CHANNEL;
                //fixed4 col = float4(0.0,float(stencil)/255.0f,0.0f,1.0f);
                //return col.g;
                return col.xxxx;
            }
            ENDCG
        }
    }
}
