using UnityEditor;
using UnityEngine;
using ProceduralWorlds.Biomator;
using System.Linq;

namespace ProceduralWorlds.Editor
{
	public class BiomeBlendListDrawer : PWDrawer
	{
		BiomeBlendList	bbList;

		public override void OnEnable()
		{
			bbList = target as BiomeBlendList;
		}

		public void OnGUI(BiomeData biomeData)
		{
			bbList.listFoldout = EditorGUILayout.Foldout(bbList.listFoldout, "Biome blending list");

			int		length = bbList.blendEnabled.GetLength(0);
			int		foldoutSize = 16;
			int		leftPadding = 10;

			base.OnGUI(new Rect());

			if (bbList.listFoldout)
			{
				float biomeSamplerNameWidth = BiomeSamplerName.GetNames().Max(n => EditorStyles.label.CalcSize(new GUIContent(n)).x);
				Rect r = GUILayoutUtility.GetRect(length * foldoutSize + biomeSamplerNameWidth, length * foldoutSize);

				using (DefaultGUISkin.Get())
				{
					GUIStyle coloredLabel = new GUIStyle(EditorStyles.label);
					for (int i = 0; i < bbList.blendEnabled.GetLength(0); i++)
					{
						Rect labelRect = r;
						labelRect.y += i * foldoutSize;
						labelRect.x += leftPadding;
						GUI.Label(labelRect, biomeData.GetBiomeKey(i), coloredLabel);

						Rect toggleRect = r;
						toggleRect.size = new Vector2(foldoutSize, foldoutSize);
						toggleRect.y += i * foldoutSize;
						toggleRect.x += leftPadding + biomeSamplerNameWidth;
						bbList.blendEnabled[i] = GUI.Toggle(toggleRect, bbList.blendEnabled[i], GUIContent.none);
					}
				}
			}
		}
	}
}