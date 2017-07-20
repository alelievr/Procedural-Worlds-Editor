// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "ProceduralWorlds/Basic terrain" {
	Properties {
		// _MainTex ("Surface (RGB)", 2D) = "white" {}
		// _HeightMap ("Heightmap", 2D) = "black" {}
		_AlbedoMaps ("Albedo maps", 2DArray) = "" {}
		_BlendMaps ("Blend maps", 2DArray) = "blackTexture" {}
		[HideInInspector] _BlendMapsCount ("Blend maps count", Int) = 1
		_ShowBlendMap ("Show blend map", Range(0, 1)) = 0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		Pass {
			CGPROGRAM

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
			int			_BlendMapsCount;
			float4		_AlbedoMaps_ST;

			//unity appdata_fulll:
			/*
			struct appdata_full {
				float4 vertex : POSITION;
				float4 tangent : TANGENT;
				float3 normal : NORMAL;
				float4 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				float4 texcoord3 : TEXCOORD3;
				fixed4 color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			*/
	
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
	
				o.pos = UnityObjectToClipPos(input.pos);
				o.color = input.color;
				o.uv = input.uv;
				return o;
			}

			half4 blend(half4 texture1, float a1, half4 texture2)
			{
				float a2 = 1 - a1;
				float depth = 0.2;
				float ma = max(texture1.a + a1, texture2.a + a2) - depth;
			
				float b1 = max(texture1.a + a1 - ma, 0);
				float b2 = max(texture2.a + a2 - ma, 0);
			
				return (texture1 * b1 + texture2 * b2) / (b1 + b2);
			}
	
	
			half4 frag(VertOutput o) : COLOR {
				half4 col = half4(0, 0, 0, 0);

				for (int j = 0; j < _BlendMapsCount; j++)
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
						float2 nuv = TRANSFORM_TEX(o.uv, _AlbedoMaps);
                        col = blend(UNITY_SAMPLE_TEX2DARRAY(_AlbedoMaps, float3(nuv, j + 0)), b.r, col);
                        col = blend(UNITY_SAMPLE_TEX2DARRAY(_AlbedoMaps, float3(nuv, j + 1)), b.g, col);
                        col = blend(UNITY_SAMPLE_TEX2DARRAY(_AlbedoMaps, float3(nuv, j + 2)), b.b, col);
                        col = blend(UNITY_SAMPLE_TEX2DARRAY(_AlbedoMaps, float3(nuv, j + 3)), b.a, col);
					}
				}

				return col;
			}
			ENDCG
		}
	}
	FallBack "Diffuse"
}
