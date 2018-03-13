using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using ProceduralWorlds.Core;
using ProceduralWorlds.Biomator;

namespace ProceduralWorlds.Editor
{
	public class AssetHandlers
	{
	
		public static readonly string worldGraphFileName = "New ProceduralWorld";
		public static readonly string biomeGraphFileName = "New ProceduralBiome";
	
		static Dictionary< Type, Type > editorTypeTable = new Dictionary< Type, Type >()
		{
			{ typeof(WorldGraph), typeof(WorldGraphEditor)},
			{ typeof(BiomeGraph), typeof(BiomeGraphEditor)},
		};
	
		[OnOpenAssetAttribute(1)]
		public static bool OnOpenAssetAttribute(int instanceId, int line)
		{
			object instance = EditorUtility.InstanceIDToObject(instanceId);
	
			//if selected object is not a graph
			if (!editorTypeTable.ContainsKey(instance.GetType()))
				return false;
	
			//open Graph window:
			BaseGraphEditor window = (BaseGraphEditor)EditorWindow.GetWindow(editorTypeTable[instance.GetType()]);
			window.Show();
			window.LoadGraph(instance as BaseGraph);
	
			return false;
		}
	
		static string	GetCurrentHierarchyPath()
		{
			string	path;
	
			if (Selection.activeObject == null)
				path = "Assets";
			else
				path = AssetDatabase.GetAssetPath(Selection.activeObject.GetInstanceID());
	
			return path;
		}
	
		[MenuItem("Assets/Create/ProceduralWorlds/Procedural World", false, -200)]
		public static void CreateNewProceduralWorld()
		{
			GraphFactory.CreateWorldGraph();
		}
	
		[MenuItem("Assets/Create/ProceduralWorlds/Procedural Biome", false, -200)]
		public static void CreateNewProceduralBiome()
		{
			GraphFactory.CreateBiomeGraph();
		}

		[MenuItem("Assets/Create/ProceduralWorlds/Biome Surface Maps", false, -1)]
		public static void CreateBiomeSurfaceMaps()
		{
			string path = GetCurrentHierarchyPath();

			path += "/New BiomeSurfaceMaps.asset";

			path = AssetDatabase.GenerateUniqueAssetPath(path);

			var biomeSurfaceMaps = ScriptableObject.CreateInstance< BiomeSurfaceMapsObject >();

			ProjectWindowUtil.CreateAsset(biomeSurfaceMaps, path);
		}
	}
}