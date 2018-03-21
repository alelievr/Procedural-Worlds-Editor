using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Reflection;

namespace ProceduralWorlds.Editor
{
	public static class NodeScriptMenuItem
	{
		static string		nodeBaseName = "Node.cs";
	
		[MenuItem("Assets/Create/ProceduralWorlds/Node C# Script", false, 200)]
		private static void CreateNodeCSharpScritpt()
		{
			string	path = GraphFactory.GetCurrentPath() + "/" + nodeBaseName;
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
		
	}
}