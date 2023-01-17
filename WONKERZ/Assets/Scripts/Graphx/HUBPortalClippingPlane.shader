Shader "Custom/HUBPortalClippingPlane"
{
    Properties{
        _Color ("Tint", Color) = (0, 0, 0, 1)
        _MainTex ("Texture", 2D) = "white" {}
        _Smoothness ("Smoothness", Range(0, 1)) = 0
        _Metallic ("Metalness", Range(0, 1)) = 0
        [HDR]_Emission ("Emission", color) = (0,0,0)

        [HDR]_CutoffColor("Cutoff Color", Color) = (1,0,0,0)
    }

    SubShader{
        Tags{ "RenderType"="Opaque" "Queue"="Geometry"}

        // render all faces
        Cull Off

        CGPROGRAM

        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0 // VFACE

        sampler2D _MainTex;
        fixed4 _Color;

        half _Smoothness;
        half _Metallic;
        half3 _Emission;

        float4 _Plane;
        float  _TriggerPlaneClipping;
        float  _ClipAbovePlane;

        float4 _CutoffColor;

        struct Input 
        {
            float2 uv_MainTex;
            float3 worldPos;
            float facing : VFACE;
        };

        void surf (Input i, inout SurfaceOutputStandard o) 
        {
            //calculate signed distance to plane
            float distance = dot(i.worldPos, _Plane.xyz);
            distance = distance + _Plane.w;

            //discard surface above/below plane if clipping triggered
            if (_TriggerPlaneClipping==1)
            {
                if (_ClipAbovePlane==1)
                    clip(-distance);
                else
                    clip(distance);
            }

            float facing = i.facing * 0.5 + 0.5;
            if (_TriggerPlaneClipping==0)
            {
                facing = 1;
            }

            // classic surf mul by facing
            fixed4 col = tex2D(_MainTex, i.uv_MainTex);
            col *= _Color;
            o.Albedo = col.rgb * facing;
            o.Metallic = _Metallic * facing;
            o.Smoothness = _Smoothness * facing;
            o.Emission = lerp(_CutoffColor, _Emission, facing);
        }
        ENDCG
    }
    FallBack "Standard"
}
