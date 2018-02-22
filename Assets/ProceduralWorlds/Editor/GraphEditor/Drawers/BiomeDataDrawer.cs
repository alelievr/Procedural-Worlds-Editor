using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW.Core;
using PW.Biomator;

namespace PW.Editor
{
	public class BiomeDataDrawer : PWDrawer
	{
		bool[]		samplerFoldouts;

		BiomeData	b;

		public override void OnEnable()
		{
			b = target as BiomeData;
		}
	
		public void OnGUI(Rect view)
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