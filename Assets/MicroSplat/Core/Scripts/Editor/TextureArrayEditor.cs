using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// thanks to Lennart @ awesome technologies
namespace JBooth.MicroSplat.Utility
{
   [CustomEditor(typeof(Texture2DArray))]
   public class TextureArrayEditor : Editor
   {
      private readonly List<Texture2D> _previewTextureList = new List<Texture2D>();

      public void OnEnable()
      {
         Texture2DArray texture2DArray = (Texture2DArray)target;
         for (int i = 0; i <= texture2DArray.depth - 1; i++)
         {
           
            // annoying to work around all the odd unity issues
            // copy to temp texture..
            Texture2D tempTexture = new Texture2D(texture2DArray.width, texture2DArray.height, texture2DArray.format, true, true);
            Graphics.CopyTexture(texture2DArray, i, tempTexture, 0);
            tempTexture.Apply();

            // blit to render target
            RenderTexture rt = RenderTexture.GetTemporary(256, 256, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            Graphics.Blit(tempTexture, rt);

            // read back from render target
            DestroyImmediate(tempTexture);
            tempTexture = new Texture2D(256, 256, TextureFormat.ARGB32, false, true);
            var old = RenderTexture.active;
            RenderTexture.active = rt;
            tempTexture.ReadPixels(new Rect(0, 0, 256, 256), 0, 0);
            tempTexture.Apply();
            RenderTexture.active = old;

            // work around linear/gamma issue with GUI.
            tempTexture.LoadImage(tempTexture.EncodeToJPG());
            tempTexture.Apply();
            _previewTextureList.Add(tempTexture);
         }
      }

      public void OnDisable()
      {
         for (int i = 0; i <= _previewTextureList.Count - 1; i++)
         {
            DestroyImmediate(_previewTextureList[i]);
         }
         _previewTextureList.Clear();
      }

      public override void OnInspectorGUI()
      {
         EditorGUILayout.LabelField("Texture array content");

         Texture2DArray texture2DArray = (Texture2DArray)target;
         GUIContent[] imageButtons = new GUIContent[texture2DArray.depth];

         for (int i = 0; i <= texture2DArray.depth - 1; i++)
         {
            if (_previewTextureList.Count > i)
            {
               imageButtons[i] = new GUIContent { image = _previewTextureList[i] };
            }
            else
            {
               imageButtons[i] = new GUIContent { image = null };
            }
         }
         int imageWidth = 120;
         int columns = Mathf.FloorToInt(EditorGUIUtility.currentViewWidth / imageWidth);
         int rows = Mathf.CeilToInt((float)imageButtons.Length / columns);
         int gridHeight = (rows) * imageWidth;
         GUILayout.SelectionGrid(0, imageButtons, columns, GUI.skin.label, GUILayout.MaxWidth(columns * imageWidth), GUILayout.MaxHeight(gridHeight));

         texture2DArray.anisoLevel = EditorGUILayout.IntField("Anisotropic", texture2DArray.anisoLevel);
         texture2DArray.filterMode = (FilterMode)EditorGUILayout.EnumPopup("Filter Mode", texture2DArray.filterMode);
         texture2DArray.wrapMode = (TextureWrapMode)EditorGUILayout.EnumPopup("Wrap Mode", texture2DArray.wrapMode);
         texture2DArray.mipMapBias = EditorGUILayout.FloatField("Mip Map Bias", texture2DArray.mipMapBias);

         EditorGUILayout.LabelField("Texture count: " + texture2DArray.depth);
         EditorGUILayout.LabelField("Width: " + texture2DArray.width);
         EditorGUILayout.LabelField("Height: " + texture2DArray.height);
         EditorGUILayout.LabelField("Texture format: " + texture2DArray.format);

      }
   }
}
