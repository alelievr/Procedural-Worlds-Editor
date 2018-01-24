using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Text;
using JBooth.MicroSplat;
using System.Collections.Generic;
using System.Linq;

namespace JBooth.MicroSplat
{
   public class SurfaceShaderRenderLoopAdapter : IRenderLoopAdapter 
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
         return "Surface Shader"; 
      }

      public string GetRenderLoopKeyword() 
      {
         return "_MSRENDERLOOP_SURFACESHADER";
      }

      public int GetNumPasses() { return 1; }

      public void WriteShaderHeader(string[] features, StringBuilder sb, MicroSplatShaderGUI.MicroSplatCompiler compiler, bool blend)
      {
         sb.AppendLine();
         sb.AppendLine("   CGINCLUDE");

         if (features.Contains<string>("_BDRF1") || features.Contains<string>("_BDRF2") || features.Contains<string>("_BDRF3"))
         {
            if (features.Contains<string>("_BDRF1"))
            {
               sb.AppendLine("      #define UNITY_BRDF_PBS BRDF1_Unity_PBS");
            }
            else if (features.Contains<string>("_BDRF2"))
            {
               sb.AppendLine("      #define UNITY_BRDF_PBS BRDF2_Unity_PBS");
            }
            else if (features.Contains<string>("_BDRF3"))
            {
               sb.AppendLine("      #define UNITY_BRDF_PBS BRDF3_Unity_PBS");
            }
         }
         sb.AppendLine("   ENDCG");
         sb.AppendLine();

         sb.AppendLine("   SubShader {");

         sb.AppendLine("      Tags{ \"RenderType\" = \"Opaque\"  \"Queue\" = \"Geometry+100\" }");
         sb.AppendLine("      Cull Back");
         sb.AppendLine("      ZTest LEqual");
         if (blend)
         {
            sb.AppendLine("      BLEND ONE ONE");
         }
         sb.AppendLine("      CGPROGRAM");
      }

      public void WritePassHeader(string[] features, StringBuilder sb, MicroSplatShaderGUI.MicroSplatCompiler compiler, int pass, bool blend)
      {

         sb.AppendLine("      #pragma exclude_renderers d3d9");


         if (blend)
         {  
            sb.Append(declareBlend);
         }
         else if (!features.Contains<string>("_TESSDISTANCE"))
         {
            if (MicroSplatShaderGUI.MicroSplatCompiler.HasDebugFeature(features))
            {
               sb.Append(declareTerrainDebug);
               if (features.Contains("_ALPHAHOLE") || features.Contains("_ALPHABELOWHEIGHT"))
               {
                  // generate a shadow pass so we clip that too..
                  sb.Append(" addshadow");
               }
            }
            else
            {
               sb.Append(declareTerrain);
            }
         }
         else
         {
            sb.Append(declareTerrainTess);
         }

         if (!blend)
         {
            if (features.Contains<string>("_BDRF1") || features.Contains<string>("_BDRF2") || features.Contains<string>("_BDRF3"))
            {
               sb.Append(" exclude_path:deferred");
            }
         }

         // don't remove
         sb.AppendLine();
         sb.AppendLine();

         sb.AppendLine("      #pragma target " + compiler.GetShaderModel(features));

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
            if (p.EndsWith("microsplat_terrain_surface_vertex.txt"))
            {
               vertexFunc = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("microsplat_terrain_surface_fragment.txt"))
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
