// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/My Shader"
{
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
				i.position = UnityObjectToClipPos(v.position + v.normal * shellHeight * _ShellLength);
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


				float3 diffuse = albedo * lightColor * halfLambert * ambientOcclusion;
				return float4(diffuse, 1);
			}



			ENDCG
		}
	}
}
