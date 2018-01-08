using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace PW.Editor
{
	public class PWNodeScriptMenuItem
	{
	
		const string	templateFile = "Assets/Editor/PWNodeTemplate.cs";
		const string	newFileBaseName = "PWNode.cs";
	
		[MenuItem("Assets/Create/PWNode C# Script", false, 3)]
		private static void CreatePWNodeCSharpScritpt()
		{
			string	path = AssetDatabase.GetAssetPath(Selection.activeObject) + "/" + newFileBaseName;
			path = AssetDatabase.GenerateUniqueAssetPath(path);
	
			File.Copy(templateFile, path);
			AssetDatabase.Refresh();
		}
	}
}