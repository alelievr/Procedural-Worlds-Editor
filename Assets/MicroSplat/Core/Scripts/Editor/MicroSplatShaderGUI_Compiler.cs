//////////////////////////////////////////////////////
// MicroSplat
// Copyright (c) Jason Booth, slipster216@gmail.com
//////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Text;
using JBooth.MicroSplat;
using System.Collections.Generic;
using System.Linq;

public partial class MicroSplatShaderGUI : ShaderGUI 
{
   static TextAsset terrainBody;
   static TextAsset sharedInc;

   static List<IRenderLoopAdapter> availableRenderLoops = new List<IRenderLoopAdapter>();


   [MenuItem ("Assets/Create/Shader/MicroSplat Shader")]
   static void NewShader2()
   {
      NewShader();
   }

   [MenuItem ("Assets/Create/MicroSplat/MicroSplat Shader")]
   public static Shader NewShader()
   {
      string path = "Assets";
      foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
      {
         path = AssetDatabase.GetAssetPath(obj);
         if (System.IO.File.Exists(path))
         {
            path = System.IO.Path.GetDirectoryName(path);
         }
         break;
      }
      path = path.Replace("\\", "/");
      path = AssetDatabase.GenerateUniqueAssetPath(path + "/MicroSplat.shader");
      string name = path.Substring(path.LastIndexOf("/"));
      name = name.Substring(0, name.IndexOf("."));
      MicroSplatCompiler compiler = new MicroSplatCompiler();
      compiler.Init();
      string ret = compiler.Compile(new string[0], name, name);
      System.IO.File.WriteAllText(path, ret);
      AssetDatabase.Refresh();
      return AssetDatabase.LoadAssetAtPath<Shader>(path);
   }

   public static Material NewShaderAndMaterial(string path, string name)
   {
      string shaderPath = AssetDatabase.GenerateUniqueAssetPath(path + "/MicroSplat.shader");
      string shaderBasePath = shaderPath.Replace(".shader", "_Base.shader");
      string matPath = AssetDatabase.GenerateUniqueAssetPath(path + "/MicroSplat.mat");

      MicroSplatCompiler compiler = new MicroSplatCompiler();
      compiler.Init();

      string baseName = "Hidden/MicroSplat/" + name + "_Base";

      string baseShader = compiler.Compile(new string[0], baseName);
      string regularShader = compiler.Compile(new string[0], name, baseName);
      System.IO.File.WriteAllText(shaderPath, regularShader);
      System.IO.File.WriteAllText(shaderBasePath, baseShader);
      AssetDatabase.Refresh();
      Shader s = AssetDatabase.LoadAssetAtPath<Shader>(shaderPath);

      Material m = new Material(s);
      AssetDatabase.CreateAsset(m, matPath);
      AssetDatabase.SaveAssets();
      AssetDatabase.Refresh();

      return AssetDatabase.LoadAssetAtPath<Material>(matPath);
   }

   public static Material NewShaderAndMaterial(Terrain t)
   {
      string path = MicroSplatUtilities.RelativePathFromAsset(t.terrainData);
      return NewShaderAndMaterial(path, t.name);
   }

   public class MicroSplatCompiler
   {
      public List<FeatureDescriptor> extensions = new List<FeatureDescriptor>();

      public string GetShaderModel(string[] features)
      {
         string minModel = "3.5";
         for (int i = 0; i < extensions.Count; ++i)
         {
            if (extensions[i].RequiresShaderModel46())
            {
               minModel = "4.6";
            }
         }
         if (features.Contains("_FORCEMODEL46"))
         {
            minModel = "4.6";
         }
         if (features.Contains("_FORCEMODEL50"))
         {
            minModel = "5.0";
         }

         return minModel;
      }

      public void Init()
      {
         if (terrainBody == null || extensions.Count == 0)
         {
            string[] paths = AssetDatabase.FindAssets("microsplat_ t:TextAsset");
            for (int i = 0; i < paths.Length; ++i)
            {
               paths[i] = AssetDatabase.GUIDToAssetPath(paths[i]);
            }

            for (int i = 0; i < paths.Length; ++i)
            {
               var p = paths[i];

               if (p.EndsWith("microsplat_terrain_body.txt"))
               {
                  terrainBody = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
               }
               if (p.EndsWith("microsplat_shared.txt"))
               {
                  sharedInc = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
               }
            }

            // init extensions
            var types = System.Reflection.Assembly.GetExecutingAssembly().GetTypes();
            var possible = (from System.Type type in types
                                 where type.IsSubclassOf(typeof(FeatureDescriptor))
                                 select type).ToArray();

            for (int i = 0; i < possible.Length; ++i)
            {
               var typ = possible[i];
               FeatureDescriptor ext = System.Activator.CreateInstance(typ) as FeatureDescriptor;
               ext.InitCompiler(paths);
               extensions.Add(ext);
            }
            extensions.Sort(delegate(FeatureDescriptor p1, FeatureDescriptor p2)
            {
               if (p1.DisplaySortOrder() != 0 || p2.DisplaySortOrder() != 0)
               {
                  return p1.DisplaySortOrder().CompareTo(p2.DisplaySortOrder());
               }
               return p1.GetType().Name.CompareTo(p2.GetType().Name);
            });


            var adapters = (from System.Type type in types
               where(type.GetInterfaces().Contains(typeof(IRenderLoopAdapter))) 
               select type).ToArray();

            availableRenderLoops.Clear();
            for (int i = 0; i < adapters.Length; ++i)
            {
               var typ = adapters[i];
               IRenderLoopAdapter adapter = System.Activator.CreateInstance(typ) as IRenderLoopAdapter;
               adapter.Init(paths);
               availableRenderLoops.Add(adapter);
            }

         }
      }
         


