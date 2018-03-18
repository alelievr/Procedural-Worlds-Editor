using UnityEngine;
using UnityEditor.ProjectWindowCallback;
using UnityEditor;
using System.IO;
using ProceduralWorlds.Core;

namespace ProceduralWorlds.Editor
{
	public class DoCreateWorldGraph : EndNameEditAction
	{
		public override void Action(int instanceId, string pathName, string resourceFile)
		{
			var name = Path.GetFileName(pathName);
			var guid = AssetDatabase.CreateFolder(Path.GetDirectoryName(pathName), name);

			string folderPath = AssetDatabase.GUIDToAssetPath(guid);

			var folderAsset = AssetDatabase.LoadAssetAtPath(folderPath, typeof(Object));

	    	ProjectWindowUtil.ShowCreatedAsset(folderAsset);

			var graph = GraphFactory.CreateGraph< WorldGraph >(folderPath, name, false);
			
	    	ProjectWindowUtil.ShowCreatedAsset(graph);

			//create Biome folder:
			AssetDatabase.CreateFolder(folderPath, GraphFactory.baseGraphBiomeFolderName);
		}
	}
}