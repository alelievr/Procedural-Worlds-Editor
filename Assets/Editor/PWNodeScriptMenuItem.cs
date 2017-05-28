using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PWNodeScriptMenuItem {

	const string	templateFile = "Assets/Editor/PWNodeTemplate.cs";
	const string	newFileBaseName = "PWNode.cs";

	[MenuItem("Assets/Create/PWNode C# Script")]
	private static void CreatePWNodeCSharpScritpt()
	{
		string	path = AssetDatabase.GetAssetPath(Selection.activeObject) + "/" + newFileBaseName;
		path = AssetDatabase.GenerateUniqueAssetPath(path);
		Debug.Log(path);

		//TODO: create the file, focus name for editing and when name is validated,
		// fill the script with template file with the classname in parameter
	}

}
