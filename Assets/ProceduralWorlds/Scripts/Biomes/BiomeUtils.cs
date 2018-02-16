using System;
using System.Linq;
using System.Collections;
using System.Diagnostics;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW.Core;
using Debug = UnityEngine.Debug;

namespace PW.Biomator
{
	public static class BiomeUtils
	{

		static bool[]		samplerFoldouts;
		static PWGUIManager	PWGUI = new PWGUIManager();
	
		public static void DrawBiomeInfos(Rect view, BiomeData b)
		{
			if (b == null)
			{
				EditorGUILayout.LabelField("Null biome data");
				return ;
			}

			PWGUI.StartFrame(view);

			if (samplerFoldouts == null || samplerFoldouts.Length != b.length)
				samplerFoldouts = new bool[b.length];

			// update = GUILayout.Button("Update maps");

			//2D maps:
			int i = 0;
			foreach (var samplerDataKP in b.biomeSamplerNameMap)
			{
				if (!samplerDataKP.Value.is3D)
				{
					samplerFoldouts[i] = EditorGUILayout.Foldout(samplerFoldouts[i], samplerDataKP.Key);

					if (samplerFoldouts[i])
						PWGUI.Sampler2DPreview(samplerDataKP.Value.data2D);
				}
				//TODO: 3D maps preview

				i++;
			}
		}

	}
}