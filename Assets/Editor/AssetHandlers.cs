using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using PW.Core;

public class AssetHandlers {

	public static readonly string mainGraphFileName = "New ProceduralWorld";
	public static readonly string biomeGraphFileName = "New ProceduralBiome";

    [OnOpenAssetAttribute(1)]
	public static bool OnOpenAssetAttribute(int instanceId, int line)
	{
		Object instance = EditorUtility.InstanceIDToObject(instanceId);

		if (instance.GetType() == typeof(PWMainGraph))
		{
			//open PWNodeGraph window:
			PWMainGraphEditor window = (PWMainGraphEditor)EditorWindow.GetWindow(typeof(PWMainGraphEditor));
			window.graph = instance as PWGraph;
		}
		if (instance.GetType() == typeof(PWBiomeGraph))
		{
			//TODO: PWBiomeGraph editor
			// PWBiomeGraphEditor
		}
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
}
