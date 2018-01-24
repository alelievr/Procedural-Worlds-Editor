//////////////////////////////////////////////////////
// MicroSplat
// Copyright (c) Jason Booth, slipster216@gmail.com
//////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

namespace JBooth.MicroSplat
{
   public class MicroSplatUtilities 
   {
      public static string MakeRelativePath(string path)
      {
         path = path.Replace(Application.dataPath, "Assets/");
         path = path.Replace("\\", "/");
         path = path.Replace("//", "/");
         return path;
      }

      public static string MakeAbsolutePath(string path)
      {
         if (!path.StartsWith(Application.dataPath))
         {
            if (path.StartsWith("Assets"))
            {
               path = path.Substring(6);
               path = Application.dataPath + path;
            }
         }
         return path;
      }

      static Dictionary<string, Texture2D> autoTextures = null;

      public static void EnforceDefaultTexture(MaterialProperty texProp, string autoTextureName)
      {
         if (texProp.textureValue == null)
         {
            Texture2D def = MicroSplatUtilities.GetAutoTexture(autoTextureName);
            if (def != null)
            {
               texProp.textureValue = def;
            }
         }
      }

      public static Texture2D GetAutoTexture(string name)
      {
         if (autoTextures == null)
         {
            autoTextures = new Dictionary<string, Texture2D>();
            var guids = AssetDatabase.FindAssets("microsplat_def_ t:texture2D");
            for (int i = 0; i < guids.Length; ++i)
            {
               Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(guids[i]));
               autoTextures.Add(tex.name, tex);
            }
         }
         Texture2D ret;
         if (autoTextures.TryGetValue(name, out ret))
         {
            return ret;
         }
         return null;
      }

      public static void DrawTextureField(MicroSplatTerrain t, GUIContent content, ref Texture2D tex, string keword )
      {
          if (t.templateMaterial.IsKeywordEnabled(keword))
          {
              EditorGUI.BeginChangeCheck();

              tex = EditorGUILayout.ObjectField(content, tex, typeof(Texture2D), false) as Texture2D;

              if (EditorGUI.EndChangeCheck())
              {
                    EditorUtility.SetDirty(t);
              }
          }
      }


      static Dictionary<string, bool> rolloutStates = new Dictionary<string, bool>();
      static GUIStyle rolloutStyle;
      public static bool DrawRollup(string text, bool defaultState = true, bool inset = false)
      {
         if (rolloutStyle == null)
         {
            rolloutStyle = GUI.skin.box;
            rolloutStyle.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
         }
         var oldColor = GUI.contentColor;
         GUI.contentColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
         if (inset == true)
         {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.GetControlRect(GUILayout.Width(40));
         }

         if (!rolloutStates.ContainsKey(text))
         {
            rolloutStates[text] = defaultState;
         }
         if (GUILayout.Button(text, rolloutStyle, new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.Height(20)}))
         {
            rolloutStates[text] = !rolloutStates[text];
         }
         if (inset == true)
         {
            EditorGUILayout.GetControlRect(GUILayout.Width(40));
            EditorGUILayout.EndHorizontal();
         }
         GUI.contentColor = oldColor;
         return rolloutStates[text];
      }

      public static void DrawSeparator()
      {
         EditorGUILayout.Separator();
         GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(1) });
         EditorGUILayout.Separator();
      }
     
      static List<TextureArrayPreviewCache> previewCache = new List<TextureArrayPreviewCache>(16);

      static Texture2D FindInPreviewCache(int hash)
      {
         for (int i = 0; i < previewCache.Count; ++i)
         {
            if (previewCache[i].hash == hash)
               return previewCache[i].texture;
         }
         return null;
      }

      public static void ClearPreviewCache()
      {
         for (int i = 0; i < previewCache.Count; ++i)
         {
            if (previewCache[i].texture != null)
            {
               GameObject.DestroyImmediate(previewCache[i].texture);
            }
         }
         previewCache.Clear();
      }


      // for caching previews
      public class TextureArrayPreviewCache
      {
         public int hash;
         public Texture2D texture;
      }

      // workaround for unity's editor bug w/ linear. Blit texture into linear render buffer,
      // readback int linear texture, write into PNG, read back from PNG. Cause Unity..
      public static void FixUnityEditorLinearBug(ref Texture2D tex, int width = 128, int height = 128)
      {
         RenderTexture rt = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
         Graphics.Blit(tex, rt);
         GameObject.DestroyImmediate(tex);
         var tempTex = new Texture2D(width, height, TextureFormat.RGBA32, true, true);
         RenderTexture.active = rt;
         tempTex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
         RenderTexture.active = null;
         tex = new Texture2D(2, 2, TextureFormat.RGBA32, true, true);
         tex.LoadImage(tempTex.EncodeToPNG());
         GameObject.DestroyImmediate(tempTex);
      }

      public static int DrawTextureSelector(int textureIndex, Texture2DArray ta, bool compact = false)
      {
         if (ta == null)
            return textureIndex;
         int count = ta.depth;
         if (count > 16)
            count = 16;
         Texture2D disp = Texture2D.blackTexture;
         if (ta != null)
         {
            int hash = ta.GetHashCode() * (textureIndex + 7);
            Texture2D hashed = FindInPreviewCache(hash);
            if (hashed == null)
            {
               hashed = new Texture2D(ta.width, ta.height, ta.format, false);
               Graphics.CopyTexture(ta, textureIndex, 0, hashed, 0, 0);
               hashed.Apply(false, false);
               //FixUnityEditorLinearBug(ref hashed);
               var hd = new TextureArrayPreviewCache();
               hd.hash = hash;
               hd.texture = hashed;
               previewCache.Add(hd);
               if (previewCache.Count > 20)
               {
                  hd = previewCache[0];
                  previewCache.RemoveAt(0);
                  if (hd.texture != null)
                  {
                     GameObject.DestroyImmediate(hd.texture);
                  }
               }

            }
            disp = hashed;
         }
         if (compact)
         {
            EditorGUILayout.BeginVertical();
            EditorGUI.DrawPreviewTexture(EditorGUILayout.GetControlRect(GUILayout.Width(110), GUILayout.Height(96)), disp);
            textureIndex = EditorGUILayout.IntSlider(textureIndex, 0, count-1, GUILayout.Width(120));
            EditorGUILayout.EndVertical();

         }
         else
         {
            textureIndex = EditorGUILayout.IntSlider("index", textureIndex, 0, count-1);
            EditorGUI.DrawPreviewTexture(EditorGUILayout.GetControlRect(GUILayout.Width(128), GUILayout.Height(128)), disp);
         }
         return textureIndex;
      }


      public static string RelativePathFromAsset(UnityEngine.Object o)
      {
         string path = null;
         if (o != null)
         {
            path = AssetDatabase.GetAssetPath(o);
         }
         if (string.IsNullOrEmpty(path))
         {
            string selectionPath = AssetDatabase.GetAssetPath (Selection.activeObject);
            if (!string.IsNullOrEmpty(selectionPath))
            {
               path = selectionPath;
            }
         }
         if (string.IsNullOrEmpty(path))
         {
            path = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().path;
         }

         if (string.IsNullOrEmpty(path))
         {
            path = "Assets";
         }

         path = path.Replace("\\", "/");
         if (path.Contains("/"))
         {
            path = path.Substring(0, path.LastIndexOf("/"));
         }
         path += "/MicroSplatData";
         if (!System.IO.Directory.Exists(path))
         {
            System.IO.Directory.CreateDirectory(path);
         }

         return path;
      }

   }
}
