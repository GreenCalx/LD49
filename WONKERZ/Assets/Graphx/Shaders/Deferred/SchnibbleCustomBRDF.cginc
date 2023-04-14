#ifndef SCHNIBBLE_CUSTOM_BRDF_INCLUDED
#define SCHNIBBLE_CUSTOM_BRDF_INCLUDED

			#include "SchnibbleCustomGBuffer.cginc"
			#include "../SchnibbleTools/SchnibbleFunctions.cginc"

			half4 BRDF_Toon(half3 albedo, half3 specularColor, half roughness, half3 normal,
                            half3 viewDir, half oneMinusReflectivity, UnityLight light, float atten, int id)
			{
			    float perceptualRoughness = roughness;
				roughness = perceptualRoughness*perceptualRoughness;

				half3 halfDir = Unity_SafeNormalize(light.dir + viewDir);

				half nv = abs(dot(normal, viewDir));    // This abs allow to limit artifact
				float nl = saturate(dot(normal, light.dir));
				float nh = saturate(dot(normal, halfDir));

				half lv = saturate(dot(light.dir, viewDir));
				half lh = saturate(dot(light.dir, halfDir));

				float ndotl   = dot(normal, light.dir);
				float ndotl_ramped = SampleTextureR8( float2( saturate((ndotl + 1.0)*0.5), RGBToHSV(albedo).x),
													  id,
													  SchTextureDiffuseBRDFOffset);
				half3 lightColor = light.color;
				float lightIntensity = RGBToHSV(lightColor).b;

				float diffuseIntensity = ndotl_ramped * atten;
				half3 diffuseColor = diffuseIntensity * lightColor;

				// GGX with roughtness to 0 would mean no specular at all, using max(roughness, 0.002) here to match HDrenderloop roughtness remapping.
				roughness = max(roughness, 0.002);
				float V = SmithJointGGXVisibilityTerm (nl, nv, roughness);
				float D = GGXTerm (nh, roughness);

				float specularTerm = V*D * UNITY_PI; // Torrance-Sparrow model, Fresnel is applied later
				// specularTerm * nl can be NaN on Metal in some cases, use max() to make sure it's a sane value
				specularTerm = max(0, specularTerm * nl);
				// To provide true Lambert lighting, we need to be able to kill specular completely.
				specularTerm *= any(specularColor.xyz) ? 1.0 : 0.0;
				specularColor = specularTerm * specularColor * lightColor;

				float fresnel = 1 - dot(viewDir, normal);
				float rimLight = fresnel;
				half3 rimLightColor = atten * smoothstep(0.96 - 0.01, 0.96 + 0.01, rimLight);
				//if(fresnel < 0.) fresnel = 0;
				half3 fresnelColor = fresnel.xxx;

				// surfaceReduction = Int D(NdotH) * NdotH * Id(NdotL>0) dH = 1/(roughness^2+1)
				half surfaceReduction;
				#ifdef UNITY_COLORSPACE_GAMMA
					surfaceReduction = 1.0-0.28*roughness*perceptualRoughness;      // 1-0.28*x^3 as approximation for (1/(x^4+1))^(1/2.2) on the domain [0;1]
				#else
					surfaceReduction = 1.0 / (roughness*roughness + 1.0);           // fade \in [0.5;1]
				#endif

                // Diffuse term
                half diffuseTerm = DisneyDiffuse(nv, nl, lh, perceptualRoughness) * nl;
				half grazingTerm = saturate((1-perceptualRoughness) + (1-oneMinusReflectivity));
				half3 color2 =   albedo * (light.color * diffuseTerm)
								+ specularTerm * light.color * FresnelTerm (specularColor, lh)
								+ surfaceReduction * FresnelLerp (specularColor, grazingTerm, nv);


				half3 color = diffuseColor + specularColor; //+ rimLightColor;

				color = color2;

				half finalIntensity = RGBToHSV(color).b;
				return half4(color, finalIntensity);
			}

#endif
