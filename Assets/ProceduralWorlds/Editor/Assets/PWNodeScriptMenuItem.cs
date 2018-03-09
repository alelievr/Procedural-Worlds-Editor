using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Reflection;

namespace PW.Editor
{
	public static class PWNodeScriptMenuItem
	{
		static string		nodeBaseName = "PWNode.cs";
	
		[MenuItem("Assets/Create/ProceduralWorlds/PWNode C# Script", false, 200)]
		private static void CreatePWNodeCSharpScritpt()
		{
			string	path = PWGraphFactory.GetCurrentPath() + "/" + nodeBaseName;
			path = AssetDatabase.GenerateUniqueAssetPath(path);

			ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
				0,
				ScriptableObject.CreateInstance< DoCreatePWNodeScript >(),
				path,
				EditorGUIUtility.FindTexture("cs Script Icon"),
				null
			);

			AssetDatabase.Refresh();
		}
		
	}
}