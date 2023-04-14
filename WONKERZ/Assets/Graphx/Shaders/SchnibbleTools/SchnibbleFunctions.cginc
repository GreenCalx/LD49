#ifndef SCHNIBBLE_FUNCTIONS_INCLUDED
#define SCHNIBBLE_FUNCTIONS_INCLUDED

#ifdef SCHNIBBLE_VERT
    void SCHNIBBLE_VERT(inout SchVertexInput v);
    #define SCHNIBBLE_CUSTOM_VERT(i) SCHNIBBLE_VERT(i)
#else
    #define SCHNIBBLE_CUSTOM_VERT(i)
#endif

#ifdef SCHNIBBLE_FRAG
    void SCHNIBBLE_FRAG(VertexOutput o, inout SchnibbleGBuffer buffer);
    #define SCHNIBBLE_CUSTOM_FRAG(o,b) SCHNIBBLE_FRAG(o,b);
#else
    #define SCHNIBBLE_CUSTOM_FRAG(o,b)
#endif

float3 DepthToWorld(float2 uv, float depth) {
	const float2 p11_22 = float2(unity_CameraProjection._11, unity_CameraProjection._22);
	const float2 p13_31 = float2(unity_CameraProjection._13, unity_CameraProjection._23);
	const float isOrtho = unity_OrthoParams.w;
	const float near = _ProjectionParams.y;
	const float far = _ProjectionParams.z;

	#if defined(UNITY_REVERSED_Z)
		depth = 1 - depth;
	#endif
	float vz = near * far / lerp(far, near, depth);

	float3 vpos = float3((uv * 2 - 1 - p13_31) / p11_22 * vz, -vz);
	return mul(UNITY_MATRIX_I_V, float4(vpos, 1));
}

inline float4 SchComputeScreenPos(float4 pos) {
    float4 o = pos * 0.5f;
    o.xy = float2(o.x, o.y*_ProjectionParams.x) + o.w;
    o.zw = pos.zw;
    return o;
}

float3 SchNormalTangentToWorld(half3 normalTangent, float4 tangentToWorld[3])
{
    half3 tangent = tangentToWorld[0].xyz;
    half3 binormal = tangentToWorld[1].xyz;
    half3 normal = tangentToWorld[2].xyz;

    #if UNITY_TANGENT_ORTHONORMALIZE
        normal = normalize(normal);

        // ortho-normalize Tangent
        tangent = normalize (tangent - normal * dot(tangent, normal));

        // recalculate Binormal
        half3 newB = cross(normal, tangent);
        binormal = newB * sign (dot (newB, binormal));
    #endif

    float3 normalWorld = normalize(tangent * normalTangent.x + binormal * normalTangent.y + normal * normalTangent.z); // @TODO: see if we can squeeze this normalize on SM2.0 as well
    return normalWorld;
}

float3x3 GetBasis(float3 normal)
{
	float3 Normal = normal;
	float3 Tangent = float3(0,0,0);
	float3 BiTangent = float3(0,0,0);

	if (abs(Normal.x) > abs(Normal.y))
	{
		// The new X-axis is at right angles to the world Y-axis.
		Tangent = float3(Normal.z, 0, -Normal.x);
		// The new Y-axis is at right angles to the new X- and Z-axes.
		BiTangent = float3(Normal.y * Tangent.x,
								Normal.z * Tangent.x - Normal.x * Tangent.z,
								-Normal.y * Tangent.x);
	}
	else
	{
		// The new X-axis is at right angles to the world X-axis.
		Tangent = float3(0, -Normal.z, Normal.y);
		// The new Y-axis is at right angles to the new X- and Z-axes.
		BiTangent = float3(Normal.y * Tangent.z - Normal.z * Tangent.y,
								 -Normal.x * Tangent.z,
								 Normal.x * Tangent.y);
	}

	return float3x3(BiTangent, Normal, Tangent);
}

float3 RGBToHSV(float3 c)
{
	float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
	float4 p = lerp( float4( c.bg, K.wz ), float4( c.gb, K.xy ), step( c.b, c.g ) );
	float4 q = lerp( float4( p.xyw, c.r ), float4( c.r, p.yzx ), step( p.x, c.r ) );
	float d = q.x - min( q.w, q.y );
	float e = 1.0e-10;
	return float3( abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

float3 HSVToRGB( float3 c )
{
	float4 K = float4( 1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0 );
	float3 p = abs( frac( c.xxx + K.xyz ) * 6.0 - K.www );
	return c.z * lerp( K.xxx, saturate( p - K.xxx ), c.y );
}

float3 NormalizeColor(float3 c) {
	float sum = c.x + c.y + c.z;
	return float3(c.x/sum, c.y/sum, c.z/sum);
}

#endif
