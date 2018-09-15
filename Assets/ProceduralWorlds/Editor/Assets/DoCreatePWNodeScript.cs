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
		
		public static string nodeTemplate
		{
			get { return Application.dataPath + AssetUtils.proceduralWorldsTemplatePath + "/NodeTemplate.cs.txt"; }
		}
		
		public static string nodeEditorTemplate
		{
			get { return Application.dataPath + AssetUtils.proceduralWorldsTemplatePath + "/NodeEditorTemplate.cs.txt"; }
		}

		static MethodInfo	createScriptAsset = typeof(ProjectWindowUtil).GetMethod("CreateScriptAssetFromTemplate", BindingFlags.Static | BindingFlags.NonPublic);

		public override void Action(int instanceId, string pathName, string resourceFile)
		{
			string name = Path.GetFileNameWithoutExtension(pathName);

			//Node editor file asset
			createScriptAsset.Invoke(null, new object[]{ pathName, nodeEditorTemplate });
			File.Move(Path.GetFullPath(pathName), Application.dataPath + AssetUtils.proceduralWorldsEditorPath + "/" + name + "Editor.cs");
			
			//then node file asset
			createScriptAsset.Invoke(null, new object[]{ pathName, nodeTemplate });

			var asset = AssetDatabase.LoadAssetAtPath(pathName, typeof(MonoScript));
			ProjectWindowUtil.ShowCreatedAsset(asset);

			AssetDatabase.Refresh();
		}
	}
}