      void WriteFeatures(string[] features, StringBuilder sb)
      {
         sb.AppendLine();
         for (int i = 0; i < features.Length; ++i)
         {
            sb.AppendLine("      #define " + features[i] + " 1");
         }
         sb.AppendLine();

         sb.AppendLine(sharedInc.text);

         // sort for compile order
         extensions.Sort(delegate(FeatureDescriptor p1, FeatureDescriptor p2)
         {
            if (p1.CompileSortOrder() != p2.CompileSortOrder())
               return (p1.CompileSortOrder() < p2.CompileSortOrder()) ? -1 : 1;
            return p1.GetType().Name.CompareTo(p2.GetType().Name);
         });
            
         for (int i = 0; i < extensions.Count; ++i)
         {
            var ext = extensions[i];
            if (ext.GetVersion() == MicroSplatVersion)
            {
               extensions[i].WriteFunctions(sb);
            }
         }

         // sort by name, then display order..
         extensions.Sort(delegate(FeatureDescriptor p1, FeatureDescriptor p2)
         {
            if (p1.DisplaySortOrder() != 0 || p2.DisplaySortOrder() != 0)
            {
               return p1.DisplaySortOrder().CompareTo(p2.DisplaySortOrder());
            }
            return p1.GetType().Name.CompareTo(p2.GetType().Name);
         });

      }


      void WriteProperties(string[] features, StringBuilder sb, bool blendable)
      {
         sb.AppendLine("   Properties {");

         sb.AppendLine("      [HideInInspector] _Control0 (\"Control0\", 2D) = \"red\" {}");
         bool max4 = features.Contains("_MAX4TEXTURES");
         bool max8 = features.Contains("_MAX8TEXTURES");
         bool max12 = features.Contains("_MAX12TEXTURES");
         if (!max4)
         {
            sb.AppendLine("      [HideInInspector] _Control1 (\"Control1\", 2D) = \"black\" {}");
         }
         if (!max4 && !max8)
         {
            sb.AppendLine("      [HideInInspector] _Control2 (\"Control2\", 2D) = \"black\" {}");
         }
         if (!max4 && !max8 && !max12)
         {
            sb.AppendLine("      [HideInInspector] _Control3 (\"Control3\", 2D) = \"black\" {}");
         }

         for (int i = 0; i < extensions.Count; ++i)
         {
            var ext = extensions[i];
            if (ext.GetVersion() == MicroSplatVersion)
            {
               ext.WriteProperties(features, sb);
            }
            sb.AppendLine("");
         }
         sb.AppendLine("   }");
      }

      public static bool HasDebugFeature(string[] features)
      {
         return features.Contains("_DEBUG_OUTPUT_ALBEDO") ||
            features.Contains("_DEBUG_OUTPUT_NORMAL") ||
            features.Contains("_DEBUG_OUTPUT_HEIGHT") ||
            features.Contains("_DEBUG_OUTPUT_METAL") ||
            features.Contains("_DEBUG_OUTPUT_SMOOTHNESS") ||
            features.Contains("_DEBUG_OUTPUT_AO") ||
            features.Contains("_DEBUG_OUTPUT_EMISSION");

      }

