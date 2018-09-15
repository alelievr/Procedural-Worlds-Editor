using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Reflection;

namespace ProceduralWorlds.Editor
{
	public static class ScriptCreationMenuItems
	{
		static string		nodeBaseName = "Node.cs";
		static string		terrainFileName = "Terrain.cs";
		static string		terrainTemplateFile
		{
			get { return Application.dataPath + AssetUtils.proceduralWorldsTemplatePath + "/TerrainTemplate.cs.txt"; }
		}
		
		static MethodInfo	createScriptAsset = typeof(ProjectWindowUtil).GetMethod("CreateScriptAsset", BindingFlags.Static | BindingFlags.NonPublic);

		[MenuItem("Assets/Create/ProceduralWorlds/Node C# Script", false, 200)]
		private static void CreateNodeCSharpScritpt()
		{
			string	path = AssetUtils.GetCurrentPath() + "/" + nodeBaseName;
			path = AssetDatabase.GenerateUniqueAssetPath(path);

			ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
				0,
				ScriptableObject.CreateInstance< DoCreateNodeScript >(),
				path,
				EditorGUIUtility.FindTexture("cs Script Icon"),
				null
			);

			AssetDatabase.Refresh();
		}

		[MenuItem("Assets/Create/ProceduralWorlds/Terrain C# Script")]
		private static void CreateTerrainCSharpScript()
		{
			string path = AssetUtils.GetCurrentPath();
			path = AssetDatabase.GenerateUniqueAssetPath(path + "/" + terrainFileName);

			createScriptAsset.Invoke(null, new object[] { terrainTemplateFile, path });
		}
	}
}