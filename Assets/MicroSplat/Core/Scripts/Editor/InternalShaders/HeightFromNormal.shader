
Shader "Hidden/MicroSplat/HeightFromNormal" 
{
   // generate a height map from a normalSAO map
   Properties {
      _MainTex ("Base (RGB)", 2D) = "bump" {}
   }

   CGINCLUDE
      
      #include "UnityCG.cginc"
   
      struct v2f {
         float4 pos : SV_POSITION;
         float2 uv : TEXCOORD0;
      };
      
      sampler2D _MainTex;

      float4 _MainTex_TexelSize;


      half BlendOverlay(half base, half blend) { return (base < 0.5 ? (2.0 * base * blend) : (1.0 - 2.0 * (1.0 - base) * (1.0 - blend))); }
         
          
      fixed4 frag(v2f_img i) : SV_Target
      {
         half2 n0 = tex2Dbias(_MainTex, float4(i.uv, 0, 0)).xy;
         half n = BlendOverlay(n0.x, n0.y);
         return fixed4(n,n,n,n);
      }

      ENDCG

   SubShader {
      Pass {
         ZTest Always Cull Off ZWrite Off
            
         CGPROGRAM
         #pragma vertex vert_img
         #pragma fragment frag
         #include "UnityCG.cginc"
         ENDCG
      }

   }

   Fallback off

}