using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using JBooth.MicroSplat;
using System.Linq;

#if __MICROSPLAT__
public partial class MicroSplatTerrainEditor : Editor 
{

   struct WeightPair
   {
      public int index;
      public float weight;
   }

   int weightLimit = 4;
   bool multipassWeightLimit = false;
   GUIContent CMultipassWeight = new GUIContent("Multipass", "First pass limits weights per control pixel to weight limit. Second pass tests neighbors for non-shared weights, and reduces weights further on that pixel if found");

   void WeightLimitingGUI(MicroSplatTerrain t)
   {
      if (MicroSplatUtilities.DrawRollup("Weight Limiting", false))
      {
         weightLimit = EditorGUILayout.IntSlider("Weight Limit", weightLimit, 2, 4);
         multipassWeightLimit = EditorGUILayout.Toggle(CMultipassWeight, multipassWeightLimit);

         if (GUILayout.Button("Limit"))
         {
            WeightLimitTerrain(t, weightLimit, false);
            if (multipassWeightLimit)
            {
               WeightLimitTerrain(t, weightLimit, true);
            }
         }
      }
   }


   /// <summary>
   /// Preprocess the terrain to clamp down on the number of splat maps which have weights on each control point. First pass
   /// limits the number of weights to the specified amount per control point. Since each rendered pixel is a blend of 4 possible
   /// control points, this still means a given pixel may need up to 4 weights even if the control point is clamped to 1 weight. 
   /// In the second pass, we check all of the neighoring pixels to see if they have different weights- if they do, we clamp
   /// down to one less weight on this control point. The idea here is to create some extra headroom for the blend, but since
   /// you can still need 4 blend weights in some cases, there is no perfect solution to this issue when running with less than
   /// 4 blend weights. It does, however, greatly help when running under those constraints.
   /// 
   /// </summary>
   /// <param name="bt">Bt.</param>
   /// <param name="maxWeights">Max weights.</param>
   /// <param name="secondPass">If set to <c>true</c> second pass.</param>
   public static void WeightLimitTerrain(MicroSplatTerrain bt, int maxWeights, bool secondPass = false)
   {
      Terrain t = bt.GetComponent<Terrain>();
      if (t == null)
         return;
      TerrainData td = t.terrainData;
      if (td == null)
         return;

      int w = td.alphamapWidth;
      int h = td.alphamapHeight;
      int l = td.alphamapLayers;

      Undo.RegisterCompleteObjectUndo(t, "Weight Limit Terrain");

      var splats = td.GetAlphamaps(0, 0, w, h);
      float[] data = new float[16];
      List<WeightPair> sorted = new List<WeightPair>();
      List<int> validIndexes = new List<int>();

      for (int x = 0; x < w; ++x)
      {
         for (int y = 0; y < h; ++y)
         {
            
            // gather all weights
            for (int i = 0; i < l; ++i)
            {
               data[i] = splats[x, y, i];
            }

            sorted.Clear();
            for (int i = 0; i < 16; ++i)
            {
               var wp = new WeightPair();
               wp.index = i;
               wp.weight = data[i];
               sorted.Add(wp);
            }

            sorted.Sort((w0,w1) => w1.weight.CompareTo(w0.weight));

            // remove lower weights
            int allowedWeights = maxWeights;
            while (sorted.Count > allowedWeights)
            {
               sorted.RemoveAt(sorted.Count - 1);
            }

            // generate valid index list
            validIndexes.Clear();
            for (int i = 0; i < sorted.Count; ++i)
            {
               if (sorted[i].weight > 0)
               {
                  validIndexes.Add(sorted[i].index);
               }
            }
            // figure out if our neighbors have weights which we don't have- if so, clamp down harder to make room for blending..
            // if not, allow us to blend fully. We do this in a second pass so that small weights are reduced before we make
            // this decision..

            if (secondPass)
            {
               for (int xm = -1; xm < 2; ++xm)
               {
                  for (int ym = -1; ym < 2; ++ym)
                  {
                     int nx = x + xm;
                     int ny = y + ym;
                     if (nx >= 0 && ny >= 0 && nx < w && ny < y)
                     {
                        for (int layer = 0; layer < l; ++layer)
                        {
                           float weight = splats[nx, ny, layer];
                           if (weight > 0 && !validIndexes.Contains(layer))
                           {
                              allowedWeights = maxWeights - 1;
                           }
                        }
                     }

                  }
               }
               while (sorted.Count > allowedWeights)
               {
                  sorted.RemoveAt(sorted.Count - 1);
               }
               // generate valid index list
               validIndexes.Clear();
               for (int i = 0; i < sorted.Count; ++i)
               {
                  if (sorted[i].weight > 0)
                  {
                     validIndexes.Add(sorted[i].index);
                  }
               }
            }


            // clear non-valid indexes

            for (int j = 0; j < 16; ++j)
            {
               if (!validIndexes.Contains(j))
               {
                  data[j] = 0;
               }
            }


            // now normalize weights so that they total one on each pixel

            float total = 0;
            for (int j = 0; j < 16; ++j)
            {
               total += data[j];
            }
            float scale = 1.0f / total;
            for (int j = 0; j < 16; ++j)
            {
               data[j] *= scale;
            }


            // now map back to splat data array
            for (int i = 0; i < l; ++i)
            {
               splats[x, y, i] = data[i];
            }

         }
      }

      td.SetAlphamaps(0, 0, splats);

   }
}
#endif