using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using ProceduralWorlds.Core;
using System;

using Object = UnityEngine.Object;

namespace ProceduralWorlds.Editor
{
	public partial class BaseGraphEditor
	{
		Vector2				notFoundScrollBar;
		[System.NonSerialized]
		bool				graphListLoaded;
		[System.NonSerialized]
		List< Object >		openableGraphList = new List< Object >();
	
		void RenderGraphNotFound()
		{
			
			EditorGUILayout.LabelField("TODO: better GUI and new graph button. If you want to create a new graph, use the create asset menu");
			
			EditorGUILayout.LabelField("Graph not found, double click on a graph asset file to a graph to open it");

			if (expectedGraphType == null)
				return ;

			if (!graphListLoaded)
				LoadGraphList();
			BaseGraph newGraph = EditorGUILayout.ObjectField(null, expectedGraphType, false) as BaseGraph;
	
			if (newGraph != null)
				LoadGraph(newGraph);

			notFoundScrollBar = EditorGUILayout.BeginScrollView(notFoundScrollBar);
			{
				foreach (var obj in openableGraphList)
				{
					string path = AssetDatabase.GetAssetPath(obj);

					if (String.IsNullOrEmpty(path))
						continue ;
					
					if (GUILayout.Button("Open " + obj + "(" + path + ")"))
						LoadGraph(obj as BaseGraph);
				}
			}
			EditorGUILayout.EndScrollView();
		}

		void LoadGraphList()
		{
			if (expectedGraphType != null)
			{
				openableGraphList = Resources.FindObjectsOfTypeAll(expectedGraphType).ToList();
				string[] assetGUIDs = AssetDatabase.FindAssets("t:" + expectedGraphType);

				openableGraphList = new List< Object >();
				
				foreach (var assetGUID in assetGUIDs)
				{
					string path = AssetDatabase.GUIDToAssetPath(assetGUID);
					var asset = AssetDatabase.LoadAssetAtPath(path, expectedGraphType);

					if (asset != null)
						openableGraphList.Add(asset);
				}
			}
		}

	}
}