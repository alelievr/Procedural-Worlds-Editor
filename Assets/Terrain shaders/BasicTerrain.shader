Shader "ProceduralWorlds/Basic terrain" {
	Properties {
		// _MainTex ("Surface (RGB)", 2D) = "white" {}
		// _HeightMap ("Heightmap", 2D) = "black" {}
		_AlbedoMaps ("Albedo maps", 2DArray) = "" {}
		_BlendMaps ("Blend maps", 2DArray) = "blackTexture" {}
		_ShowBlendMap ("Show blend map", Range(0, 1)) = 0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		Pass {
			CGPROGRAM

			//define the maximum number of texture to be displayed in the shader divided by 4 so 8 will be fore 32 textures.
			#define MAX_BLENDMAPS		1

			// Physically based Standard lighting model, and enable shadows on all light types
			#pragma vertex vert
			#pragma fragment frag
	
			// Use shader model 3.5 to user TextureArray
			#pragma target 3.5
	
			#include "UnityCG.cginc"
			
			// sampler2D	_MainTex;
			// sampler2D	_HeightMap;
			// float4		_MainTex_ST;
			float		_ShowBlendMap;
	
			UNITY_DECLARE_TEX2DARRAY(_AlbedoMaps);
			UNITY_DECLARE_TEX2DARRAY(_BlendMaps);

			struct VertInput {
				float4	pos : POSITION;
				float3	norm : NORMAL;
				float2	uv : TEXCOORD0;
				float4	color : COLOR;
			};
	
			struct VertOutput {
				float4	pos : SV_POSITION;
				float2	uv : TEXCOORD0;
				float4	color : COLOR;
			};
	
			VertOutput vert(VertInput input) {
				VertOutput	o;
	
				o.pos = mul(UNITY_MATRIX_MVP, input.pos);
				o.color = input.color;
				o.uv = input.uv;
				return o;
			}
	
			half4 frag(VertOutput o) : COLOR {
				half4 col = half4(0, 0, 0, 0);

				for (int j = 0; j < MAX_BLENDMAPS; j++)
				{
					float4 b = UNITY_SAMPLE_TEX2DARRAY(_BlendMaps, float3(o.uv, j));

					if (_ShowBlendMap > .5f)
					{
						if (length(b) == 0)
							b = float4(1, 1, 1, 1);
						col += b;
					}
					else
					{
                        col += UNITY_SAMPLE_TEX2DARRAY(_AlbedoMaps, float3(o.uv, j + 0)) * b.r;
                        col += UNITY_SAMPLE_TEX2DARRAY(_AlbedoMaps, float3(o.uv, j + 1)) * b.g;
                        col += UNITY_SAMPLE_TEX2DARRAY(_AlbedoMaps, float3(o.uv, j + 2)) * b.b;
                        col += UNITY_SAMPLE_TEX2DARRAY(_AlbedoMaps, float3(o.uv, j + 3)) * b.a;
					}
				}

				return col / MAX_BLENDMAPS;
			}
			ENDCG
		}
	}
	FallBack "Diffuse"
}
