using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using ProceduralWorlds.Core;
using UnityEditor;
using System;
using System.Reflection;

namespace ProceduralWorlds.Editor
{
	public class BaseGraphOptionPanel : LayoutPanel
	{
		//option bar styles:
		GUIStyle			navBarBackgroundStyle;
		
		public Action< Rect >	onDrawAdditionalOptions;

		GUIContent			recenterGraphContent;
		GUIContent			locateGraphContent;
		GUIContent			saveGraphContent;
		GUIContent			resetLayoutContent;
		GUIContent			bugReportContent;
		GUIContent			tryFixContent;

		public override void OnLoadStyle()
		{
			Texture2D rencenterIconTexture = Resources.Load< Texture2D >("Icons/ic_recenter");
			Texture2D fileIconTexture = Resources.Load< Texture2D >("Icons/ic_file");
			Texture2D saveIconTexture = Resources.Load< Texture2D >("Icons/ic_save");
			Texture2D resetIconTexture = Resources.Load< Texture2D >("Icons/ic_reset");
			Texture2D bugReportTexture = Resources.Load< Texture2D >("Icons/ic_bug_report");
			Texture2D tryFixTexture = Resources.Load< Texture2D >("Icons/ic_fix");
			
			navBarBackgroundStyle = new GUIStyle("NavBarBackground");

			recenterGraphContent = new GUIContent(rencenterIconTexture, "Recenter graph");
			locateGraphContent = new GUIContent(fileIconTexture, "Locate graph asset file");
			saveGraphContent = new GUIContent(saveIconTexture, "Save graph as text file");
			resetLayoutContent = new GUIContent(resetIconTexture, "Reset layout");
			bugReportContent = new GUIContent(bugReportTexture, "Report a bug");
			tryFixContent = new GUIContent(tryFixTexture, "Try to fix anchor and link issues (will remove all links)");
		}
		
		public override void DrawDefault(Rect graphRect)
		{
			bool 	saveGraph = false;

			Profiler.BeginSample("[PW] Rendering option bar");

			EditorGUILayout.BeginVertical(navBarBackgroundStyle);
			{
				//Icon bar:
				Rect optionBarRect = EditorGUILayout.BeginHorizontal(navBarBackgroundStyle, GUILayout.MaxHeight(40), GUILayout.ExpandWidth(true));
				{
					//recenter the graph
					if (GUILayout.Button(recenterGraphContent, GUILayout.Width(30), GUILayout.Height(30)))
						graphRef.panPosition = graphRect.center;
					
					//ping the current PW object in the project window
					if (GUILayout.Button(locateGraphContent, GUILayout.Width(30), GUILayout.Height(30)))
						EditorGUIUtility.PingObject(graphRef);

					if (onDrawAdditionalOptions != null)
						onDrawAdditionalOptions(optionBarRect);

					if (GUILayout.Button(saveGraphContent, GUILayout.Width(30), GUILayout.Height(30)))
						saveGraph = true;

					if (GUILayout.Button(resetLayoutContent, GUILayout.Width(30), GUILayout.Height(30)))
						graphEditor.ResetLayout();
					
					GUILayout.FlexibleSpace();

					if (GUILayout.Button(bugReportContent, Styles.yellowButton, GUILayout.Width(30), GUILayout.Height(30)))
						Application.OpenURL("https://github.com/alelievr/Procedural-Worlds-Editor/issues/new");
					
					if (GUILayout.Button(tryFixContent, Styles.redButton, GUILayout.Width(30), GUILayout.Height(30)))
						TryFix();
				}
				EditorGUILayout.EndHorizontal();
		
				//remove 4 pixels for the separation bar
				graphRect.size -= Vector2.right * 4;
			}
			EditorGUILayout.EndVertical();

			Profiler.EndSample();

			if (saveGraph)
				SaveGraph();
		}

		void SaveGraph()
		{
			string defaultPath = Application.dataPath + "/ProceduralWorlds/Editor/Resources/GraphPresets/";

			string path = EditorUtility.SaveFilePanel("Save " + graphRef.name, defaultPath, graphRef.name, "txt");

			if (String.IsNullOrEmpty(path))
				return ;

			graphRef.Export(path);

			//if the exported graphRef is inside the asset folder
			if (path.Contains(Application.dataPath))
			{
				string relativePath = "Assets/" + path.Substring(Application.dataPath.Length + 1);
	
				AssetDatabase.Refresh();
	
				var graphTextAsset = AssetDatabase.LoadAssetAtPath(relativePath, typeof(TextAsset));
	
				ProjectWindowUtil.ShowCreatedAsset(graphTextAsset);
			}
		}

		void TryFix()
		{
			foreach (var node in graphRef.allNodes)
			{
				var loadFields = typeof(BaseNode).GetMethod("LoadFieldAttributes", BindingFlags.NonPublic | BindingFlags.Instance);
				var clearDupKeys = typeof(BaseNode).GetMethod("RemoveAnchorFieldDulicatedKeys", BindingFlags.NonPublic | BindingFlags.Instance);
				var updateAnchors = typeof(BaseNode).GetMethod("UpdateAnchorProperties", BindingFlags.NonPublic | BindingFlags.Instance);

				if (loadFields != null && clearDupKeys != null && updateAnchors != null)
				{
					clearDupKeys.Invoke(node, new object[]{});
					node.anchorFieldDictionary.Clear();
					loadFields.Invoke(node, new object[]{});
					node.inputAnchorFields.Clear();
					node.outputAnchorFields.Clear();
					updateAnchors.Invoke(node, new object[]{});
				}
				else
					Debug.Log("Ho no ...");

			}
		}

	}
}