// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

#ifndef SCHNIBBLE_CUSTOM_GBUFFER_PASS_INCLUDED
#define SCHNIBBLE_CUSTOM_GBUFFER_PASS_INCLUDED

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
#pragma vertex schVert
#pragma fragment schFrag

#include "UnityInstancing.cginc"
#include "UnityStandardInput.cginc"
#include "UnityStandardCore.cginc"
#include "SchnibbleCustomGBuffer.cginc"
#include "SchnibbleCustomGBufferInputs.cginc"

#include "../SchnibbleTools/SchnibbleFunctions.cginc"

sampler2D_float _GBufferDepth;
float _Decal;

VertexOutput schVert (SchVertexInput input) {
	VertexOutput o;
	UNITY_INITIALIZE_OUTPUT(VertexOutput, o);
	UNITY_SETUP_INSTANCE_ID(input);
	UNITY_TRANSFER_INSTANCE_ID(input, o);
	
	float4 posWorld = mul(unity_ObjectToWorld, input.vertex);
	#if UNITY_REQUIRE_FRAG_WORLDPOS
		#if UNITY_PACK_WORLDPOS_WITH_TANGENT
			o.tangentToWorldAndPackedData[0].w = posWorld.x;
			o.tangentToWorldAndPackedData[1].w = posWorld.y;
			o.tangentToWorldAndPackedData[2].w = posWorld.z;
		#else
			o.posWorld = posWorld.xyz;
		#endif
	#endif
	
	SCHNIBBLE_CUSTOM_VERT(input);

    o.color = input.color;
	o.pos = UnityObjectToClipPos(input.vertex);
	o.tex = SchTexCoords(input);
	o.normalWorld = UnityObjectToWorldNormal(input.normal);
	o.screenPos = ComputeScreenPos(o.pos);
	#ifdef _TANGENT_TO_WORLD
		float4 tangentWorld = float4(UnityObjectToWorldDir(input.tangent.xyz), input.tangent.w);

		float3x3 tangentToWorld = CreateTangentToWorldPerVertex(o.normalWorld, tangentWorld.xyz, tangentWorld.w);
		o.tangentToWorldAndPackedData[0].xyz = tangentToWorld[0];
		o.tangentToWorldAndPackedData[1].xyz = tangentToWorld[1];
		o.tangentToWorldAndPackedData[2].xyz = tangentToWorld[2];
	#else
		o.tangentToWorldAndPackedData[0].xyz = 0;
		o.tangentToWorldAndPackedData[1].xyz = 0;
		o.tangentToWorldAndPackedData[2].xyz = o.normalWorld;
	#endif

	#ifdef _PARALLAXMAP
		//TANGENT_SPACE_ROTATION;
		float3 binormal = cross( normalize(input.normal), normalize(input.tangent.xyz) ) * input.tangent.w;
		float3x3 rotation = float3x3( input.tangent.xyz, binormal, input.normal );
		
		half3 viewDirForParallax = mul (rotation, ObjSpaceViewDir(input.vertex));
		o.tangentToWorldAndPackedData[0].w = viewDirForParallax.x;
		o.tangentToWorldAndPackedData[1].w = viewDirForParallax.y;
		o.tangentToWorldAndPackedData[2].w = viewDirForParallax.z;
	#endif

	return o;
}

half2 MetallicRoughCustom(float2 uv)
{
    half2 mg;
    // mg.r = tex2D(_MetallicGlossMap, uv).r;
    mg.r = _Metallic;
    // mg.g = tex2D(_SpecGlossMap, uv).r;
    mg.g = _Roughness;
    return mg;
}

sampler2D_float _CameraDepthTexture;

GBuffer schFrag (VertexOutput input){
	UNITY_SETUP_INSTANCE_ID(input);
	input.tex = Parallax(input.tex, IN_VIEWDIR4PARALLAX(input));


	GBuffer output;

    if(_Decal) {

        float2 uv = input.screenPos.xy/input.screenPos.w;
        float3 wpos = DepthToWorld(uv, tex2D(_CameraDepthTexture, uv));
        float3 opos = mul (unity_WorldToObject, float4(wpos,1)).xyz;

				clip (float3(0.5,0.5,0.5) - abs(opos.xyz));

				input.tex.xy = opos.xz+0.5;

				float4 normal = tex2D(_GBufferNormals, uv);
				fixed3 wnormal = normal.rgb * 2 - 1;
				clip (dot(wnormal, mul ((float3x3)unity_ObjectToWorld, float3(0,1,0))) - 0.3);

				fixed4 col = tex2D (_MainTex, input.tex);
                col.rgb = opos;

	if (_DepthMask != 0) {
		// write depth mask
                output.gBuffer0.xyz = col.xyz;
                output.gBuffer1 = float4(0,0,0,0);
                //output.gBuffer2 = float4(input.normalWorld*0.5+0.5, 1);
                output.gBuffer3	= float4(0,0,0,0);
		output.gBuffer1.b = _DepthMask;
	} else {
                output.gBuffer0.xyz = col.xyz;
                output.gBuffer1 = float4(0,0,0,0);
                output.gBuffer2.xyz = normal;
                output.gBuffer3	= float4(0,0,0,0);
        }
    } else {

	if (_DepthMask != 0) {
		// write depth mask
		output.gBuffer0 = float4(0,0,0,0);
		output.gBuffer1 = float4(0,0,0,0);
		output.gBuffer2 = float4(0,0,0,0);
		output.gBuffer3	= float4(0,0,0,0);
		output.gBuffer1.b = _DepthMask;
	}
	else {
		float depthMask = tex2D(_DepthMaskTexture, input.screenPos.xy/input.screenPos.w).b;
		if (_DepthMaskRead != 0) {
			// read depth mask, only render inside depth mask value
			if(depthMask < 0.9)
				discard;
		}
		
		SchnibbleGBuffer gbuffer;
		UNITY_INITIALIZE_OUTPUT(SchnibbleGBuffer, gbuffer);		
		
		float2 metRough = MetallicRoughCustom(input.tex);
		gbuffer.albedo = input.color*_Color*tex2D (_MainTex, input.tex);
		gbuffer.metallic = metRough.r;
		gbuffer.roughness = metRough.g;
		gbuffer.normalWorld = PerPixelWorldNormal(input.tex, input.tangentToWorldAndPackedData); //input.normalWorld;
		gbuffer.occlusion = Occlusion(input.tex);
		gbuffer.matId = _MaterialID;
		
		SCHNIBBLE_CUSTOM_FRAG(input, gbuffer);
						
		PackSchnibbleGBuffer(gbuffer, output);
		output.gBuffer3 = float4(_EmissionColor.rgb, RGBToHSV(_EmissionColor).b);
	}

}
	
	
	return output;
}

#endif
