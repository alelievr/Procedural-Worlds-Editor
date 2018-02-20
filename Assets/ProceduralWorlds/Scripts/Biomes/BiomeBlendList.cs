using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Reflection;
using System;
using PW.Core;

namespace PW.Biomator
{
	[System.Serializable]
	public class BiomeBlendList
	{
		public bool[]	blendEnabled;

		[SerializeField]
		bool			listFoldout = false;

		public void UpdateMatrixIfNeeded(BiomeData biomeData)
		{
			if (blendEnabled != null && biomeData.length == blendEnabled.Length)
				return ;
			
			blendEnabled = new bool[biomeData.length];
			
			//set all blendEnabled value to true
			for (int i = 0; i < biomeData.length; i++)
				blendEnabled[i] = true;

			int waterIndex = biomeData.GetBiomeIndex(BiomeSamplerName.waterHeight);

			if (waterIndex != -1)
				blendEnabled[waterIndex] = false;
		}

		public void DrawList(BiomeData biomeData, Rect position)
		{
			listFoldout = EditorGUILayout.Foldout(listFoldout, "Biome blending list");

			int		length = blendEnabled.GetLength(0);
			int		foldoutSize = 16;
			int		leftPadding = 10;

			if (listFoldout)
			{
				float biomeSamplerNameWidth = BiomeSamplerName.GetNames().Max(n => EditorStyles.label.CalcSize(new GUIContent(n)).x);
				Rect r = GUILayoutUtility.GetRect(length * foldoutSize + biomeSamplerNameWidth, length * foldoutSize);

				using (DefaultGUISkin.Get())
				{
					GUIStyle coloredLabel = new GUIStyle(EditorStyles.label);
					for (int i = 0; i < blendEnabled.GetLength(0); i++)
					{
						Rect labelRect = r;
						labelRect.y += i * foldoutSize;
						labelRect.x += leftPadding;
						GUI.Label(labelRect, biomeData.GetBiomeKey(i), coloredLabel);

						Rect toggleRect = r;
						toggleRect.size = new Vector2(foldoutSize, foldoutSize);
						toggleRect.y += i * foldoutSize;
						toggleRect.x += leftPadding + biomeSamplerNameWidth;
						blendEnabled[i] = GUI.Toggle(toggleRect, blendEnabled[i], GUIContent.none);
					}
				}
			}
		}
	}
}