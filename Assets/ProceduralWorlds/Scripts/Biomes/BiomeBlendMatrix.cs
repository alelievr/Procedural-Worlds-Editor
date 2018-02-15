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
	public class BiomeBlendMatrix : ISerializationCallbackReceiver
	{
		public bool[,]	matrix { get; private set; }

		[SerializeField]
		bool[]			serializedMatrix;

		[SerializeField]
		bool			matrixFoldout = false;

		public Color[]	biomeSamplerColors;

		public void UpdateMatrixIfNeeded(BiomeData biomeData)
		{
			if (matrix != null && biomeData.length == matrix.GetLength(0))
				return ;
			
			//Fill color array for DrawMatrix
			biomeSamplerColors = new Color[biomeData.length];
			int colorScheneNameLength = Enum.GetValues(typeof(PWColorSchemeName)).Length;
			for (int i = 0; i < biomeData.length; i++)
			{
				var colorScheme = (PWColorSchemeName)((i * 6) % (colorScheneNameLength - 1) + 1);
				biomeSamplerColors[i] = PWColorTheme.GetNodeColor(colorScheme);
			}
			
			matrix = new bool[biomeData.length, biomeData.length];
			
			//set all matrix value to true
			for (int i = 0; i < biomeData.length; i++)
				for (int j = 0; j < biomeData.length; j++)
					SetValue(i, j, true);

			int waterIndex = biomeData.GetBiomeIndex(BiomeSamplerName.waterHeight);

			if (waterIndex == -1)
				return ;
			
			//set all biome blend with water to false
			for (int i = 0; i < biomeData.length; i++)
				SetValue(i, waterIndex, false);
		}

		public void SetValue(int i1, int i2, bool value)
		{
			matrix[i1, i2] = value;
			matrix[matrix.GetLength(0) - i2 - 1, matrix.GetLength(1) - i1 - 1] = value;
		}

		public void DrawMatrix(BiomeData biomeData, Rect position)
		{
			matrixFoldout = EditorGUILayout.Foldout(matrixFoldout, "Biome blending matrix");

			int		length = matrix.GetLength(0);
			int		foldoutSize = 16;

			if (matrixFoldout)
			{
				float biomeSamplerNameWidth = BiomeSamplerName.GetNames().Max(n => EditorStyles.label.CalcSize(new GUIContent(n)).x);
				Rect r = GUILayoutUtility.GetRect(length * foldoutSize + biomeSamplerNameWidth, length * foldoutSize);

				using (new DefaultGUISkin())
				{
					GUIStyle coloredLabel = new GUIStyle(EditorStyles.label);
					for (int i = 0; i < matrix.GetLength(0); i++)
					{
						Rect labelRect = r;
						labelRect.y += i * foldoutSize;
						PWStyles.ColorizeText(coloredLabel, biomeSamplerColors[i]);
						GUI.color = Color.white;
						GUI.Label(labelRect, biomeData.GetBiomeKey(i), coloredLabel);
						for (int j = 0; j < matrix.GetLength(1) - i; j++)
						{
							Rect toggleRect = r;
							toggleRect.size = new Vector2(foldoutSize, foldoutSize);
							toggleRect.x += biomeSamplerNameWidth + i * foldoutSize;
							toggleRect.y += j * foldoutSize;
							GUI.color = biomeSamplerColors[matrix.GetLength(0) - i - 1];
							bool value = GUI.Toggle(toggleRect, matrix[i, j], GUIContent.none);
							if (value != matrix[i, j])
								SetValue(i, j, value);
						}
					}
	
					GUI.color = Color.white;
				}
			}
		}

		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
			if (matrix == null)
				return ;
			
			serializedMatrix = new bool[matrix.Length];

			for (int x = 0; x < matrix.GetLength(0); x++)
				for (int y = 0; y < matrix.GetLength(1); y++)
					serializedMatrix[x + y * matrix.GetLength(0)] = matrix[x, y];
		}

		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			if (serializedMatrix == null)
				return ;
			
			int length = (int)Mathf.Sqrt(serializedMatrix.Length);
			matrix = new bool[length, length];

			for (int x = 0; x < matrix.GetLength(0); x++)
				for (int y = 0; y < matrix.GetLength(1); y++)
					matrix[x, y] = serializedMatrix[x + y *  length];
		}
	}
}