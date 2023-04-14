#ifndef SCHNIBBLE_CUSTOM_GBUFFER_FORWARD_PASS_INCLUDED
#define SCHNIBBLE_CUSTOM_GBUFFER_FORWARD_PASS_INCLUDED

#pragma target 5.0

#pragma shader_feature_local _NORMALMAP
#pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
#pragma shader_feature_fragment _EMISSION
#pragma shader_feature_local _METALLICGLOSSMAP
#pragma shader_feature_local_fragment _DETAIL_MULX2
#pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
#pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
#pragma shader_feature_local_fragment _GLOSSYREFLECTIONS_OFF
#pragma shader_feature_local _PARALLAXMAP

#pragma multi_compile_fwdbase
#pragma multi_compile_fog
#pragma multi_compile_instancing
// Uncomment the following line to enable dithering LOD crossfade. Note: there are more in the file to uncomment for other passes.
//#pragma multi_compile _ LOD_FADE_CROSSFADE


#pragma vertex schvertForwardBase
#pragma fragment schFragBase

#include "UnityStandardCore.cginc"
#include "SchnibbleCustomBRDF.cginc"
#include "SchnibbleCustomGBufferInputs.cginc"
#include "SchnibbleCustomGBuffer.cginc"

#include "SchnibbleCustomOIT.cginc"

struct SchVertexOutputForwardBase
{
    UNITY_POSITION(pos);
    float4 tex                            : TEXCOORD0;
    float4 eyeVec                         : TEXCOORD1;    // eyeVec.xyz | fogCoord
    float4 tangentToWorldAndPackedData[3] : TEXCOORD2;    // [3x3:tangentToWorld | 1x3:viewDirForParallax or worldPos]
    half4 ambientOrLightmapUV             : TEXCOORD5;    // SH or Lightmap UV
    UNITY_LIGHTING_COORDS(6,7)

    // next ones would not fit into SM2.0 limits, but they are always for SM3.0+
#if UNITY_REQUIRE_FRAG_WORLDPOS && !UNITY_PACK_WORLDPOS_WITH_TANGENT
    float3 posWorld                     : TEXCOORD8;
#endif

