using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using JBooth.MicroSplat;

[CustomEditor(typeof(MicroSplatTerrain))]
[CanEditMultipleObjects]
public partial class MicroSplatTerrainEditor : Editor
{
#if __MICROSPLAT__
   static GUIContent geoTexOverride = new GUIContent("Geo Texture Override", "If you want each terrain object to have it's own geo texture instead of the one defined in the material, add it here");
   static GUIContent geoTintOverride = new GUIContent("Tint Texture Override", "If you want each terrain object to have it's own global tint instead of the one defined in the material, add it here");
   static GUIContent geoNormalOverride = new GUIContent("Global Normal Override", "If you want each terrain object to have it's own global normal instead of the one defined in the material, add it here");
   static GUIContent CTemplateMaterial = new GUIContent("Template Material", "Material to use for this terrain");

   static GUIContent CVSGrassMap = new GUIContent("Grass Map", "Grass Map from Vegetation Studio");
   static GUIContent CVSShadowMap = new GUIContent("Shadow Map", "Shadow map texture from Vegetation Studio");

   static GUIContent AdvDetailControl = new GUIContent("Advanced Detail Control", "Control map for Advanced Details"); 

   public override void OnInspectorGUI()
   {
      MicroSplatTerrain t = target as MicroSplatTerrain;
      if (t == null)
      {
         EditorGUILayout.HelpBox("No Terrain Present, please put this component on a terrain", MessageType.Error);
         return;
      }
      EditorGUI.BeginChangeCheck();
      t.templateMaterial = EditorGUILayout.ObjectField(CTemplateMaterial, t.templateMaterial, typeof(Material), false) as Material;
      t.propData = EditorGUILayout.ObjectField("Per Texture Data", t.propData, typeof(MicroSplatPropData), false) as MicroSplatPropData;
      if (EditorGUI.EndChangeCheck())
      {
         EditorUtility.SetDirty(t);
      }
      if (t.templateMaterial == null)
      {
         if (GUILayout.Button("Convert to MicroSplat"))
         {
            // get all terrains in selection, not just this one, and treat as one giant terrain
            var objs = Selection.gameObjects;
            List<Terrain> terrains = new List<Terrain>();
            for (int i = 0; i < objs.Length; ++i)
            {
               Terrain ter = objs[i].GetComponent<Terrain>();
               if (ter != null)
               {
                  terrains.Add(ter);
               }
               Terrain[] trs = objs[i].GetComponentsInChildren<Terrain>();
               for (int x = 0; x < trs.Length; ++x)
               {
                  if (!terrains.Contains(trs[x]))
                  {
                     terrains.Add(trs[x]);
                  }
               }
            }
            // setup this terrain
            Terrain terrain = t.GetComponent<Terrain>();
            t.templateMaterial = MicroSplatShaderGUI.NewShaderAndMaterial(terrain);
            var config = TextureArrayConfigEditor.CreateConfig(terrain);
            t.templateMaterial.SetTexture("_Diffuse", config.diffuseArray);
            t.templateMaterial.SetTexture("_NormalSAO", config.normalSAOArray);

            t.propData = MicroSplatShaderGUI.FindOrCreatePropTex(t.templateMaterial);
            bool perTexScaleOffset = false;
            if (terrain.terrainData.splatPrototypes.Length > 0)
            {
               var uvScale = terrain.terrainData.splatPrototypes[0].tileSize;
               var uvOffset = terrain.terrainData.splatPrototypes[0].tileOffset;
               for (int i = 1; i < terrain.terrainData.splatPrototypes.Length; ++i)
               {
                  if (uvScale != terrain.terrainData.splatPrototypes[0].tileSize ||
                      uvOffset != terrain.terrainData.splatPrototypes[0].tileOffset)
                  {
                     perTexScaleOffset = true;
                  }
               }
               if (!perTexScaleOffset)
               {
                  uvScale = MicroSplatRuntimeUtil.UnityUVScaleToUVScale(uvScale, terrain);
                  uvOffset.x = uvScale.x / terrain.terrainData.size.x * 0.5f * uvOffset.x;
                  uvOffset.y = uvScale.y / terrain.terrainData.size.x * 0.5f * uvOffset.y;
                  Vector4 scaleOffset = new Vector4(uvScale.x, uvScale.y, uvOffset.x, uvOffset.y);
                  t.templateMaterial.SetVector("_UVScale", scaleOffset);
               }
            }

            if (perTexScaleOffset)
            {
               t.templateMaterial.SetVector("_UVScale", new Vector4(1, 1, 0, 0));
               t.templateMaterial.EnableKeyword("_PERTEXUVSCALE");
               t.templateMaterial.EnableKeyword("_PERTEXUVOFFSET");
               var propTex = MicroSplatShaderGUI.FindOrCreatePropTex(t.templateMaterial);
               // set per tex props for each texture
               for (int i = 0; i < terrain.terrainData.splatPrototypes.Length; ++i)
               {
                  var uvScale = terrain.terrainData.splatPrototypes[i].tileSize;
                  var uvOffset = terrain.terrainData.splatPrototypes[i].tileOffset;
                  uvScale = MicroSplatRuntimeUtil.UnityUVScaleToUVScale(uvScale, terrain);
                  uvOffset.x = uvScale.x / terrain.terrainData.size.x * 0.5f * uvOffset.x;
                  uvOffset.y = uvScale.y / terrain.terrainData.size.x * 0.5f * uvOffset.y;
                  Color c = new Color(uvScale.x, uvScale.y, uvOffset.x, uvOffset.y);
                  propTex.SetValue(i, 0, c);
               }
            }

            // now make sure others all have the same settings as well.
            for (int i = 0; i < terrains.Count; ++i)
            {
               var nt = terrains[i];
               var mgr = nt.GetComponent<MicroSplatTerrain>();
               if (mgr == null)
               {
                  mgr = nt.gameObject.AddComponent<MicroSplatTerrain>();
               }
               mgr.templateMaterial = t.templateMaterial;

               if (mgr.propData == null)
               {
                  mgr.propData = MicroSplatShaderGUI.FindOrCreatePropTex(mgr.templateMaterial);
               }
            }
            Selection.SetActiveObjectWithContext(config, config);
         }
         MicroSplatTerrain.SyncAll();
         return;
      }

      if (t.propData == null)
      {
         t.propData = MicroSplatShaderGUI.FindOrCreatePropTex(t.templateMaterial);
      }

#if __MICROSPLAT_TERRAINBLEND__ || __MICROSPLAT_STREAMS__
      DoTerrainDescGUI();
#endif

      // could move this to some type of interfaced component created by the module if this becomes a thing,
      // but I think this will be most of the cases?
      
      MicroSplatUtilities.DrawTextureField(t, geoTexOverride, ref t.geoTextureOverride, "_GEOMAP");

      MicroSplatUtilities.DrawTextureField(t, geoTintOverride, ref t.tintMapOverride, "_GLOBALTINT");

      MicroSplatUtilities.DrawTextureField(t, geoNormalOverride, ref t.globalNormalOverride, "_GLOBALNORMALS");

      MicroSplatUtilities.DrawTextureField(t, AdvDetailControl, ref t.advDetailControl, "_ADVANCED_DETAIL");


      if (t.templateMaterial.IsKeywordEnabled("_VSGRASSMAP"))
      {
         EditorGUI.BeginChangeCheck();

         t.vsGrassMap = EditorGUILayout.ObjectField(CVSGrassMap, t.vsGrassMap, typeof(Texture2D), false) as Texture2D;

         if (EditorGUI.EndChangeCheck())
         {
            EditorUtility.SetDirty(t);
         }
      }

      if (t.templateMaterial.IsKeywordEnabled("_VSSHADOWMAP"))
      {
         EditorGUI.BeginChangeCheck();


         t.vsShadowMap = EditorGUILayout.ObjectField(CVSShadowMap, t.vsShadowMap, typeof(Texture2D), false) as Texture2D;

         if (EditorGUI.EndChangeCheck())
         {
            EditorUtility.SetDirty(t);
         }
      }


      EditorGUILayout.BeginHorizontal();
      if (GUILayout.Button("Sync"))
      {
         var mgr = target as MicroSplatTerrain;
         mgr.Sync();
      }
      if (GUILayout.Button("Sync All"))
      {
         MicroSplatTerrain.SyncAll();
      }
      EditorGUILayout.EndHorizontal();

      BakingGUI(t);
      WeightLimitingGUI(t);
   }
#endif
}
