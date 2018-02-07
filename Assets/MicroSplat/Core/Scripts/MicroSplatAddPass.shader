Shader "Hidden/MicroSpat/AddPass" 
{
   Properties {
      [HideInInspector] _Control0 ("Control (RGBA)", 2D) = "red" {}
      [HideInInspector] _Control1 ("Control1 (RGBA)", 2D) = "black" {}
      [HideInInspector] _Control2 ("Control2 (RGBA)", 2D) = "black" {}
      [HideInInspector] _Control3 ("Control3 (RGBA)", 2D) = "black" {}
      // Splats
      [NoScaleOffset]_Diffuse ("Diffuse Array", 2DArray) = "white" {}
      [NoScaleOffset]_NormalSAO ("Normal Array", 2DArray) = "bump" {}
      [NoScaleOffset]_PerTexProps("Per Texture Properties", 2D) = "black" {}
      _DummyTex("Dummy", 2D) = "white" {}
      _Contrast("Blend Contrast", Range(0.01, 0.99)) = 0.4
      _UVScale("UV Scales", Vector) = (45, 45, 0, 0)

      // used in fallback on old cards & base map
      [HideInInspector] _MainTex ("BaseMap (RGB)", 2D) = "white" {}
      [HideInInspector] _Color ("Main Color", Color) = (1,1,1,1)
   }

   CGINCLUDE

   half _Contrast;
      UNITY_DECLARE_TEX2D(_Control0);
      UNITY_DECLARE_TEX2D_NOSAMPLER(_Control1);
      UNITY_DECLARE_TEX2D_NOSAMPLER(_Control2);
      UNITY_DECLARE_TEX2D_NOSAMPLER(_Control3);
      sampler2D _PerTexProps;

      float4 _UVScale; // scale and offset
      sampler2D _MainTex;

      UNITY_DECLARE_TEX2DARRAY(_Diffuse);
      UNITY_DECLARE_TEX2DARRAY(_NormalSAO);

      struct Input
      {
         float foo;
      };

      #pragma surface surf Lambert noforwardadd noshadow nometa nofog nolightmap novertexlights noambient noshadow
      #define TERRAIN_SPLAT_ADDPASS

      void surf(Input IN, inout SurfaceOutput o)
      {
         o.Albedo = 0;

      }
   ENDCG

   Category {
      Tags {
         "Queue" = "Geometry-100"
         "IgnoreProjector"="True"
         "RenderType" = "Opaque"

      }

      SubShader {
         ColorMask 0
         ZWrite Off
         CGPROGRAM
            #pragma target 3.5
         ENDCG
      }

   }

   //Fallback off
}
