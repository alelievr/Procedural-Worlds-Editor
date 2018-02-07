using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Text;
using JBooth.MicroSplat;
using System.Collections.Generic;
using System.Linq;

namespace JBooth.MicroSplat
{
   public class UnityLDRenderLoopAdapter : IRenderLoopAdapter 
   {
      const string declareTerrain        = "      #pragma surface surf Standard vertex:vert fullforwardshadows addshadow";
      const string declareTerrainDebug   = "      #pragma surface surf Unlit vertex:vert nofog";
      const string declareTerrainTess    = "      #pragma surface surf Standard vertex:disp tessellate:TessDistance fullforwardshadows addshadow";
      const string declareBlend        = "      #pragma surface blendSurf TerrainBlendable fullforwardshadows addshadow decal:blend";

      static TextAsset vertexFunc;
      static TextAsset fragmentFunc;
      static TextAsset terrainBlendBody;

      public string GetDisplayName() 
      { 
         return "Unity LD"; 
      }

      public string GetRenderLoopKeyword() 
      {
         return "_MSRENDERLOOP_UNITYLD";
      }

      public int GetNumPasses() { return 4; }

      public void WriteShaderHeader(string[] features, StringBuilder sb, MicroSplatShaderGUI.MicroSplatCompiler compiler, bool blend)
      {
         sb.AppendLine("   SubShader {");

         sb.AppendLine("      Tags{\"RenderType\" = \"Opaque\" \"RenderPipeline\" = \"LightweightPipeline\"}");
         sb.AppendLine("      Cull Back");
         sb.AppendLine("      ZTest LEqual");
         if (blend)
         {
            sb.AppendLine("      BLEND ONE ONE");
         }

      }

      public void WritePassHeader(string[] features, StringBuilder sb, MicroSplatShaderGUI.MicroSplatCompiler compiler, int pass, bool blend)
      {
         sb.AppendLine("      Pass");
         sb.AppendLine("      {");
            
         sb.AppendLine("      HLSLPROGRAM");

         sb.AppendLine("      #pragma target " + compiler.GetShaderModel(features));

         sb.AppendLine("      #pragma multi_compile _ _MAIN_LIGHT_COOKIE");
         sb.AppendLine("      #pragma multi_compile _MAIN_DIRECTIONAL_LIGHT _MAIN_SPOT_LIGHT");
         sb.AppendLine("      #pragma multi_compile _ _ADDITIONAL_LIGHTS");
         sb.AppendLine("      #pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE");
         sb.AppendLine("      #pragma multi_compile _ UNITY_SINGLE_PASS_STEREO STEREO_INSTANCING_ON STEREO_MULTIVIEW_ON");
         sb.AppendLine("      #pragma multi_compile _ LIGHTMAP_ON");
         sb.AppendLine("      #pragma multi_compile _ DIRLIGHTMAP_COMBINED");
         sb.AppendLine("      #pragma multi_compile _ _HARD_SHADOWS _SOFT_SHADOWS _HARD_SHADOWS_CASCADES _SOFT_SHADOWS_CASCADES");
         sb.AppendLine("      #pragma multi_compile _ _VERTEX_LIGHTS");
         sb.AppendLine("      #pragma multi_compile_fog");


         sb.AppendLine("      #pragma vertex vert");
         sb.AppendLine("      #pragma fragment frag");



         sb.AppendLine("      ENDHLSL");
      }


      public void WriteVertexFunction(string[] features, StringBuilder sb, MicroSplatShaderGUI.MicroSplatCompiler compiler, int pass, bool blend)
      {
         sb.AppendLine(vertexFunc.text);
      }

      public void WriteFragmentFunction(string[] features, StringBuilder sb, MicroSplatShaderGUI.MicroSplatCompiler compiler, int pass, bool blend)
      {
         sb.AppendLine(fragmentFunc.text);
         if (blend && terrainBlendBody != null)
         {
            sb.AppendLine(terrainBlendBody.text);
         }
      }


      public void WriteShaderFooter(string[] features, StringBuilder sb, MicroSplatShaderGUI.MicroSplatCompiler compiler, bool blend, string baseName)
      {
         if (blend)
         {
            sb.AppendLine("   CustomEditor \"MicroSplatBlendableMaterialEditor\"");
         }
         else if (baseName != null)
         {
            sb.AppendLine("   Dependency \"AddPassShader\" = \"Hidden/MicroSplat/AddPass\"");
            sb.AppendLine("   Dependency \"BaseMapShader\" = \"" + baseName + "\"");
            sb.AppendLine("   CustomEditor \"MicroSplatShaderGUI\"");
         }
         sb.AppendLine("   Fallback \"Nature/Terrain/Diffuse\"");
         sb.Append("}");
      }

      public void Init(string[] paths)
      {
         for (int i = 0; i < paths.Length; ++i)
         {
            string p = paths[i];
            if (p.EndsWith("microsplat_terrain_unityld_vertex.txt"))
            {
               vertexFunc = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("microsplat_terrain_unityld_fragment.txt"))
            {
               fragmentFunc = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("microsplat_terrainblend_body.txt"))
            {
               terrainBlendBody = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
         }
      }

      public void PostProcessShader(string[] features, StringBuilder sb, MicroSplatShaderGUI.MicroSplatCompiler compiler, bool blend)
      {
      }
   }
}
