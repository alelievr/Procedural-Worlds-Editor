using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using JBooth.MicroSplat;


public partial class MicroSplatTerrainEditor : Editor 
{
   public enum BakingResolutions
   {
      k256 = 256,
      k512 = 512,
      k1024 = 1024,
      k2048 = 2048, 
      k4096 = 4096, 
      k8192 = 8192
   };

   public enum BakingPasses
   {
      Albedo = 1,
      Height = 2,
      Normal = 4,
      Metallic = 8,
      Smoothness = 16,
      AO = 32,
      Emissive = 64,
   };

   public BakingPasses passes = 0;
   public BakingResolutions res = BakingResolutions.k1024;

   bool needsBake = false;
   public void BakingGUI(MicroSplatTerrain t)
   {
      if (needsBake && Event.current.type == EventType.Repaint)
      {
         needsBake = false;
         Bake(t);
      }
      if (MicroSplatUtilities.DrawRollup("Render Baking", false))
      {
         res = (BakingResolutions)EditorGUILayout.EnumPopup(new GUIContent("Resolution"), res);

         #if UNITY_2017_3_OR_NEWER
            passes = (BakingPasses)EditorGUILayout.EnumFlagsField(new GUIContent("Features"), passes);
         #else
            passes = (BakingPasses)EditorGUILayout.EnumMaskPopup(new GUIContent("Features"), passes);
         #endif

         if (GUILayout.Button("Export Selected"))
         {
            needsBake = true;
         }
      }
   }


   bool IsEnabled(BakingPasses p)
   {
      return ((int)passes & (int)p) == (int)p;
   }
      


   static MicroSplatBaseFeatures.DefineFeature FeatureFromOutput(MicroSplatBaseFeatures.DebugOutput p)
   {
      if (p == MicroSplatBaseFeatures.DebugOutput.Albedo)
      {
         return MicroSplatBaseFeatures.DefineFeature._DEBUG_OUTPUT_ALBEDO;
      }
      else if (p == MicroSplatBaseFeatures.DebugOutput.AO)
      {
         return MicroSplatBaseFeatures.DefineFeature._DEBUG_OUTPUT_AO;
      }
      else if (p == MicroSplatBaseFeatures.DebugOutput.Emission)
      {
         return MicroSplatBaseFeatures.DefineFeature._DEBUG_OUTPUT_EMISSION;
      }
      else if (p == MicroSplatBaseFeatures.DebugOutput.Height)
      {
         return MicroSplatBaseFeatures.DefineFeature._DEBUG_OUTPUT_HEIGHT;
      }
      else if (p == MicroSplatBaseFeatures.DebugOutput.Metallic)
      {
         return MicroSplatBaseFeatures.DefineFeature._DEBUG_OUTPUT_METAL;
      }
      else if (p == MicroSplatBaseFeatures.DebugOutput.Normal)
      {
         return MicroSplatBaseFeatures.DefineFeature._DEBUG_OUTPUT_NORMAL;
      }
      else if (p == MicroSplatBaseFeatures.DebugOutput.Smoothness)
      {
         return MicroSplatBaseFeatures.DefineFeature._DEBUG_OUTPUT_SMOOTHNESS;
      }
      return MicroSplatBaseFeatures.DefineFeature._DEBUG_OUTPUT_ALBEDO;
   }

   static MicroSplatBaseFeatures.DebugOutput OutputFromPass(BakingPasses p)
   {
      if (p == BakingPasses.Albedo)
      {
         return MicroSplatBaseFeatures.DebugOutput.Albedo;
      }
      else if (p == BakingPasses.AO)
      {
         return MicroSplatBaseFeatures.DebugOutput.AO;
      }
      else if (p == BakingPasses.Emissive)
      {
         return MicroSplatBaseFeatures.DebugOutput.Emission;
      }
      else if (p == BakingPasses.Height)
      {
         return MicroSplatBaseFeatures.DebugOutput.Height;
      }
      else if (p == BakingPasses.Metallic)
      {
         return MicroSplatBaseFeatures.DebugOutput.Metallic;
      }
      else if (p == BakingPasses.Normal)
      {
         return MicroSplatBaseFeatures.DebugOutput.Normal;
      }
      else if (p == BakingPasses.Smoothness)
      {
         return MicroSplatBaseFeatures.DebugOutput.Smoothness;
      }
      return MicroSplatBaseFeatures.DebugOutput.Albedo;
   }

