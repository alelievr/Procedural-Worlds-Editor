Shader "ProceduralWorlds/Basic terrain" {
	Properties {
		// _MainTex ("Surface (RGB)", 2D) = "white" {}
		// _HeightMap ("Heightmap", 2D) = "black" {}
		_AlbedoMaps ("Albedo maps", 2DArray) = "" {}
		_BlendMaps ("Blend maps", 2DArray) = "blackTexture" {}
		[HideInInspector] _BlendMapsCount ("Blend maps count", Int) = 1
		_ShowBlendMap ("Show blend map", Range(0, 1)) = 0
		_Displacement ("Displacement", Range(0, 5)) = 0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		
		CGPROGRAM

		#pragma surface surf StandardSpecular vertex:vert

		// Use shader model 3.5 for TextureArray feature
		#pragma target 3.5

		#include "UnityCG.cginc"
		
		float		_ShowBlendMap;
		float		_Displacement;
		int			_BlendMapsCount;

		//unity appdata_full:
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
		
		struct Input
		{
			//store biome blend infos:
			//	x -> first biome id (for TextureArrays) and y -> first biome percent
			//	z -> second biome id and y -> second biome percent
			float4	biomeBlendInfos;	//UV chan 1 in Vector4
			float4	data : COLOR;		//vertex color, encode 4 variables from *Terrain nodes
			// float4	otherDatas;		//Tangent chan, unused

			//we keep the channel 0 of Uvs for textures:
			float2	uv_AlbedoMaps;
		};

		void vert(inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.biomeBlendInfos = v.texcoord1;
			// o.otherDatas = v.tangent; //unused

			//data.x is certainly the height of the terrain
			v.vertex.xyz += v.normal * o.data.x * _Displacement;
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

		void surf(in Input v, inout SurfaceOutputStandardSpecular o)
		{
			int		biomeId1 = v.biomeBlendInfos.x;
			float	blend1 = v.biomeBlendInfos.y;
			int		biomeId2 = v.biomeBlendInfos.z;
			float	blend2 = v.biomeBlendInfos.w;

			if (_ShowBlendMap > .5f)
			{
				half3	col = half3(1, 1, 0) * blend1 + half3(0, 1, 1) * blend2;

				o.Albedo = col;
				o.Emission = col;
				return ;
			}

			half4	b1 = UNITY_SAMPLE_TEX2DARRAY(_AlbedoMaps, float3(v.uv_AlbedoMaps.xy, biomeId1));
			half4	b2 = UNITY_SAMPLE_TEX2DARRAY(_AlbedoMaps, float3(v.uv_AlbedoMaps.xy, biomeId2));

			o.Albedo = blend(b1, blend1, b2).xyz;
			o.Emission = o.Albedo;
		}

		ENDCG
	}
	FallBack "Diffuse"
}
