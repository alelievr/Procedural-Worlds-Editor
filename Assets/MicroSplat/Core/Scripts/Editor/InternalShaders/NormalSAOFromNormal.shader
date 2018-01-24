
Shader "Hidden/MicroSplat/NormalSAOFromNormal" 
{
   // generate a full NormalSAO texture from just a diffuse image
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

      fixed4 frag(v2f_img i) : SV_Target
      {
         half4 norm = tex2D(_MainTex, i.uv);


         half3 un = UnpackNormal(norm);
         un.xy = un.xy * 0.5 + 0.5;
         half4 nsao = 0;
         nsao.xy = un.xy;
         // ao is just the normal maps length
         nsao.w = un.b * un.b;
         // smoothness, be conservative
         nsao.z = un.b * 0.07;

         return nsao.brag;
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