   static void RemoveKeyword(List<string> keywords, string keyword)
   {
      if (keywords.Contains(keyword))
      {
         keywords.Remove(keyword);
      }
   }

   static Material SetupMaterial(Material mat, MicroSplatBaseFeatures.DebugOutput debugOutput)
   {
      MicroSplatShaderGUI.MicroSplatCompiler comp = new MicroSplatShaderGUI.MicroSplatCompiler();
      List<string> keywords = new List<string>(mat.shaderKeywords);

      RemoveKeyword(keywords, "_SNOW");
      RemoveKeyword(keywords, "_TESSDISTANCE");
      RemoveKeyword(keywords, "_WINDPARTICULATE");
      RemoveKeyword(keywords, "_SNOWPARTICULATE");
      RemoveKeyword(keywords, "_GLITTER");
      RemoveKeyword(keywords, "_SNOWGLITTER");

      keywords.Add(FeatureFromOutput(debugOutput).ToString());

      string shader = comp.Compile(keywords.ToArray(), "RenderBake_" + debugOutput.ToString());
      Shader s = ShaderUtil.CreateShaderAsset(shader);
      Material renderMat = new Material(mat);
      renderMat.shader = s;
      return renderMat;
   }


   public static Texture2D Bake(MicroSplatTerrain mst, BakingPasses p, int resolution)
   {
      Camera cam = new GameObject("cam").AddComponent<Camera>();
      cam.orthographic = true;
      cam.orthographicSize = 0.5f;
      cam.transform.position = new Vector3(0.5f, 10000.5f, -1);
      cam.nearClipPlane = 0.1f;
      cam.farClipPlane = 2.0f;
      cam.enabled = false;
      cam.depthTextureMode = DepthTextureMode.None;
      cam.clearFlags = CameraClearFlags.Color;
      cam.backgroundColor = Color.grey;

      var debugOutput = OutputFromPass(p);
      var readWrite = (debugOutput == MicroSplatBaseFeatures.DebugOutput.Albedo || debugOutput == MicroSplatBaseFeatures.DebugOutput.Emission) ?
         RenderTextureReadWrite.sRGB : RenderTextureReadWrite.Linear;

      RenderTexture rt = RenderTexture.GetTemporary(resolution, resolution, 0, RenderTextureFormat.ARGB32, readWrite);
      RenderTexture.active = rt;
      cam.targetTexture = rt;

      GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
      go.transform.position = new Vector3(0, 10000, 0);
      cam.transform.position = new Vector3(0, 10000, -1);
      Material renderMat = SetupMaterial(mst.matInstance, debugOutput);
      go.GetComponent<MeshRenderer>().sharedMaterial = renderMat;
      bool fog = RenderSettings.fog;
      Unsupported.SetRenderSettingsUseFogNoDirty(false);
      cam.Render();
      Unsupported.SetRenderSettingsUseFogNoDirty(fog);
      Texture2D tex = new Texture2D(resolution, resolution, TextureFormat.ARGB32, false);
      tex.ReadPixels(new Rect(0, 0, resolution, resolution), 0, 0);
      RenderTexture.active = null;
      RenderTexture.ReleaseTemporary(rt);

      tex.Apply();


      MeshRenderer mr = go.GetComponent<MeshRenderer>();
      if (mr != null)
      {
         if (mr.sharedMaterial != null)
         {
            if (mr.sharedMaterial.shader != null)
               GameObject.DestroyImmediate(mr.sharedMaterial.shader);
            GameObject.DestroyImmediate(mr.sharedMaterial);
         }
      }

      GameObject.DestroyImmediate(go);
      GameObject.DestroyImmediate(cam.gameObject);
      return tex;
   }

   void Bake(MicroSplatTerrain mst)
   {
      
      // for each pass
      int pass = 1;
      while (pass <= (int)(BakingPasses.Emissive))
      {
         BakingPasses p = (BakingPasses)pass;
         pass *= 2;
         if (!IsEnabled(p))
         {
            continue;
         }
         var debugOutput = OutputFromPass(p);
         var tex = Bake(mst, p, (int)res);
         var bytes = tex.EncodeToPNG();
         
         string texPath = MicroSplatUtilities.RelativePathFromAsset(mst.terrain) + "/" + mst.terrain.name + "_" + debugOutput.ToString();
         System.IO.File.WriteAllBytes(texPath + ".png", bytes);

      }

      AssetDatabase.Refresh();
   }


}
