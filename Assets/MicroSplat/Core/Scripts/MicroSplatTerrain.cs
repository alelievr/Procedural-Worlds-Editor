using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using JBooth.MicroSplat;

[ExecuteInEditMode]
[RequireComponent(typeof(Terrain))]
[DisallowMultipleComponent]
public class MicroSplatTerrain : MonoBehaviour 
{
   [Tooltip("Material to use for terrain")]
   public Material templateMaterial;
   [System.NonSerialized]
   [HideInInspector]
   public Material matInstance;

   [HideInInspector]
   public Texture2D terrainDesc;

   [HideInInspector]
   public Material blendMat;

   [HideInInspector]
   public Material blendMatInstance;

   [HideInInspector] // for build include
   public Shader addPass;

   public delegate void MaterialSyncAll();
   public delegate void MaterialSync(Material m);

   public static event MaterialSyncAll OnMaterialSyncAll;
   public event MaterialSync OnMaterialSync;

   static List<MicroSplatTerrain> sInstances = new List<MicroSplatTerrain>();

   public Terrain terrain;

   [HideInInspector]
   public Texture2D tintMapOverride;
   [HideInInspector]
   public Texture2D globalNormalOverride;
   [HideInInspector]
   public Texture2D geoTextureOverride;
   [HideInInspector]
   public Texture2D streamTexture;
   [HideInInspector]
   public Texture2D vsGrassMap;
   [HideInInspector]
   public Texture2D vsShadowMap;
   [HideInInspector]
   public Texture2D advDetailControl;

    [HideInInspector]
   public MicroSplatPropData propData;

   #if UNITY_EDITOR
   [HideInInspector]
   public int oldBaseMapSize;
   [HideInInspector]
   public float oldBaseMapDistance;
   #endif

   // LOTS of hacking around Unity bugs. In some versions of Unity, accessing any data on the terrain too early can cause a crash, either
   // in editor, or in playmode, but not in the same way. Really just want to call Sync on Enabled, and Cleanup on Disabled, but triggers
   // too many issues, so we dodge the bullet in various cases to hack around it- so ugly.

   void Awake()
   {
      terrain = GetComponent<Terrain>();
      #if UNITY_EDITOR
      Sync();
      #endif
   }
      
   void OnEnable()
   {
      terrain = GetComponent<Terrain>();
      sInstances.Add(this);
      #if UNITY_EDITOR
      Sync();
      #else
      if (reenabled)
      {
         Sync();
      }
      #endif
   }

   #if !UNITY_EDITOR
   void Start()
   {
      Sync();
   }
   #endif

   [HideInInspector]
   public bool reenabled = false;
   void OnDisable()
   {
      sInstances.Remove(this);
      Cleanup();
      reenabled = true;
   }

   #if UNITY_EDITOR
   public bool sTerrainDirty = false;
   void OnTerrainChanged(int f)
   {
      TerrainChangedFlags flags = (TerrainChangedFlags)f;
      if ((flags & TerrainChangedFlags.Heightmap) != 0)
      {
         sTerrainDirty = true;
      }

      if ((flags & TerrainChangedFlags.DelayedHeightmapUpdate) != 0)
      {
         sTerrainDirty = true;
      }
   }
   #endif

   void Cleanup()
   {
      if (matInstance != null && matInstance != templateMaterial)
      {
         DestroyImmediate(matInstance);
         terrain.materialTemplate = null;
      }
      #if UNITY_EDITOR
      terrain.basemapDistance = oldBaseMapDistance;
      if (terrain.terrainData != null)
      {
         terrain.terrainData.baseMapResolution = Mathf.Max(16, oldBaseMapSize);
      }
      #endif
   }

