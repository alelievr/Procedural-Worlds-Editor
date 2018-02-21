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
	
		static string		nodeTemplate = Application.dataPath + "/ProceduralWorlds/Editor/PWNodeTemplate.cs.txt";
		static string		nodeEditorTemplate = Application.dataPath + "/ProceduralWorlds/Editor/PWNodeEditorTemplate.cs.txt";
		static string		nodeBaseName = "PWNode.cs";
		static string		nodeEditorBaseName = "PWNodeEditor.cs";

		static MethodInfo	createScriptAsset = typeof(ProjectWindowUtil).GetMethod("CreateScriptAsset", BindingFlags.Static | BindingFlags.NonPublic);
	
		[MenuItem("Assets/Create/PWNode C# Script", false, 3)]
		private static void CreatePWNodeCSharpScritpt()
		{
			string	path = AssetDatabase.GetAssetPath(Selection.activeObject) + "/" + nodeBaseName;
			path = AssetDatabase.GenerateUniqueAssetPath(path);

			// UnityEngine.Object o = ProjectWindowUtil.CreateScriptAssetFromTemplate(pathName, resourceFile);
			// ProjectWindowUtil.ShowCreatedAsset(o);
		
			createScriptAsset.Invoke(null, new object[]{ nodeTemplate, path });

			AssetDatabase.Refresh();
		}
		
		[MenuItem("Assets/Create/PWNode C# Script", false, 4)]
		private static void CreatePWNodeEditorCSharpScritpt()
		{
			string	path = AssetDatabase.GetAssetPath(Selection.activeObject) + "/" + nodeEditorBaseName;
			path = AssetDatabase.GenerateUniqueAssetPath(path);

			// UnityEngine.Object o = ProjectWindowUtil.CreateScriptAssetFromTemplate(pathName, resourceFile);
			// ProjectWindowUtil.ShowCreatedAsset(o);
		
			createScriptAsset.Invoke(null, new object[]{ nodeEditorTemplate, path });

			AssetDatabase.Refresh();
		}
	}
}