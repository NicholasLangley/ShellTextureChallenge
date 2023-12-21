// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/My Shader"
{
	Properties {
		_AudioTex ("Audio Texture", 2D) = "white" {}
		_AudioTex1D ("Audio Texture 1D", 2D) = "white" {}
	}

		SubShader
	{
		Pass
		{
			Tags {
				"LightMode" = "ForwardBase"
			}

			

			Cull Off
			CGPROGRAM

			#pragma vertex MyVertexProgram
			#pragma fragment MyFragmentProgram

			//#include "UnityCG.cginc"
			#include "UnityStandardBRDF.cginc"
			
			int _Density; //Controls how many blades there are
			int _ShellCount; //The total amount of Shells rendered
			int _ShellIndex;  //The position of the shell in the hierarchy, higher indexes are further from base
			float _ShellLength; //Total Length of all shells together
			float _Thickness; //How thick the strands are
			float _DisplacementStrength; //How much the shells are affected  by gravity/movement
			float3 _DisplacementDirection; // the direction the shells are displaced in
			float _Curvature; // controls how 'stiff' hair is, a higher value means curve starts further up
			float4 _ShellColor; // The color of the shells
			float _Attenuation; //How fast the ambientOcclusion takes effect
			float _OcclusionBias;// adds a bit of bias to the ambient occlusion so it doesn't hit pure black at the bottom shell

			//audio textures to use as a heightmap by the vertex shader
			sampler2D _AudioTex; //2D audio visualizer texture split into spectrum blocks
			sampler2D _AudioTex1D; // 1D full spectrum audio visual texture
			int _TextureSelector; // swap between textures to use

			float _AudioLevel; // average normalized audio level for the entire spectrum

			struct VertexData {
				float4 position : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
			};

			struct v2f {
				float4 position : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : TEXCOORD1;
			};

			//hashing function
			float hash(float2 uv, int density) {
				//This expands the UV values and cuts to the floor so that there are specific "blocks with the same newUV area. Each block is a potential blade of grass"
				float2 uvDensity = floor(uv * (float)density);

				//The hash function itself
				//https://www.shadertoy.com/view/4djSRW

				float3 p3  = frac(float3(uvDensity.xyx) * .1031);
				p3 += dot(p3, p3.yzx + 33.33);
				return frac((p3.x + p3.y) * p3.z);
			};

			v2f MyVertexProgram(VertexData v) {
				v2f i;
				float shellHeight = (float)_ShellIndex / (float)_ShellCount;

				//audio visualizer
				float4 audioDisplacement = 0;
				float disBias = 1.0;
				float audioMult = 5.0;
				if (_TextureSelector == 0){
					audioDisplacement = tex2Dlod(_AudioTex, float4(v.uv, 0.0, 0.0)); 
					audioMult = 4;
				}
				else {
					float2 dist = v.uv * 2.0 - 1.0;
					float dis = length(dist);
					dis -= 0.2;
					dis *= 2;
					if (dis < 0) {audioDisplacement = tex2Dlod(_AudioTex1D, float4(0.0, 0.0, 0.0, 0.0)); disBias = 1;}
					else{audioDisplacement = tex2Dlod(_AudioTex1D, float4(dis, 0.0, 0.0, 0.0)); disBias = max(2 - dis, 0.5);}
					if(audioDisplacement.r < 0.05 && dis > 0.8 && dis < 1.2 || audioDisplacement.r < 0.05 && dis > 1.8){audioDisplacement = tex2Dlod(_AudioTex1D, float4(0.0, 0.0, 0.0, 0.0)) * 0.5;}
					audioMult = 1.5f;
				}
				float audioD = 1 + audioDisplacement.r * (0.75 + 2*_AudioLevel) * audioMult * disBias;

				i.position = UnityObjectToClipPos(v.position + (v.normal * shellHeight * _ShellLength * audioD));
				i.position.xyz += _DisplacementDirection * pow(_DisplacementStrength * shellHeight, _Curvature);
				i.uv = v.uv;
				i.normal = UnityObjectToWorldNormal(v.normal);
				return i;
			}

			float4 MyFragmentProgram(v2f i) : SV_TARGET {
				//hash the UV's to get descrete blocks to test for grass'
				float hashedUV = hash(i.uv, _Density);
				float shellHeight = (float)_ShellIndex / (float)_ShellCount;
				
				//calculate how far pixel is from centre of blade
				float2 localSpace = frac(i.uv * _Density) * 2.0 -1.0;
				float distFromCentre = length(localSpace);

				
				//If the hashed value is not high enough, or  the pixel is too far from the centre of the blasde/hair, discard it. Also dont discard the base layer
				if(_ShellIndex > 0 && distFromCentre > _Thickness * (hashedUV - shellHeight)){discard;}

				//lower indexes should be darker to fake shadows

				//lambertian diffuse
				float3 lightDir = _WorldSpaceLightPos0.xyz;
				i.normal = normalize(i.normal);
				float3 lightColor = _LightColor0.rgb;
				float3 albedo = _ShellColor.rgb; 
				//half lambert
				float3 halfLambert = (dot(lightDir, i.normal) * 0.5 + 0.5);
				halfLambert = halfLambert * halfLambert;

				//lower shells are darker to fake shadows - fake ambient occlusion
				float ambientOcclusion = pow(shellHeight, _Attenuation);
				ambientOcclusion += _OcclusionBias;
				ambientOcclusion = saturate(ambientOcclusion);

				//audio visualizer
				float4 audioDisplacement = 0;
				if (_TextureSelector == 0){audioDisplacement = tex2Dlod(_AudioTex, float4(i.uv, 0.0, 0.0));}
				else {
					float2 dist = i.uv * 2.0 - 1.0;
					float dis = length(dist);
					dis -= 0.2;
					dis *= 2;
					if (dis < 0) {audioDisplacement = tex2Dlod(_AudioTex1D, float4(0.0, 0.0, 0.0, 0.0));}
					else{audioDisplacement = tex2Dlod(_AudioTex1D, float4(dis, 0.0, 0.0, 0.0));}
					if(audioDisplacement.r < 0.05 && dis > 0.8 && dis < 1.2 || audioDisplacement.r < 0.05 && dis > 1.8){audioDisplacement = tex2Dlod(_AudioTex1D, float4(0.0, 0.0, 0.0, 0.0)) * 0.5;}
				}
				float audioD = audioDisplacement.r;
				albedo.r += audioD * (0.5 + _AudioLevel);
				albedo.b += audioD * (0.5 + _AudioLevel);

				float3 diffuse = albedo * lightColor * halfLambert * ambientOcclusion;
				return float4(diffuse, 1);
			}



			ENDCG
		}
	}
}
