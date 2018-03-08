using UnityEngine;
using UnityEditor.ProjectWindowCallback;
using UnityEditor;
using System.IO;
using System.Linq;
using PW.Core;
using System.Reflection;

namespace PW.Editor
{
	public class DoCreatePWNodeScript : EndNameEditAction
	{
		string PWEditorPath = "/ProceduralWorlds/Editor/";
		
		string nodeTemplate
		{
			get { return Application.dataPath + PWEditorPath + "PWNodeTemplate.cs.txt"; }
		}
		
		string nodeEditorTemplate
		{
			get { return Application.dataPath + PWEditorPath + "PWNodeEditorTemplate.cs.txt"; }
		}

		static MethodInfo	createScriptAsset = typeof(ProjectWindowUtil).GetMethod("CreateScriptAssetFromTemplate", BindingFlags.Static | BindingFlags.NonPublic);

		void TryFindPWEditorPath()
		{
			var dirs = Directory.GetDirectories(Application.dataPath, "ProceduralWorlds/Editor", SearchOption.AllDirectories);

			foreach (var dir in dirs)
			{
				Debug.Log("PWEditor dir: " + dir);
			}

			if (dirs.Length == 0)
				PWEditorPath = null;

			PWEditorPath = dirs.First().Substring(Application.dataPath.Length + 1);
		}

		public override void Action(int instanceId, string pathName, string resourceFile)
		{
			string name = Path.GetFileNameWithoutExtension(pathName);

			if (!File.Exists(nodeTemplate))
				TryFindPWEditorPath();
			
			//Node editor file asset
			createScriptAsset.Invoke(null, new object[]{ pathName, nodeEditorTemplate });
			File.Move(Path.GetFullPath(pathName), Application.dataPath + PWEditorPath + "/" + name + "Editor.cs");
			
			//then node file asset
			createScriptAsset.Invoke(null, new object[]{ pathName, nodeTemplate });

			var asset = AssetDatabase.LoadAssetAtPath(pathName, typeof(MonoScript));
			ProjectWindowUtil.ShowCreatedAsset(asset);

			AssetDatabase.Refresh();
		}
	}
}