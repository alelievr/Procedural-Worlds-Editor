Shader "Custom/TerrainTest" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0

		[Space]
		_WaterHeight ("Wheight", float) = .1
		_WaterColor ("Wcolor", Color) = (0, 0, 1, 1)
		_C1Height ("C1height", float) = .1
		_C1Color ("C1color", Color) = (0, 0, 1, 1)
		_C2Height ("C2height", float) = .1
		_C2Color ("C2color", Color) = (0, 0, 1, 1)
		_C3Height ("C3height", float) = .1
		_C3Color ("C3color", Color) = (0, 0, 1, 1)
		_C4Height ("C4height", float) = .1
		_C4Color ("C4color", Color) = (0, 0, 1, 1)
		_C5Height ("C5height", float) = .1
		_C5Color ("C5color", Color) = (0, 0, 1, 1)
		_C6Height ("C6height", float) = .1
		_C6Color ("C6color", Color) = (0, 0, 1, 1)
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
			float3 worldPos;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		float _WaterHeight;
		float4 _WaterColor;
		float _C1Height;
		float4 _C1Color;
		float _C2Height;
		float4 _C2Color;
		float _C3Height;
		float4 _C3Color;
		float _C4Height;
		float4 _C4Color;
		float _C5Height;
		float4 _C5Color;
		float _C6Height;
		float4 _C6Color;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = float4(1, 0, 1, 1);

			if (IN.worldPos.y < _WaterHeight)
				c = _WaterColor;
			else if (IN.worldPos.y < _C1Height)
				c = _C1Color;
			else if (IN.worldPos.y < _C2Height)
				c = _C2Color;
			else if (IN.worldPos.y < _C3Height)
				c = _C3Color;
			else if (IN.worldPos.y < _C4Height)
				c = _C4Color;
			else if (IN.worldPos.y < _C5Height)
				c = _C5Color;
			else if (IN.worldPos.y < _C6Height)
				c = _C6Color;

			o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
