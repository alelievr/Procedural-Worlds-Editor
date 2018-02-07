//////////////////////////////////////////////////////
// MicroSplat
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////


using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Collections.Generic;

namespace JBooth.MicroSplat 
{
   #if __MICROSPLAT__ && VEGETATION_STUDIO
   [InitializeOnLoad]
   public class MicroSplatVegetationStudio : FeatureDescriptor
   {
      public override string ModuleName()
      {
         return "Vegetation Studio";
      }

      public enum DefineFeature
      {
         _VSGRASSMAP,
         _VSSHADOWMAP,
         _VSSHADOWTAPNONE,
         _VSSHADOWTAPLOW,
         _VSSHADOWTAPMEDIUM,
         _VSSHADOWTAPHIGH,
         kNumFeatures,
      }

      public enum ShadowTapCount
      {
         None,
         Low,
         Medium,
         High,
      }

      public bool grassMap;
      public bool shadowMap;
      public ShadowTapCount shadowTapCount = ShadowTapCount.Medium;

      public TextAsset propsGrassMap;
      public TextAsset funcGrassMap;
      public TextAsset propsShadowMap;
      public TextAsset funcShadowMap;

      GUIContent CShaderGrassMap = new GUIContent("Vegetation Studio GrassMap", "Enable texturing of distant grasses");
      GUIContent CShaderShadowMap = new GUIContent("Vegetation Studio ShadowMap", "Enable distance shadows for trees");
      GUIContent CShaderTapCount = new GUIContent("Shadow Map Quality", "Higher quality gives smoother shadows in the distance");
      // Can we template these somehow?
      public static string GetFeatureName(DefineFeature feature)
      {
         return System.Enum.GetName(typeof(DefineFeature), feature);
      }

      public static bool HasFeature(string[] keywords, DefineFeature feature)
      {
         string f = GetFeatureName(feature);
         for (int i = 0; i < keywords.Length; ++i)
         {
            if (keywords[i] == f)
               return true;
         }
         return false;
      }

      public override string GetVersion()
      {
         return "1.7";
      }

      public override void DrawFeatureGUI(Material mat)
      {
         grassMap = EditorGUILayout.Toggle(CShaderGrassMap, grassMap);
         shadowMap = EditorGUILayout.Toggle(CShaderShadowMap, shadowMap);
         if (shadowMap)
         {
            EditorGUI.indentLevel++;
            shadowTapCount = (ShadowTapCount)EditorGUILayout.EnumPopup(CShaderTapCount, shadowTapCount);
            EditorGUI.indentLevel--;
         }
      }

      GUIContent CShaderTint = new GUIContent("Grass Mask Tint", "Tint the grass overlay color, or reduce it's overall effect with the alpha");
      //GUIContent CShadowMap = new GUIContent("Shadow Map", "Shadow Map for distant terrain");

      public override void DrawShaderGUI(MicroSplatShaderGUI shaderGUI, Material mat, MaterialEditor materialEditor, MaterialProperty[] props)
      {
         if ((grassMap || shadowMap) && MicroSplatUtilities.DrawRollup("Vegetation Studio"))
         {
            if (grassMap && mat.HasProperty("_VSTint"))
            {
               EditorGUI.BeginChangeCheck();
               var c = mat.GetColor("_VSTint");
               c = EditorGUILayout.ColorField(CShaderTint, c);
               if (EditorGUI.EndChangeCheck())
               {
                  mat.SetColor("_VSTint", c);
                  EditorUtility.SetDirty(mat);
               }
            }
           
            if (shadowMap && mat.HasProperty("_VSShadowMap"))
            {
               var offsetProp = shaderGUI.FindProp("_VSShadowMapOffsetStrength", props);

               Vector4 v = offsetProp.vectorValue;
               EditorGUI.BeginChangeCheck();
               //v.x = EditorGUILayout.FloatField("Offset", v.x);
               v.y = EditorGUILayout.Slider("Min Tree Height", v.y, 0, 1.0f);
               v.z = EditorGUILayout.Slider("Shadow Strength", v.z, 0, 1.0f);
               v.w = EditorGUILayout.Slider("Shadow Ambient", v.w, 0, 1.0f);
               if (EditorGUI.EndChangeCheck())
               {
                  offsetProp.vectorValue = v;
               }
               
            }
           
         }
      }


      public override void DrawPerTextureGUI(int index, Material mat, MicroSplatPropData propData)
      {
         
      }

      public override void InitCompiler(string[] paths)
      {
         for (int i = 0; i < paths.Length; ++i)
         {
            string p = paths[i];
            if (p.EndsWith("microsplat_properties_vsgrassmap.txt"))
            {
               propsGrassMap = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("microsplat_func_vsgrassmap.txt"))
            {
               funcGrassMap = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("microsplat_properties_vsshadowmap.txt"))
            {
               propsShadowMap = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("microsplat_func_vsshadowmap.txt"))
            {
               funcShadowMap = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
         }
      }

      public override void WriteProperties(string[] features, System.Text.StringBuilder sb)
      {
         if (grassMap)
         {
            sb.Append(propsGrassMap.text);
         }
         
         if (shadowMap)
         {
            sb.Append(propsShadowMap.text);
         }
         

      }

      public override void ComputeSampleCounts(string[] features, ref int arraySampleCount, ref int textureSampleCount, ref int maxSamples, ref int tessellationSamples, ref int depTexReadLevel)
      {
         if (grassMap)
         {
            textureSampleCount++;
         }
         
         if (shadowMap)
         {
            textureSampleCount += ((int)shadowTapCount);
         }
         
      }

      public override string[] Pack()
      {
         List<string> features = new List<string>();
         if (grassMap)
         {
            features.Add(GetFeatureName(DefineFeature._VSGRASSMAP));
         }
         
         if (shadowMap)
         {
            features.Add(GetFeatureName(DefineFeature._VSSHADOWMAP));
            switch (shadowTapCount)
            {
               case ShadowTapCount.None:
                  features.Add(GetFeatureName(DefineFeature._VSSHADOWTAPNONE));
                  break;
               case ShadowTapCount.Low:
                  features.Add(GetFeatureName(DefineFeature._VSSHADOWTAPLOW));
                  break;
               case ShadowTapCount.Medium:
                  features.Add(GetFeatureName(DefineFeature._VSSHADOWTAPMEDIUM));
                  break;
               case ShadowTapCount.High:
                  features.Add(GetFeatureName(DefineFeature._VSSHADOWTAPHIGH));
                  break;

            }
         }
         

         return features.ToArray();
      }

      public override void WriteFunctions(System.Text.StringBuilder sb)
      {
         if (grassMap)
         {
            sb.AppendLine(funcGrassMap.text);
         }
         
         if (shadowMap)
         {
            sb.AppendLine(funcShadowMap.text);
         }
         
      }

      public override void Unpack(string[] keywords)
      {
         grassMap = HasFeature(keywords, DefineFeature._VSGRASSMAP);
         shadowMap = HasFeature(keywords, DefineFeature._VSSHADOWMAP);

         shadowTapCount = ShadowTapCount.Medium;
         if (HasFeature(keywords, DefineFeature._VSSHADOWTAPNONE))
         {
            shadowTapCount = ShadowTapCount.None;
         }
         else if (HasFeature(keywords, DefineFeature._VSSHADOWTAPLOW))
         {
            shadowTapCount = ShadowTapCount.Low;
         }
         else if (HasFeature(keywords, DefineFeature._VSSHADOWTAPHIGH))
         {
            shadowTapCount = ShadowTapCount.High;
         }

      }

   }   
   #endif


}