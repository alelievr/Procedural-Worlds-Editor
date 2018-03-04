using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.ProjectWindowCallback;
using UnityEditor;
using System.IO;
using PW.Core;

namespace PW.Editor
{
	public class DoCreateMainGraph : EndNameEditAction
	{
		public override void Action(int instanceId, string pathName, string resourceFile)
		{
			var name = Path.GetFileName(pathName);
			var guid = AssetDatabase.CreateFolder(Path.GetDirectoryName(pathName), name);

			string folderPath = AssetDatabase.GUIDToAssetPath(guid);

			var folderAsset = AssetDatabase.LoadAssetAtPath(folderPath, typeof(Object));

	    	ProjectWindowUtil.ShowCreatedAsset(folderAsset);

			var graph = PWGraphFactory.CreateGraph< PWMainGraph >(folderPath, name, false);
			
	    	ProjectWindowUtil.ShowCreatedAsset(graph);

			//create Biome folder:
			AssetDatabase.CreateFolder(folderPath, PWGraphFactory.PWGraphBiomeFolderName);
		}
	}
}