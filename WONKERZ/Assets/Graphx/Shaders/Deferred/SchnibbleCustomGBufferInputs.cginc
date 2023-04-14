#ifndef SCHNIBBLE_CUSTOM_GBUFFER_INPUTS_INCLUDED
#define SCHNIBBLE_CUSTOM_GBUFFER_INPUTS_INCLUDED

float _Roughness;
float _DepthMask;
float _DepthMaskRead;
sampler2D _DepthMaskTexture;
int _MaterialID;


sampler2D _GBufferNormals;

#include "UnityInstancing.cginc"
#include "UnityStandardInput.cginc"
#include "UnityStandardCore.cginc"

struct SchInputForwardBase {
    float4 pos;
    float3 worldPos;
    float3 viewDir;
    float3 normalWorld;
    float4 screenPos;
};

struct SchVertexInput
{
    float4 vertex   : POSITION;
    fixed4 color    : COLOR;
    half3 normal    : NORMAL;
    float2 uv0      : TEXCOORD0;
    float2 uv1      : TEXCOORD1;
#if defined(DYNAMICLIGHTMAP_ON) || defined(UNITY_PASS_META)
    float2 uv2      : TEXCOORD2;
#endif
#ifdef _TANGENT_TO_WORLD
    half4 tangent   : TANGENT;
#endif
    UNITY_VERTEX_INPUT_INSTANCE_ID
};


float4 SchTexCoords(SchVertexInput v)
{
    float4 texcoord;
    texcoord.xy = TRANSFORM_TEX(v.uv0, _MainTex); // Always source from uv0
    texcoord.zw = TRANSFORM_TEX(((_UVSec == 0) ? v.uv0 : v.uv1), _DetailAlbedoMap);
    return texcoord;
}

struct VertexOutput
{
    float4 pos : POSITION;
    float4 color : COLOR;
	//UNITY_POSITION(pos);
	float4 tex        : TEXCOORD0;
	half3 normalWorld : NORMAL;
	float4 screenPos  : TEXCOORD1;
	float4 tangentToWorldAndPackedData[3] : TEXCOORD2;    // [3x3:tangentToWorld | 1x3:viewDirForParallax or worldPos]
	#if UNITY_REQUIRE_FRAG_WORLDPOS && !UNITY_PACK_WORLDPOS_WITH_TANGENT
        float3 posWorld                     : TEXCOORD6;
    #endif
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

#endif
