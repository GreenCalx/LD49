Shader "Custom/Schnibble-Gbuffer"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("Albedo", 2D) = "white" {}

        [Gamma] _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
        _MetallicGlossMap("Metallic", 2D) = "white" {}

        _Roughness("Roughness", Range(0.0, 1.0)) = 0.5

        _BumpScale("Scale", Float) = 1.0
        [Normal] _BumpMap("Normal Map", 2D) = "bump" {}

        _OcclusionStrength("Strength", Range(0.0, 1.0)) = 1.0
        _OcclusionMap("Occlusion", 2D) = "white" {}

        _EmissionColor("Color", Color) = (0,0,0)
        _EmissionMap("Emission", 2D) = "white" {}

        _MaterialID("MaterialID", Int) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            Tags {
                "LightMode" = "Deferred"
            }

            CGPROGRAM

            #pragma target 3.0
            #pragma exclude_renderers nomrt

            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature_fragment _EMISSION
            #pragma shader_feature_local _METALLICGLOSSMAP
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature_local_fragment _DETAIL_MULX2
            #pragma shader_feature_local _PARALLAXMAP

            #pragma multi_compile_prepassfinal
            #pragma multi_compile_instancing
            #pragma vertex vert
            #pragma fragment frag

            #define DEFERRED_PASS

half4       _Color;

sampler2D   _MainTex;
float4      _MainTex_ST;

half        _Metallic;
float       _Roughness;

sampler2D   _BumpMap;
half        _BumpScale;

sampler2D   _OcclusionMap;
half        _OcclusionStrength;

half4       _EmissionColor;
sampler2D   _EmissionMap;

int _MaterialID;

            struct GBuffer {
                float4 gBuffer0 : SV_Target0;
                float4 gBuffer1 : SV_Target1;
                float4 gBuffer2 : SV_Target2;
                float4 gBuffer3 : SV_Target3;
            };

struct VertexInput
{
    float4 vertex   : POSITION;
    half3 normal    : NORMAL;
    float2 uv0      : TEXCOORD0;
    float2 uv1      : TEXCOORD1;
};

            struct VertexOutput
            {
                UNITY_POSITION(pos);
                float4 tex                            : TEXCOORD0;
                half3 normalWorld : NORMAL;
            };

            #define TRANSFORM_TEX(tex,name) (tex.xy * name##_ST.xy + name##_ST.zw)
            float4 TexCoords(VertexInput v)
            {
                float4 texcoord;
                texcoord.xy = TRANSFORM_TEX(v.uv0, _MainTex); // Always source from uv0
                return texcoord;
            }

// Transforms direction from world to object space
inline float3 UnityWorldToObjectDir( in float3 dir )
{
    return normalize(mul((float3x3)unity_WorldToObject, dir));
}
// Transforms normal from object to world space
inline float3 UnityObjectToWorldNormal( in float3 norm )
{
#ifdef UNITY_ASSUME_UNIFORM_SCALING
    return UnityObjectToWorldDir(norm);
#else
    // mul(IT_M, norm) => mul(norm, I_M) => {dot(norm, I_M.col0), dot(norm, I_M.col1), dot(norm, I_M.col2)}
    return normalize(mul(norm, (float3x3)unity_WorldToObject));
#endif
}
            VertexOutput vert (VertexInput input) {
                VertexOutput o;
                o.pos = UnityObjectToClipPos(input.vertex);
                o.tex = TexCoords(input);
                o.normalWorld = UnityObjectToWorldNormal(input.normal);
                return o;
            }

half LerpOneTo(half b, half t)
{
    half oneMinusT = 1 - t;
    return oneMinusT + b * t;
}
half Occlusion(float2 uv)
{
    half occ = tex2D(_OcclusionMap, uv).g;
    return LerpOneTo (occ, _OcclusionStrength);
}

