Shader "Custom/DepthMask"
{
    SubShader
    {
        Tags { "Queue"="Geometry-1" "RenderType"="Opaque"}
		ColorMask 0
		ZWrite On
		Stencil {
		    Ref 2
			Comp Always
			Pass Replace
		}
		Pass{
		    Tags{"LightMode" = "Deferred"}
		}
    }
}
