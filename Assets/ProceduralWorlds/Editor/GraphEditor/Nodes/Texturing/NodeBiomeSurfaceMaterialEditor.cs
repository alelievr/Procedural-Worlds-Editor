using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Node;
using UnityEditor;

namespace ProceduralWorlds.Editor
{
	[CustomEditor(typeof(NodeBiomeSurfaceMaps))]
	public class NodeBiomeSurfaceMaterialEditor : BaseNodeEditor
	{
		public NodeBiomeSurfaceMaterial node;

		public override void OnNodeEnable()
		{
			node = target as NodeBiomeSurfaceMaterial;
		}

		public override void OnNodeGUI()
		{
			node.surfaceMaterial.material = EditorGUILayout.ObjectField(node.surfaceMaterial.material, typeof(Material), false) as Material;
		}

	}
}