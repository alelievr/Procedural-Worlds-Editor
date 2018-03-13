using UnityEngine;
using UnityEditor.ProjectWindowCallback;
using UnityEditor;
using System.IO;
using System.Linq;
using ProceduralWorlds.Core;
using System.Reflection;

namespace ProceduralWorlds.Editor
{
	public class DoCreateNodeScript : EndNameEditAction
	{
		string ProceduralWorldsEditorPath = "/ProceduralWorlds/Editor/";
		
		string nodeTemplate
		{
			get { return Application.dataPath + ProceduralWorldsEditorPath + "NodeTemplate.cs.txt"; }
		}
		
		string nodeEditorTemplate
		{
			get { return Application.dataPath + ProceduralWorldsEditorPath + "NodeEditorTemplate.cs.txt"; }
		}

		static MethodInfo	createScriptAsset = typeof(ProjectWindowUtil).GetMethod("CreateScriptAssetFromTemplate", BindingFlags.Static | BindingFlags.NonPublic);

		void TryFindEditorPath()
		{
			var dirs = Directory.GetDirectories(Application.dataPath, "ProceduralWorlds/Editor", SearchOption.AllDirectories);

			foreach (var dir in dirs)
			{
				Debug.Log("Procedural Worlds Editor dir: " + dir);
			}

			if (dirs.Length == 0)
				ProceduralWorldsEditorPath = null;

			ProceduralWorldsEditorPath = dirs.First().Substring(Application.dataPath.Length + 1);
		}

		public override void Action(int instanceId, string pathName, string resourceFile)
		{
			string name = Path.GetFileNameWithoutExtension(pathName);

			if (!File.Exists(nodeTemplate))
				TryFindEditorPath();
			
			//Node editor file asset
			createScriptAsset.Invoke(null, new object[]{ pathName, nodeEditorTemplate });
			File.Move(Path.GetFullPath(pathName), Application.dataPath + ProceduralWorldsEditorPath + "/" + name + "Editor.cs");
			
			//then node file asset
			createScriptAsset.Invoke(null, new object[]{ pathName, nodeTemplate });

			var asset = AssetDatabase.LoadAssetAtPath(pathName, typeof(MonoScript));
			ProjectWindowUtil.ShowCreatedAsset(asset);

			AssetDatabase.Refresh();
		}
	}
}