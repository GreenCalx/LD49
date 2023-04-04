#include "UnityGBuffer.cginc"

struct SchnibbleCustomOutline {
    float4 lightColor;
	float4 shadowColor;
	float  threshold;
	float  width;
	int    enableDistanceBlend;
	float  fadeStart;
	float4 distanceColor;
};

	struct SchnibbleCrossHatchData
	{
        float maxSize;
        float minSize;
		float spaceSize;
        float lightPower;
        float blendPower;		
	};

#define SchTextureW 255
#define SchTextureH 255

struct SchnibbleCustomGPUData {
    SchnibbleCustomOutline outlineDepth;
	int enableOutlineDepth;
	SchnibbleCustomOutline outlineNormal;
	int enableOutlineNormal;
	SchnibbleCustomOutline outlineColor;
	int enableOutlineColor;
	SchnibbleCrossHatchData hatch;
	int enableCrossHatch;
};

#define SchTextureR8Count 3
#define SchTextureDiffuseRampOffset 0
#define SchTextureDiffuseBRDFOffset 1
#define SchTextureOutlineRampOffset 2

#define SchTextureRGBCount 1
#define SchTextureToonRampOffset 0

UNITY_DECLARE_TEX2DARRAY(_SchnibbleMaterialData_TexturesRGB);
UNITY_DECLARE_TEX2DARRAY(_SchnibbleMaterialData_TexturesR8);
StructuredBuffer<SchnibbleCustomGPUData> _SchnibbleMaterialData;

sampler2D _CameraGBufferTexture0;
sampler2D _CameraGBufferTexture1;
sampler2D _CameraGBufferTexture2;
sampler2D _CameraGBufferTexture3;

            struct GBuffer {
                float4 gBuffer0 : SV_Target0;
                float4 gBuffer1 : SV_Target1;
                float4 gBuffer2 : SV_Target2;
                float4 gBuffer3 : SV_Target3;
            };

struct SchnibbleGBuffer {
    half3  albedo;
	half3  specular;
	half   metallic;
	half   roughness;
	int    matId;
	float3 normalWorld;
	half   occlusion;
};

void PackSchnibbleGBuffer(SchnibbleGBuffer input, out GBuffer output)
{
   output.gBuffer0.rgb = input.albedo;
   output.gBuffer0.a   = input.occlusion;
   output.gBuffer1.r   = input.metallic;
   output.gBuffer1.g   = input.roughness;
   output.gBuffer1.b = 0;
   output.gBuffer1.a = (float)input.matId / (float)255;
   output.gBuffer2.rgba = half4(input.normalWorld * 0.5 + 0.5, 0);
   output.gBuffer3.rgba = half4(0,0,0,0);
};

void UnpackSchnibbleGBuffer(GBuffer input, out SchnibbleGBuffer output){
	output.metallic = input.gBuffer1.r;
	output.roughness = input.gBuffer1.g;
	
	output.specular	= output.metallic * input.gBuffer0.rgb;
	output.albedo   = (1-output.metallic) * input.gBuffer0.rgb;
	
	output.matId = input.gBuffer1.a * 255;
	output.normalWorld = normalize((float3)input.gBuffer2.rgb * 2 - 1);
	output.occlusion = input.gBuffer0.a;
};

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

int GetMaterialId(float2 uv) {
    return tex2D(_CameraGBufferTexture1, uv).a * 255;
}

float SampleTextureR8(float2 uv, int materialID, int textureOffset) {
    return UNITY_SAMPLE_TEX2DARRAY(_SchnibbleMaterialData_TexturesR8, float3(uv.xy, (materialID * SchTextureR8Count) + textureOffset));                               	
}

float3 SampleTextureRGB(float2 uv, int materialID, int textureOffset) {
    return UNITY_SAMPLE_TEX2DARRAY(_SchnibbleMaterialData_TexturesRGB, float3(uv.xy, (materialID * SchTextureRGBCount) + textureOffset));
}