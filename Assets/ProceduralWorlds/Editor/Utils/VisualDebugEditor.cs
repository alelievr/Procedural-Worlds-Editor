using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ProceduralWorlds.Core;
using System;

namespace ProceduralWorlds.Editor
{
	public class VisualDebugEditor
	{
		int		currentStep;
		int		currentFrame;

		string	name;

		Vector3	position;

		VisualDebug	visualDebug;

		readonly Dictionary< Type, Action< VisualDebug.View, Vector3 > > viewDrawers = new Dictionary< Type, Action< VisualDebug.View, Vector3 > >
		{
			{typeof(VisualDebug.LabelView), DrawLabel},
			{typeof(VisualDebug.LineView), DrawLine},
			{typeof(VisualDebug.PointView), DrawPoint},
			{typeof(VisualDebug.TriangleView), DrawTriangle},
		};

		public VisualDebugEditor(string name)
		{
			this.name = name;
		}

		public void SetVisualDebugDatas(VisualDebug vd)
		{
			visualDebug = vd;
		}

		public void DrawInspector()
		{
			if (visualDebug == null)
				return ;
			
			EditorGUI.indentLevel++;
			EditorGUILayout.BeginVertical(Styles.box);
			{
				EditorGUILayout.LabelField(name);

				EditorGUI.BeginChangeCheck();
				{
					currentFrame = EditorGUILayout.IntSlider("Frame", currentFrame, 0, visualDebug.frames.Count - 1);

					var frames = visualDebug.frames;

					EditorGUILayout.BeginHorizontal();
					if (GUILayout.Button("First"))
						currentFrame = 0;
					if (GUILayout.Button("-10"))
						currentFrame = Mathf.Max(0, currentFrame - 10);
					if (GUILayout.Button("-1"))
						currentFrame = Mathf.Max(0, currentFrame - 1);
					if (GUILayout.Button("+1"))
						currentFrame = Mathf.Min(frames.Count - 1, currentFrame + 1);
					if (GUILayout.Button("+10"))
						currentFrame = Mathf.Min(frames.Count - 1, currentFrame + 10);
					if (GUILayout.Button("Last"))
						currentFrame = frames.Count - 1;
					EditorGUILayout.EndHorizontal();
					EditorGUILayout.LabelField("Current frame: " + frames[currentFrame].name);
					currentStep = EditorGUILayout.IntSlider(currentStep, 0, frames[currentFrame].infos.Count);
				}
				if (EditorGUI.EndChangeCheck())
					SceneView.RepaintAll();
			}
			EditorGUILayout.EndVertical();
			EditorGUI.indentLevel--;
		}

		public void DrawScene()
		{
			if (visualDebug == null)
				return ;
			
			if (visualDebug.frames.Count == 0 || !visualDebug.enabled)
				return ;
			
			int f = 0;

			//draw the initial state
			if (currentFrame != 0)
				foreach (var view in visualDebug.frames[0].infos)
					viewDrawers[view.GetType()](view, position);

			foreach (var view in visualDebug.frames[currentFrame].infos)
			{
				if (currentStep != 0 && f >= currentStep)
					break ;

				Handles.color = view.color;
				viewDrawers[view.GetType()](view, position);
				f++;
			}
		}

		public void SetPosition(Vector3 position)
		{
			this.position = position;
		}

		static void DrawLabel(VisualDebug.View view, Vector3 position)
		{
			var labelView = view as VisualDebug.LabelView;

			var style = labelView.style;
			if (style == null)
				style = EditorStyles.label;
			
			Handles.Label(labelView.position + position, labelView.text, style);
		}
		
		static void DrawLine(VisualDebug.View view, Vector3 position)
		{
			var lineView = view as VisualDebug.LineView;

			Handles.DrawLine(lineView.p1 + position, lineView.p2 + position);
		}

		static void DrawPoint(VisualDebug.View view, Vector3 position)
		{
			var pointView = view as VisualDebug.PointView;

			Handles.SphereHandleCap(0, pointView.position + position, Quaternion.identity, pointView.size, EventType.Repaint);
		}
		
		static void DrawTriangle(VisualDebug.View view, Vector3 position)
		{
			var triangleView = view as VisualDebug.TriangleView;

			Handles.DrawAAConvexPolygon(triangleView.p1 + position, triangleView.p2 + position, triangleView.p3 + position);
		}
	}
}