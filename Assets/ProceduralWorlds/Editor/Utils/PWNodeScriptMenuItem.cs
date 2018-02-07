using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Reflection;

namespace PW.Editor
{
	public class PWNodeScriptMenuItem
	{
	
		static string		templateFile = Application.dataPath + "/ProceduralWorlds/Editor/PWNodeTemplate.cs.txt";
		static string		newFileBaseName = "PWNode.cs";

		static MethodInfo	createScriptAsset = typeof(ProjectWindowUtil).GetMethod("CreateScriptAsset", BindingFlags.Static | BindingFlags.NonPublic);
	
		[MenuItem("Assets/Create/PWNode C# Script", false, 3)]
		private static void CreatePWNodeCSharpScritpt()
		{
			string	path = AssetDatabase.GetAssetPath(Selection.activeObject) + "/" + newFileBaseName;
			path = AssetDatabase.GenerateUniqueAssetPath(path);

			// UnityEngine.Object o = ProjectWindowUtil.CreateScriptAssetFromTemplate(pathName, resourceFile);
			// ProjectWindowUtil.ShowCreatedAsset(o);
		
			createScriptAsset.Invoke(null, new object[]{ templateFile, path });

			AssetDatabase.Refresh();
		}
	}
}