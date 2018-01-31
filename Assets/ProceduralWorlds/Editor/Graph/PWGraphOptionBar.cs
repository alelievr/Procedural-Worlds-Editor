using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using PW.Core;
using UnityEditor;
using System;

namespace PW.Editor
{
	public class PWGraphOptionBar
	{
		//graph
		PWGraph				graph;


		//option bar styles:
		GUIStyle			navBarBackgroundStyle;
		
		static Texture2D	rencenterIconTexture;
		static Texture2D	fileIconTexture;
		static Texture2D	saveIconTexture;
		// static Texture2D	eyeIconTexture;

		public Action< Rect >	onDrawAdditionalOptions;

		public PWGraphOptionBar(PWGraph graph)
		{
			this.graph = graph;
		}

		public void LoadStyles()
		{
			rencenterIconTexture = Resources.Load< Texture2D >("Icons/ic_recenter");
			fileIconTexture = Resources.Load< Texture2D >("Icons/ic_file");
			saveIconTexture = Resources.Load< Texture2D >("Icons/ic_save");
			// eyeIconTexture = Resources.Load< Texture2D >("Icons/ic_eye");
			
			navBarBackgroundStyle = new GUIStyle("NavBarBackground");
		}
		
		public void DrawOptionBar(Rect graphRect)
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
					if (GUILayout.Button(rencenterIconTexture, GUILayout.Width(30), GUILayout.Height(30)))
						graph.panPosition = graphRect.center;
					
					//ping the current PW object in the project window
					if (GUILayout.Button(fileIconTexture, GUILayout.Width(30), GUILayout.Height(30)))
						EditorGUIUtility.PingObject(graph);

					if (onDrawAdditionalOptions != null)
						onDrawAdditionalOptions(optionBarRect);

					if (GUILayout.Button(saveIconTexture, GUILayout.Width(30), GUILayout.Height(30)))
						saveGraph = true;
				}
				EditorGUILayout.EndHorizontal();
		
				//remove 4 pixels for the separation bar
				graphRect.size -= Vector2.right * 4;
			}
			EditorGUILayout.EndVertical();

			Profiler.EndSample();

			if (saveGraph)
				SaveGraph(graph);
		}

		void SaveGraph(PWGraph graph)
		{
			string defaultPath = Application.dataPath + "/ProceduralWorlds/Editor/Resources/GraphPresets/";

			string path = EditorUtility.SaveFilePanel("Save " + graph.name, defaultPath, graph.name, "txt");

			if (String.IsNullOrEmpty(path))
				return ;

			graph.Export(path);

			//if the exported graph is inside the asset folder
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