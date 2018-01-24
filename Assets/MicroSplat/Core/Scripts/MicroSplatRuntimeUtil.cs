using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace JBooth.MicroSplat
{
   public class MicroSplatRuntimeUtil 
   {
      // convert to/from regular UVs to terrain UVs, which are expressed in units per tile instead of tiles per terrain
      public static Vector2 UnityUVScaleToUVScale(Vector2 uv, Terrain t)
      {
         float w = t.terrainData.size.x;
         float h = t.terrainData.size.z;
         uv.x = 1.0f / (uv.x / w);
         uv.y = 1.0f / (uv.y / h);
         return uv;
      }

      public static Vector2 UVScaleToUnityUVScale(Vector2 uv, Terrain t)
      {
         float w = t.terrainData.size.x;
         float h = t.terrainData.size.y;

         if (uv.x < 0)
            uv.x = 0.001f;
         if (uv.y < 0)
            uv.y = 0.001f;

         uv.x = w/uv.x;              
         uv.y = h/uv.y;
         return uv;
      }
   }
}
