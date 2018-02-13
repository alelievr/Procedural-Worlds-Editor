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

		static bool			update = false;
		static Color[]		blackTexture;
		static PWGUIManager	PWGUI = new PWGUIManager();
	
		public static void DrawBiomeInfos(Rect view, BiomeData b)
		{
			if (b == null)
			{
				EditorGUILayout.LabelField("Null biome data");
				return ;
			}

			PWGUI.StartFrame(view);
			EditorGUILayout.LabelField("Biome datas:");

			update = GUILayout.Button("Update maps");

			//2D maps:
			foreach (var samplerDataKP in b.biomeSamplerNameMap)
			{
				if (!samplerDataKP.Value.is3D)
					PWGUI.Sampler2DPreview(samplerDataKP.Key, samplerDataKP.Value.data2D);
				//TODO: 3D maps preview
			}
		}

	}
}