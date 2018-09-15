using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Nodes;
using UnityEditor;
using ProceduralWorlds.Biomator;

namespace ProceduralWorlds.Editor
{
	[CustomEditor(typeof(NodeBiomeSurfaceMaps))]
	public class NodeBiomeSurfaceMapsEditor : BaseNodeEditor
	{
		public NodeBiomeSurfaceMaps node;

		public override void OnNodeEnable()
		{
			node = target as NodeBiomeSurfaceMaps;
		}

		public override void OnNodeGUI()
		{
			var maps = node.maps;

			EditorGUIUtility.labelWidth = 110;
			EditorGUI.BeginChangeCheck();
			node.surfaceMapsObject = EditorGUILayout.ObjectField("Surface maps", node.surfaceMapsObject, typeof(BiomeSurfaceMapsObject), false) as BiomeSurfaceMapsObject;
			
			if (node.surfaceMapsObject != null)
			{
				maps = node.surfaceMapsObject.maps;	
				EditorGUILayout.LabelField("maps: " + maps.name);
				EditorGUILayout.LabelField("Type: " + maps.type);
			}
			else
			{
				maps.type = (SurfaceMapsType)EditorGUILayout.EnumPopup("Surface complexity", maps.type);
			
				maps.name = EditorGUILayout.TextField("Name", maps.name);
	
				node.albedo = EditorGUILayout.ObjectField(node.albedo, typeof(Texture2D), false) as Texture2D;
				node.normal = EditorGUILayout.ObjectField(node.normal, typeof(Texture2D), false) as Texture2D;
				if (maps.type == SurfaceMapsType.Normal || maps.type == SurfaceMapsType.Complex)
				{
					node.opacity = EditorGUILayout.ObjectField(node.opacity, typeof(Texture2D), false) as Texture2D;
					node.smoothness = EditorGUILayout.ObjectField(node.smoothness, typeof(Texture2D), false) as Texture2D;
					node.metallic = EditorGUILayout.ObjectField(node.metallic, typeof(Texture2D), false) as Texture2D;
					node.roughness = EditorGUILayout.ObjectField(node.roughness, typeof(Texture2D), false) as Texture2D;
				}
				if (maps.type == SurfaceMapsType.Complex)
				{
					node.height = EditorGUILayout.ObjectField(node.height, typeof(Texture2D), false) as Texture2D;
					node.emissive = EditorGUILayout.ObjectField(node.emissive, typeof(Texture2D), false) as Texture2D;
					node.ambiantOcculison = EditorGUILayout.ObjectField(node.ambiantOcculison, typeof(Texture2D), false) as Texture2D;
					node.secondAlbedo = EditorGUILayout.ObjectField(node.secondAlbedo, typeof(Texture2D), false) as Texture2D;
					node.secondNormal = EditorGUILayout.ObjectField(node.secondNormal, typeof(Texture2D), false) as Texture2D;
					node.detailMask = EditorGUILayout.ObjectField(node.detailMask, typeof(Texture2D), false) as Texture2D;
					node.displacement = EditorGUILayout.ObjectField(node.displacement, typeof(Texture2D), false) as Texture2D;
				}
			}
			
			if (EditorGUI.EndChangeCheck())
				node.UpdateInputVisibilities();
		}

	}
}