   public void Sync()
   {
      if (templateMaterial == null)
         return;

      #if UNITY_EDITOR
      if (terrain.materialType != Terrain.MaterialType.Custom || terrain.materialTemplate == null)
      {
         oldBaseMapSize = terrain.terrainData.baseMapResolution;
         oldBaseMapDistance = terrain.basemapDistance;
      }
      if (addPass == null)
      {
         addPass = Shader.Find("Hidden/MicroSpat/AddPass");
      }
      #endif
      Material m = null;



      if (terrain.materialTemplate == matInstance && matInstance != null)
      {
         terrain.materialTemplate.CopyPropertiesFromMaterial(templateMaterial);
         m = terrain.materialTemplate;
      }
      else
      {
         m = new Material(templateMaterial);
      }
      terrain.materialType = Terrain.MaterialType.Custom;


      m.hideFlags = HideFlags.HideAndDontSave;
      terrain.materialTemplate = m;
      matInstance = m;

      if (m.IsKeywordEnabled("_GEOMAP") && geoTextureOverride != null)
      {
         m.SetTexture("_GeoTex", geoTextureOverride);
      }
      if (m.IsKeywordEnabled("_GEOCURVE") && propData != null)
      {
         m.SetTexture("_GeoCurve", propData.GetGeoCurve());
      }

      if (m.IsKeywordEnabled("_GLOBALTINT") && tintMapOverride != null)
      {
         m.SetTexture("_GlobalTintTex", tintMapOverride);
      }
      if (m.IsKeywordEnabled("_GLOBALNORMALS") && globalNormalOverride != null)
      {
         m.SetTexture("_GlobalNormalTex", globalNormalOverride);
      }
      if (m.IsKeywordEnabled("_VSGRASSMAP") && vsGrassMap != null)
      {
         m.SetTexture("_VSGrassMap", vsGrassMap);
      }
      if (m.IsKeywordEnabled("_VSSHADOWMAP") && vsShadowMap != null)
      {
         m.SetTexture("_VSShadowMap", vsShadowMap);
      }
      if (m.HasProperty("_StreamControl"))
      {
         m.SetTexture("_StreamControl", streamTexture);
      }
      if (m.IsKeywordEnabled("_ADVANCED_DETAIL"))
      {
         m.SetTexture("_AdvDetailControl", advDetailControl);
      }

      if (propData != null)
      {
         m.SetTexture("_PerTexProps", propData.GetTexture());
      }


         
      var controls = terrain.terrainData.alphamapTextures;
      m.SetTexture("_Control0", controls.Length > 0 ? controls[0] : Texture2D.blackTexture);
      m.SetTexture("_Control1", controls.Length > 1 ? controls[1] : Texture2D.blackTexture);
      m.SetTexture("_Control2", controls.Length > 2 ? controls[2] : Texture2D.blackTexture);
      m.SetTexture("_Control3", controls.Length > 3 ? controls[3] : Texture2D.blackTexture);

      // set base map distance to max of fancy features
      // base map does not use the "base map" texture, so slam to 16
      // only do this stuff editor time to avoid runtime cost
      #if UNITY_EDITOR
      if (terrain.terrainData.baseMapResolution != 16)
      {
         terrain.terrainData.baseMapResolution = 16; 
      }
      float basemapDistance = 0;

      if (m.HasProperty("_TessData2"))
      {
         float d = m.GetVector("_TessData2").y;
         if (d > basemapDistance)
            basemapDistance = d;
      }
      if (m.HasProperty("_ParallaxParams"))
      {
         Vector4 v = m.GetVector("_ParallaxParams");
         float d = v.y + v.z;
         if (d > basemapDistance)
            basemapDistance = d;
      }
      if (m.HasProperty("_DetailNoiseScaleStrengthFade"))
      {
         float d = m.GetVector("_DetailNoiseScaleStrengthFade").z;
         if (d > basemapDistance)
            basemapDistance = d;
      }

      terrain.basemapDistance = basemapDistance;
      #endif

      if (OnMaterialSync != null)
      {
         OnMaterialSync(m);
      }

      if (blendMat != null && terrainDesc != null)
      {
         if (blendMatInstance == null)
         {
            blendMatInstance = new Material(blendMat);
         }

         SyncBlendMat();
      }


      #if UNITY_EDITOR
         RestorePrototypes();
      #endif

   }

   void SyncBlendMat()
   {
      if (blendMatInstance != null && matInstance != null)
      {
         blendMatInstance.CopyPropertiesFromMaterial(matInstance);
         Vector4 bnds = new Vector4();
         bnds.z = terrain.terrainData.size.x;
         bnds.w = terrain.terrainData.size.z;
         bnds.x = transform.position.x;
         bnds.y = transform.position.z;
         blendMatInstance.SetVector("_TerrainBounds", bnds);
         blendMatInstance.SetTexture("_TerrainDesc", terrainDesc);
      }
   }

   public Material GetBlendMatInstance()
   {
      if (blendMat != null && terrainDesc != null)
      {
         if (blendMatInstance == null)
         {
            blendMatInstance = new Material(blendMat);
            SyncBlendMat();
         }
         if (blendMatInstance.shader != blendMat.shader)
         {
            blendMatInstance.shader = blendMat.shader;
            SyncBlendMat();
         }
      }
      return blendMatInstance;
   }

      
   #if UNITY_EDITOR
  

   void RestorePrototypes()
   {
      if (templateMaterial != null)
      {
         Texture2DArray diffuseArray = templateMaterial.GetTexture("_Diffuse") as Texture2DArray;
         if (diffuseArray != null)
         {
            var cfg = JBooth.MicroSplat.TextureArrayConfig.FindConfig(diffuseArray);
            if (cfg != null)
            {
               int count = cfg.sourceTextures.Count;
               if (count > 16)
                  count = 16;

               // see if we match
               var protos = terrain.terrainData.splatPrototypes;
               bool needsRefresh = false;
               if (protos.Length != count)
               {
                  needsRefresh = true;
               }
               if (!needsRefresh)
               {
                  for (int i = 0; i < protos.Length; ++i)
                  {
                     if (protos[i].texture != cfg.sourceTextures[i].diffuse)
                     {
                        needsRefresh = true;
                     }
                  }
               }

               if (needsRefresh)
               {
                  Vector4 v4 = templateMaterial.GetVector("_UVScale");

                  Vector2 uvScales = new Vector2(v4.x, v4.y);
                  uvScales = MicroSplatRuntimeUtil.UVScaleToUnityUVScale(uvScales, terrain);

                  protos = new SplatPrototype[count];
                  for (int i = 0; i < count; ++i)
                  {
                     SplatPrototype sp = new SplatPrototype();
                     sp.texture = cfg.sourceTextures[i].diffuse;
                     sp.tileSize = uvScales;
                     if (cfg.sourceTextures[i].normal != null)
                     {
                        sp.normalMap = cfg.sourceTextures[i].normal;
                     }
                     protos[i] = sp;
                  }

                  terrain.terrainData.splatPrototypes = protos;
                  UnityEditor.EditorUtility.SetDirty(terrain);
                  UnityEditor.EditorUtility.SetDirty(terrain.terrainData);
               }
            }
         }

      }
   }
   #endif

   public static void SyncAll()
   {
      for (int i = 0; i < sInstances.Count; ++i)
      {
         sInstances[i].Sync();
      }
      if (OnMaterialSyncAll != null)
      {
         OnMaterialSyncAll();
      }
   }


}
