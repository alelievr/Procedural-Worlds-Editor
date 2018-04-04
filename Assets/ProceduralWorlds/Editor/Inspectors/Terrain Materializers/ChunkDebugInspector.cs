using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using ProceduralWorlds.Core;

namespace ProceduralWorlds.Editor
{
	[CustomEditor(typeof(ChunkDebug)), CanEditMultipleObjects]
	public class ChunkDebugInspector : UnityEditor.Editor
	{
		List< ChunkDebug >	chunkDebugs = new List< ChunkDebug >();

		List< VisualDebugEditor >	visualDebugEditors = new List< VisualDebugEditor >();

		private void OnEnable()
		{
			if (targets != null && targets.Length != 0)
				chunkDebugs = targets.Cast< ChunkDebug >().ToList();
			else
				chunkDebugs.Add(target as ChunkDebug);
			
			for (int i = 0; i < chunkDebugs.Count; i++)
				visualDebugEditors.Add(new VisualDebugEditor(targets[i].name));
			
			SceneView.RepaintAll();
		}

		public override void OnInspectorGUI()
		{
			for (int i = 0; i < chunkDebugs.Count; i++)
			{
				var chunkDebug = chunkDebugs[i];
				var visualDebugEditor = visualDebugEditors[i];
				
				if (chunkDebug.visualDebug != null)
					visualDebugEditor.SetVisualDebugDatas(chunkDebug.visualDebug);
				
				visualDebugEditor.SetPosition(chunkDebug.transform.position);
				
				visualDebugEditor.DrawInspector();
			}
		}

		void OnSceneGUI()
		{
			foreach (var vde in visualDebugEditors)
				vde.DrawScene();
		}
	}
}