    float4 screenPos : TEXCOORD9;

    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

SchVertexOutputForwardBase schvertForwardBase (VertexInput v)
{
    UNITY_SETUP_INSTANCE_ID(v);
    SchVertexOutputForwardBase o;
    UNITY_INITIALIZE_OUTPUT(SchVertexOutputForwardBase, o);
    UNITY_TRANSFER_INSTANCE_ID(v, o);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

    float4 posWorld = mul(unity_ObjectToWorld, v.vertex);
    #if UNITY_REQUIRE_FRAG_WORLDPOS
        #if UNITY_PACK_WORLDPOS_WITH_TANGENT
            o.tangentToWorldAndPackedData[0].w = posWorld.x;
            o.tangentToWorldAndPackedData[1].w = posWorld.y;
            o.tangentToWorldAndPackedData[2].w = posWorld.z;
        #else
            o.posWorld = posWorld.xyz;
        #endif
    #endif
    o.pos = UnityObjectToClipPos(v.vertex);
    o.screenPos = ComputeScreenPos(o.pos);

    o.tex = TexCoords(v);
    o.eyeVec.xyz = NormalizePerVertexNormal(posWorld.xyz - _WorldSpaceCameraPos);
    float3 normalWorld = UnityObjectToWorldNormal(v.normal);
    #ifdef _TANGENT_TO_WORLD
        float4 tangentWorld = float4(UnityObjectToWorldDir(v.tangent.xyz), v.tangent.w);

        float3x3 tangentToWorld = CreateTangentToWorldPerVertex(normalWorld, tangentWorld.xyz, tangentWorld.w);
        o.tangentToWorldAndPackedData[0].xyz = tangentToWorld[0];
        o.tangentToWorldAndPackedData[1].xyz = tangentToWorld[1];
        o.tangentToWorldAndPackedData[2].xyz = tangentToWorld[2];
    #else
        o.tangentToWorldAndPackedData[0].xyz = 0;
        o.tangentToWorldAndPackedData[1].xyz = 0;
        o.tangentToWorldAndPackedData[2].xyz = normalWorld;
    #endif

    //We need this for shadow receving
    UNITY_TRANSFER_LIGHTING(o, v.uv1);

    o.ambientOrLightmapUV = VertexGIForward(v, posWorld, normalWorld);

    #ifdef _PARALLAXMAP
        TANGENT_SPACE_ROTATION;
        half3 viewDirForParallax = mul (rotation, ObjSpaceViewDir(v.vertex));
        o.tangentToWorldAndPackedData[0].w = viewDirForParallax.x;
        o.tangentToWorldAndPackedData[1].w = viewDirForParallax.y;
        o.tangentToWorldAndPackedData[2].w = viewDirForParallax.z;
    #endif

    UNITY_TRANSFER_FOG_COMBINED_WITH_EYE_VEC(o,o.pos);
    return o;
}

#include "UnityStandardUtils.cginc"

#ifndef CUSTOM_SHADER_FUNCTION_FRAG
	#define CUSTOM_SHADER_FUNCTION_FRAG(input, output)
#endif

half4 schFragBase (SchVertexOutputForwardBase i) : SV_Target {
//UNITY_APPLY_DITHER_CROSSFADE(i.pos.xy);

FRAGMENT_SETUP(s)
// fragmentt setup copied
    half metallic = _Metallic;
    half smoothness = _Roughness; // this is 1 minus the square root of real roughness m.

    half oneMinusReflectivity;
    half3 specColor;
    half3 diffColor = DiffuseAndSpecularFromMetallic (Albedo(i.tex), metallic, /*out*/ specColor, /*out*/ oneMinusReflectivity);

    FragmentCommonData o = (FragmentCommonData)0;
    s.diffColor = diffColor;
    s.specColor = specColor;
    s.oneMinusReflectivity = oneMinusReflectivity;
    s.smoothness = smoothness;

SchInputForwardBase input;
UNITY_INITIALIZE_OUTPUT(SchInputForwardBase, input);
input.worldPos = s.posWorld;
input.viewDir = -s.eyeVec;
input.normalWorld = s.normalWorld;
input.screenPos = i.screenPos;
input.pos = i.pos;

s.oneMinusReflectivity = 1 - SpecularStrength(s.specColor);

SchnibbleGBufferForward buffer;
UNITY_INITIALIZE_OUTPUT(SchnibbleGBufferForward, buffer);
buffer.albedo = s.diffColor;
buffer.specular = s.specColor;
buffer.normalWorld = s.normalWorld;
buffer.occlusion = Occlusion(i.tex.xy);
buffer.emission = Emission(i.tex.xy);
buffer.roughness = 1-s.smoothness;
buffer.alpha = s.alpha;
buffer.writeOIT = 1;
//	half   metallic;
//	int    matId;

CUSTOM_SHADER_FUNCTION_FRAG(input, buffer);

UNITY_SETUP_INSTANCE_ID(i);

UnityLight mainLight = MainLight ();
UNITY_LIGHT_ATTENUATION(atten, i, s.posWorld);

half4 c = BRDF_Toon(buffer.albedo,
                    buffer.specular,
                    s.oneMinusReflectivity,
                    buffer.roughness, buffer.normalWorld, -s.eyeVec, mainLight, atten,0);
c.rgb += buffer.emission;
c.a = 1;

UNITY_EXTRACT_FOG_FROM_EYE_VEC(i);
UNITY_APPLY_FOG(_unity_fogCoord, c.rgb);

half4 final = OutputForward(c, buffer.alpha);

if (buffer.writeOIT ==1)
    AddOITValue( (i.screenPos.xy/i.screenPos.w) , final, LinearEyeDepth(i.pos.z));

return final;
}

half4 fragAdd (VertexOutputForwardAdd i) : SV_Target { return fragForwardAddInternal(i); }

#endif
