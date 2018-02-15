using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PW.Biomator
{
	[System.Serializable]
	public class BiomeBlendMatrix
	{
		public bool[,]	matrix { get; private set; }

		[SerializeField]
		bool			matrixFoldout = false;

		public void UpdateMatrixIfNeeded(BiomeData biomeData)
		{
			if (matrix != null && biomeData.length == matrix.Length)
				return ;
			
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
				SetValue(waterIndex, i, false);
		}

		public void SetValue(int i1, int i2, bool value)
		{
			matrix[i1, i2] = value;
			matrix[matrix.GetLength(0) - i2 - 1, matrix.GetLength(1) - i1 - 1] = value;
		}

		public void DrawMatrix()
		{
			matrixFoldout = EditorGUILayout.Foldout(matrixFoldout, "Biome blending matrix");

			if (matrixFoldout)
			{
				EditorGUILayout.BeginVertical();
				for (int i = 0; i < matrix.GetLength(0); i++)
				{
					EditorGUILayout.BeginHorizontal();
					for (int j = 0; j < matrix.GetLength(1) - i; j++)
					{
						bool value = EditorGUILayout.Toggle(matrix[i, j]);
						if (value != matrix[i, j])
							SetValue(i, j, value);
					}
					EditorGUILayout.EndVertical();
				}
				EditorGUILayout.EndVertical();
			}
		}
	}
}