      public IRenderLoopAdapter renderLoop = null;
      static StringBuilder sBuilder = new StringBuilder(256000);
      public string Compile(string[] features, string name, string baseName = null, bool blendable = false)
      {
         Init();

         // get default render loop if it doesn't exist
         if (renderLoop == null)
         {
            for (int i = 0; i < availableRenderLoops.Count; ++i)
            {
               if (availableRenderLoops[i].GetType() == typeof(SurfaceShaderRenderLoopAdapter))
               {
                  renderLoop = availableRenderLoops[i];
               }
            }
         }

         for (int i = 0; i < extensions.Count; ++i)
         {
            var ext = extensions[i];
            ext.Unpack(features);
         }
         sBuilder.Length = 0;
         var sb = sBuilder;
         sb.AppendLine("//////////////////////////////////////////////////////");
         sb.AppendLine("// MicroSplat");
         sb.AppendLine("// Copyright (c) Jason Booth");
         sb.AppendLine("//");
         sb.AppendLine("// Auto-generated shader code, don't hand edit!");
         sb.AppendLine("//   Compiled with MicroSplat " + MicroSplatVersion);
         sb.AppendLine("//   Unity : " + Application.unityVersion);
         sb.AppendLine("//   Platform : " + Application.platform);
         sb.AppendLine("//////////////////////////////////////////////////////");
         sb.AppendLine();

         if (!blendable && baseName == null)
         {
            sb.Append("Shader \"Hidden/MicroSplat/");
         }
         else
         {
            sb.Append("Shader \"MicroSplat/");
         }
         while (name.Contains("/"))
         {
            name = name.Substring(name.IndexOf("/") + 1);
         }
         sb.Append(name);
         if (blendable)
         { 
            sb.Append("_BlendWithTerrain");
         }
         sb.AppendLine("\" {");


         // props
         WriteProperties(features, sb, blendable);
         renderLoop.WriteShaderHeader(features, sb, this, blendable);


         for (int pass = 0; pass < renderLoop.GetNumPasses(); ++pass)
         {
            renderLoop.WritePassHeader(features, sb, this, pass, blendable);

            // don't remove
            sb.AppendLine();
            sb.AppendLine();

            WriteFeatures(features, sb);
            sb.AppendLine(terrainBody.text);

            renderLoop.WriteVertexFunction(features, sb, this, pass, blendable);
            renderLoop.WriteFragmentFunction(features, sb, this, pass, blendable);

            sb.AppendLine("ENDCG\n\n   }");
         }


         renderLoop.WriteShaderFooter(features, sb, this, blendable, baseName);

         for (int i = 0; i < extensions.Count; ++i)
         {
            var ext = extensions[i];
            ext.OnPostGeneration(sb, features, name, baseName, blendable);
         }
         
         sb.AppendLine("");
         string output = sb.ToString();

         // fix newline mixing warnings..
         output = System.Text.RegularExpressions.Regex.Replace(output, "\r\n?|\n", System.Environment.NewLine);
         return output;
      }

      public void Compile(Material m, string shaderName = null)
      {
         int hash = 0;
         for (int i = 0; i < m.shaderKeywords.Length; ++i)
         {
            hash += 31 + m.shaderKeywords[i].GetHashCode();
         }
         var path = AssetDatabase.GetAssetPath(m.shader);
         string nm = m.shader.name;
         if (!string.IsNullOrEmpty(shaderName))
         {
            nm = shaderName;
         }
         string baseName = "Hidden/" + nm + "_Base" + hash.ToString();

         string terrainShader = Compile(m.shaderKeywords, nm, baseName);

         string blendShader = null;

         // strip extra feature from terrain blending to make it cheaper
         if (m.IsKeywordEnabled("_TERRAINBLENDING"))
         {
            List<string> blendKeywords = new List<string>(m.shaderKeywords);
            if (m.IsKeywordEnabled("_TBDISABLE_DETAILNOISE") && blendKeywords.Contains("_DETAILNOISE"))
            {
               blendKeywords.Remove("_DETAILNOISE");
            }
            if (m.IsKeywordEnabled("_TBDISABLE_DISTANCENOISE") && blendKeywords.Contains("_DISTANCENOISE"))
            {
               blendKeywords.Remove("_DISTANCENOISE");
            }
            if (m.IsKeywordEnabled("_TBDISABLE_DISTANCERESAMPLE") && blendKeywords.Contains("_DISTANCERESAMPLE"))
            {
               blendKeywords.Remove("_DISTANCERESAMPLE");
            }

            blendShader = Compile(blendKeywords.ToArray(), nm, null, true);
         }


         // generate fallback
         string[] oldKeywords = new string[m.shaderKeywords.Length];
         System.Array.Copy(m.shaderKeywords, oldKeywords, m.shaderKeywords.Length);
         m.DisableKeyword("_TESSDISTANCE");
         m.DisableKeyword("_TESSEDGE");
         m.DisableKeyword("_PARALLAX");
         m.DisableKeyword("_DETAILNOISE");

         // maybe reduce layers in distance? can cause a pop though..
         //m.DisableKeyword("_MAX3LAYER");
         //m.EnableKeyword("_MAX2LAYER");


         string fallback = Compile(m.shaderKeywords, baseName);
         m.shaderKeywords = oldKeywords;

         System.IO.File.WriteAllText(path, terrainShader);
         string fallbackPath = path.Replace(".shader", "_Base.shader");
         string terrainBlendPath = path.Replace(".shader", "_TerrainObjectBlend.shader");
         System.IO.File.WriteAllText(fallbackPath, fallback);
         if (blendShader != null)
         {
            System.IO.File.WriteAllText(terrainBlendPath, blendShader); 
         }

         EditorUtility.SetDirty(m);
         AssetDatabase.Refresh();
         MicroSplatTerrain.SyncAll(); 
      }
   }
}
