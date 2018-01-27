using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using PW.Core;
using PW.Biomator;

namespace PW.Editor
{
	public class AssetHandlers
	{
	
		public static readonly string mainGraphFileName = "New ProceduralWorld";
		public static readonly string biomeGraphFileName = "New ProceduralBiome";
	
		static Dictionary< Type, Type > editorTypeTable = new Dictionary< Type, Type >()
		{
			{ typeof(PWMainGraph), typeof(PWMainGraphEditor)},
			{ typeof(PWBiomeGraph), typeof(PWBiomeGraphEditor)},
		};
	
		[OnOpenAssetAttribute(1)]
		public static bool OnOpenAssetAttribute(int instanceId, int line)
		{
			object instance = EditorUtility.InstanceIDToObject(instanceId);
	
			//if selected object is not a graph
			if (!editorTypeTable.ContainsKey(instance.GetType()))
				return false;
	
			//open Graph window:
			PWGraphEditor window = (PWGraphEditor)EditorWindow.GetWindow(editorTypeTable[instance.GetType()]);
			window.Show();
			window.LoadGraph(instance as PWGraph);
	
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
	
		[MenuItem("Assets/Create/Procedural World", false, 1)]
		public static void CreateNewProceduralWorld()
		{
			PWGraphManager.CreateMainGraph();
		}
	
		[MenuItem("Assets/Create/Procedural Biome", false, 1)]
		public static void CreateNewProceduralBiome()
		{
			PWGraphManager.CreateBiomeGraph();
		}

		[MenuItem("Assets/Create/Biome Surface Maps", false, 1)]
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