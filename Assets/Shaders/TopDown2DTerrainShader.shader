Shader "ProceduralWorlds/TopDown2DTerrainShader" {
	Properties {
		_Color ("Color modifier", Color) = (1,1,1,1)
		_MainTex ("Surface (RGB)", 2D) = "white" {}
		_HeightMap ("Heightmap", 2D) = "black" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		Pass {
			CGPROGRAM
			// Physically based Standard lighting model, and enable shadows on all light types
			#pragma vertex vert
			#pragma fragment frag
	
			// Use shader model 3.0 target, to get nicer looking lighting
			#pragma target 3.0
	
			#include "UnityCG.cginc"
			
	
			sampler2D	_MainTex;
			sampler2D	_HeightMap;
			half4		_Color;
			float4		_MainTex_ST;
	
			struct VertInput {
				float4	pos : POSITION;
				float2	uv : TEXCOORD0;
			};
	
			struct VertOutput {
				float4	pos : SV_POSITION;
				float2	uv : TEXCOORD0;
			};
	
			VertOutput vert(VertInput input) {
				VertOutput	o;
	
				float height = tex2Dlod(_HeightMap, float4(input.uv, 0, 0)).x;
				o.pos = mul(UNITY_MATRIX_MVP, input.pos) + float4(0, height, 0, 0);
				o.uv = TRANSFORM_TEX(input.uv, _MainTex);
				return o;
			}
	
			half4 frag(VertOutput o) : COLOR {
				return tex2D(_MainTex, o.uv) * _Color;
			}
			ENDCG
		}
	}
	FallBack "Diffuse"
}