half2 MetallicRough(float2 uv)
{
    half2 mg;
    // mg.r = tex2D(_MetallicGlossMap, uv).r;
    mg.r = _Metallic;
    // mg.g = tex2D(_SpecGlossMap, uv).r;
    mg.g = _Roughness;
    return mg;
}
            GBuffer frag (VertexOutput input){
                GBuffer output;
                output.gBuffer0.rgb = tex2D (_MainTex, input.tex);
                output.gBuffer0.a = Occlusion(input.tex);
                output.gBuffer1.rgb = MetallicRough(input.tex).g;
                output.gBuffer1.a = _MaterialID;
                output.gBuffer2.rgb = half4(input.normalWorld * 0.5f + 0.5f, 1.0f);
                output.gBuffer2.a = MetallicRough(input.tex).r;
                output.gBuffer3 = float4(0,0,0,1);
                // output.gBuffer3.rgb = diffuseLightColorAndIntensity;
                // output.gBuffer3.a = diffuseIntensity;
                return output;
            }

            /*
            struct VertexOutputDeferred
            {
                UNITY_POSITION(pos);
                float4 tex                            : TEXCOORD0;
                float3 eyeVec                         : TEXCOORD1;
                float4 tangentToWorldAndPackedData[3] : TEXCOORD2;    // [3x3:tangentToWorld | 1x3:viewDirForParallax or worldPos]
                half4 ambientOrLightmapUV             : TEXCOORD5;    // SH or Lightmap UVs

                #if UNITY_REQUIRE_FRAG_WORLDPOS && !UNITY_PACK_WORLDPOS_WITH_TANGENT
                float3 posWorld                     : TEXCOORD6;
                #endif

                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            VertexOutputDeferred vert (VertexInput v) {
                UNITY_SETUP_INSTANCE_ID(v);
                VertexOutputDeferred o;
                UNITY_INITIALIZE_OUTPUT(VertexOutputDeferred, o);
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

                o.tex = TexCoords(v);
                o.eyeVec = NormalizePerVertexNormal(posWorld.xyz - _WorldSpaceCameraPos);
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

                o.ambientOrLightmapUV = 0;
                #ifdef LIGHTMAP_ON
                o.ambientOrLightmapUV.xy = v.uv1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
                #elif UNITY_SHOULD_SAMPLE_SH
                o.ambientOrLightmapUV.rgb = ShadeSHPerVertex (normalWorld, o.ambientOrLightmapUV.rgb);
                #endif
                #ifdef DYNAMICLIGHTMAP_ON
                o.ambientOrLightmapUV.zw = v.uv2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
                #endif

                #ifdef _PARALLAXMAP
                TANGENT_SPACE_ROTATION;
                half3 viewDirForParallax = mul (rotation, ObjSpaceViewDir(v.vertex));
                o.tangentToWorldAndPackedData[0].w = viewDirForParallax.x;
                o.tangentToWorldAndPackedData[1].w = viewDirForParallax.y;
                o.tangentToWorldAndPackedData[2].w = viewDirForParallax.z;
                #endif

                return o;
            }
            GBuffer frag (){
                GBuffer output;
                #if defined(DEFERRED_PASS)
                output.gBuffer0.rgb = tex2D (_MainTex, IN.uv_MainTex);
                output.gBuffer0.a = GetOcclusion(i);
                output.gBuffer1.rgb = GetSmoothness()
                #else
                output.color = color;
                #endif


                #if (SHADER_TARGET < 30)
                outGBuffer0 = 1;
                outGBuffer1 = 1;
                outGBuffer2 = 0;
                outEmission = 0;
                #if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
                outShadowMask = 1;
                #endif
                return;
                #endif

                UNITY_APPLY_DITHER_CROSSFADE(i.pos.xy);

                FRAGMENT_SETUP(s)
                UNITY_SETUP_INSTANCE_ID(i);

                // no analytic lights in this pass
                UnityLight dummyLight = DummyLight ();
                half atten = 1;

                // only GI
                half occlusion = Occlusion(i.tex.xy);
                #if UNITY_ENABLE_REFLECTION_BUFFERS
                bool sampleReflectionsInDeferred = false;
                #else
                bool sampleReflectionsInDeferred = true;
                #endif

                UnityGI gi = FragmentGI (s, occlusion, i.ambientOrLightmapUV, atten, dummyLight, sampleReflectionsInDeferred);

                half3 emissiveColor = UNITY_BRDF_PBS (s.diffColor, s.specColor, s.oneMinusReflectivity, s.smoothness, s.normalWorld, -s.eyeVec, gi.light, gi.indirect).rgb;

                #ifdef _EMISSION
                emissiveColor += Emission (i.tex.xy);
                #endif

                #ifndef UNITY_HDR_ON
                emissiveColor.rgb = exp2(-emissiveColor.rgb);
                #endif

                UnityStandardData data;
                data.diffuseColor   = s.diffColor;
                data.occlusion      = occlusion;
                data.specularColor  = s.specColor;
                data.smoothness     = s.smoothness;
                data.normalWorld    = s.normalWorld;

                UnityStandardDataToGbuffer(data, outGBuffer0, outGBuffer1, outGBuffer2);

                // Emissive lighting buffer
                outEmission = half4(emissiveColor, 1);

                // Baked direct lighting occlusion if any
                #if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
                outShadowMask = UnityGetRawBakedOcclusions(i.ambientOrLightmapUV.xy, IN_WORLDPOS(i));
                #endif
                return output;
            }
            */

            ENDCG

        }
    }
}
