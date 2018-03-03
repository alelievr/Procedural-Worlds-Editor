using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using PW.Core;
using UnityEditor;
using System;

namespace PW.Editor
{
	public class PWGraphOptionPanel : PWLayoutPanel
	{
		//option bar styles:
		GUIStyle			navBarBackgroundStyle;
		
		public Action< Rect >	onDrawAdditionalOptions;

		GUIContent			recenterGraphContent;
		GUIContent			locateGraphContent;
		GUIContent			saveGraphContent;
		GUIContent			resetLayoutContent;

		public override void OnLoadStyle()
		{
			Texture2D rencenterIconTexture = Resources.Load< Texture2D >("Icons/ic_recenter");
			Texture2D fileIconTexture = Resources.Load< Texture2D >("Icons/ic_file");
			Texture2D saveIconTexture = Resources.Load< Texture2D >("Icons/ic_save");
			Texture2D resetIconTexture = Resources.Load< Texture2D >("Icons/ic_reset");
			
			navBarBackgroundStyle = new GUIStyle("NavBarBackground");

			recenterGraphContent = new GUIContent(rencenterIconTexture, "Recenter graph");
			locateGraphContent = new GUIContent(fileIconTexture, "Locate graph asset file");
			saveGraphContent = new GUIContent(saveIconTexture, "Save graph as text file");
			resetLayoutContent = new GUIContent(resetIconTexture, "Reset layout");
		}
		
		public override void DrawDefault(Rect graphRect)
		{
			Event	e = Event.current;
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

	}
}