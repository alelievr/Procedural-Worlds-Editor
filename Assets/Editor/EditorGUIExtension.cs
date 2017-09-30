using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;

public static class EditorGUIExtension
{
	const float			editorWindowTabHeight = 21.0f;
    static Matrix4x4	oldGUIMatrix;
	static Rect			oldPosition;

	public static void BeginZoomArea(float zoom, Rect position)
	{	
		GUI.EndGroup();

		Rect	clippedArea = position;//position.ScaleSizeBy(1f / zoom, position.size / 2);
		clippedArea.position = Vector2.zero;
		clippedArea.yMin += editorWindowTabHeight / 2;

		float	scale = 1 / zoom;

		Debug.Log("scale: " + scale);
		clippedArea.size *= 5;

		oldGUIMatrix = GUI.matrix;

		EditorGUIUtility.ScaleAroundPivot(Vector2.one * zoom, position.size / 2);
		
		Debug.Log("clippedArea: " + clippedArea + ", position: " + position);
		
		EditorGUI.DrawRect(clippedArea, Color.red);

		GUI.BeginGroup(clippedArea);
	}

	public static void EndZoomArea()
	{
		GUI.EndGroup();

		GUI.matrix = oldGUIMatrix;

		GUI.BeginGroup(oldPosition);
	}
}

	public static class GUIScaleUtility
	{
		// cache the reflected methods
		private static FieldInfo currentGUILayoutCache;
		private static FieldInfo currentTopLevelGroup;

		// Delegates to the reflected methods
		private static Func<Rect> GetTopRectDelegate;
		private static Func<Rect> topmostRectDelegate;

		// Delegate accessors
		public static Rect getTopRect { get { return (Rect)GetTopRectDelegate.Invoke (); } }
		public static Rect getTopRectScreenSpace { get { return (Rect)topmostRectDelegate.Invoke (); } }

		// Rect stack for manipulating groups
		public static List<Rect> currentRectStack { get; private set; }
		private static List<List<Rect>> rectStackGroups;

		// Matrices stack
		private static List<Matrix4x4> GUIMatrices;
		private static List<bool> adjustedGUILayout;

		private static bool isEditorWindow;

		static GUIScaleUtility()
		{
			Init();
		}

		static void Init () 
		{
			// Fetch rect acessors using Reflection
			Assembly UnityEngine = Assembly.GetAssembly (typeof (UnityEngine.GUI));
			Type GUIClipType = UnityEngine.GetType ("UnityEngine.GUIClip", true);

			PropertyInfo topmostRect = GUIClipType.GetProperty ("topmostRect", BindingFlags.Static | BindingFlags.Public);
			MethodInfo GetTopRect = GUIClipType.GetMethod ("GetTopRect", BindingFlags.Static | BindingFlags.NonPublic);

			// Create simple acessor delegates
			GetTopRectDelegate = (Func<Rect>)Delegate.CreateDelegate (typeof(Func<Rect>), GetTopRect);
			topmostRectDelegate = (Func<Rect>)Delegate.CreateDelegate (typeof(Func<Rect>), topmostRect.GetGetMethod ());

			// As we can call Begin/Ends inside another, we need to save their states hierarchial in Lists (not Stack, as we need to iterate over them!):
			currentRectStack = new List<Rect> ();
			rectStackGroups = new List<List<Rect>> ();
			GUIMatrices = new List<Matrix4x4> ();
			adjustedGUILayout = new List<bool> ();
		}

		#region Scale Area

		public static Vector2 getCurrentScale { get { return new Vector2 (1/GUI.matrix.GetColumn (0).magnitude, 1/GUI.matrix.GetColumn (1).magnitude); } } 

		public static Vector2 BeginScale (ref Rect rect, Vector2 zoomPivot, float zoom, bool IsEditorWindow, bool adjustGUILayout) 
		{
			isEditorWindow = IsEditorWindow;

			Rect screenRect;
			BeginNoClip ();
			screenRect = GUIToScaledSpace (rect);

			rect = Scale (screenRect, screenRect.position + zoomPivot, new Vector2 (zoom, zoom));

			// Now continue drawing using the new clipping group
			GUI.BeginGroup (rect);
			rect.position = Vector2.zero; // Adjust because we entered the new group

			// Because I currently found no way to actually scale to a custom pivot rather than (0, 0),
			// we'll make use of a cheat and just offset it accordingly to let it appear as if it would scroll to the center
			// Note, due to that, controls not adjusted are still scaled to (0, 0)
			Vector2 zoomPosAdjust = rect.center - screenRect.size/2 + zoomPivot;
			Debug.Log("zoomPos: " + zoomPosAdjust);

			// For GUILayout, we can make this adjustment here if desired
			adjustedGUILayout.Add (adjustGUILayout);
			if (adjustGUILayout)
			{
				GUILayout.BeginHorizontal ();
				GUILayout.Space (rect.center.x - screenRect.size.x + zoomPivot.x);
				GUILayout.BeginVertical ();
				GUILayout.Space (rect.center.y - screenRect.size.y + zoomPivot.y);
			}

			// Take a matrix backup to restore back later on
			GUIMatrices.Add (GUI.matrix);

			// Scale GUI.matrix. After that we have the correct clipping group again.
			GUIUtility.ScaleAroundPivot (new Vector2 (1/zoom, 1/zoom), zoomPosAdjust);

			return zoomPosAdjust;
		}

		public static void EndScale () 
		{
			// Set last matrix and clipping group
			if (GUIMatrices.Count == 0 || adjustedGUILayout.Count == 0)
				throw new UnityException ("GUIScaleUtility: You are ending more scale regions than you are beginning!");
			GUI.matrix = GUIMatrices[GUIMatrices.Count-1];
			GUIMatrices.RemoveAt (GUIMatrices.Count-1);

			// End GUILayout zoomPosAdjustment
			if (adjustedGUILayout[adjustedGUILayout.Count-1])
			{
				GUILayout.EndVertical ();
				GUILayout.EndHorizontal ();
			}
			adjustedGUILayout.RemoveAt (adjustedGUILayout.Count-1);

			// End the scaled group
			GUI.EndGroup ();

			GUIScaleUtility.RestoreClips ();
		}

		#endregion

		#region Clips Hierarchy

		public static void BeginNoClip () 
		{
			// Record and close all clips one by one, from bottom to top, until we hit the 'origin'
			List<Rect> rectStackGroup = new List<Rect> ();
			Rect topMostClip = getTopRect;
			while (topMostClip != new Rect (-10000, -10000, 40000, 40000)) 
			{
				rectStackGroup.Add (topMostClip);
				GUI.EndClip ();
				topMostClip = getTopRect;
			}
			// Store the clips appropriately
			rectStackGroup.Reverse ();
			rectStackGroups.Add (rectStackGroup);
			currentRectStack.AddRange (rectStackGroup);
		}

		public static void MoveClipsUp (int count) 
		{
			// Record and close all clips one by one, from bottom to top, until reached the count or hit the 'origin'
			List<Rect> rectStackGroup = new List<Rect> ();
			Rect topMostClip = getTopRect;
			while (topMostClip != new Rect (-10000, -10000, 40000, 40000) && count > 0)
			{
				rectStackGroup.Add (topMostClip);
				GUI.EndClip ();
				topMostClip = getTopRect;
				count--;
			}
			// Store the clips appropriately
			rectStackGroup.Reverse ();
			rectStackGroups.Add (rectStackGroup);
			currentRectStack.AddRange (rectStackGroup);
		}

		public static void RestoreClips () 
		{
			if (rectStackGroups.Count == 0)
			{
				Debug.LogError ("GUIClipHierarchy: BeginNoClip/MoveClipsUp - RestoreClips count not balanced!");
				return;
			}

			// Read and restore clips one by one, from top to bottom
			List<Rect> rectStackGroup = rectStackGroups[rectStackGroups.Count-1];
			for (int clipCnt = 0; clipCnt < rectStackGroup.Count; clipCnt++)
			{
				GUI.BeginClip (rectStackGroup[clipCnt]);
				currentRectStack.RemoveAt (currentRectStack.Count-1);
			}
			rectStackGroups.RemoveAt (rectStackGroups.Count-1);
		}

		#endregion

		#region Space Transformations

		public static Vector2 Scale (Vector2 pos, Vector2 pivot, Vector2 scale) 
		{
			return Vector2.Scale (pos - pivot, scale) + pivot;
		}

		public static Rect Scale (Rect rect, Vector2 pivot, Vector2 scale) 
		{
			rect.position = Vector2.Scale (rect.position - pivot, scale) + pivot;
			rect.size = Vector2.Scale (rect.size, scale);
			return rect;
		}

		public static Vector2 ScaledToGUISpace (Vector2 scaledPosition) 
		{
			if (rectStackGroups == null || rectStackGroups.Count == 0)
				return scaledPosition;
			// Iterate through the clips and substract positions
			List<Rect> rectStackGroup = rectStackGroups[rectStackGroups.Count-1];
			for (int clipCnt = 0; clipCnt < rectStackGroup.Count; clipCnt++)
				scaledPosition -= rectStackGroup[clipCnt].position;
			return scaledPosition;
		}

		public static Rect ScaledToGUISpace (Rect scaledRect) 
		{
			if (rectStackGroups == null || rectStackGroups.Count == 0)
				return scaledRect;
			scaledRect.position = ScaledToGUISpace (scaledRect.position);
			return scaledRect;
		}

		public static Vector2 GUIToScaledSpace (Vector2 guiPosition) 
		{
			if (rectStackGroups == null || rectStackGroups.Count == 0)
				return guiPosition;
			// Iterate through the clips and add positions ontop
			List<Rect> rectStackGroup = rectStackGroups[rectStackGroups.Count-1];
			for (int clipCnt = 0; clipCnt < rectStackGroup.Count; clipCnt++)
				guiPosition += rectStackGroup[clipCnt].position;
			return guiPosition;
		}

		public static Rect GUIToScaledSpace (Rect guiRect) 
		{
			if (rectStackGroups == null || rectStackGroups.Count == 0)
				return guiRect;
			guiRect.position = GUIToScaledSpace (guiRect.position);
			return guiRect;
		}

		public static Vector2 GUIToScreenSpace (Vector2 guiPosition) 
		{
			#if UNITY_EDITOR
			if (isEditorWindow)
				return guiPosition + getTopRectScreenSpace.position - new Vector2 (0, 22);
			#endif
			return guiPosition + getTopRectScreenSpace.position;
		}

		public static Rect GUIToScreenSpace (Rect guiRect) 
		{
			guiRect.position += getTopRectScreenSpace.position;
			#if UNITY_EDITOR
			if (isEditorWindow)
				guiRect.y -= 22;
			#endif
			return guiRect;
		}

		#endregion
	}