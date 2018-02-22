using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW.Node;
using UnityEditor;

namespace PW.Editor
{
	[CustomEditor(typeof(PWNodeBiomeSurfaceMaps))]
	public class PWNodeBiomeSurfaceMaterialEditor : PWNodeEditor
	{
		public PWNodeBiomeSurfaceMaterial node;

		public override void OnNodeEnable()
		{
			node = target as PWNodeBiomeSurfaceMaterial;
		}

		public override void OnNodeGUI()
		{
			node.surfaceMaterial.material = EditorGUILayout.ObjectField(node.surfaceMaterial.material, typeof(Material), false) as Material;
		}

	}
}