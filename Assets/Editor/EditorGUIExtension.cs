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

	private static Stack<Matrix4x4> previousMatrices = new Stack<Matrix4x4>();

	public static void BeginZoomArea(float s, Rect position)
	{	
/*		GUI.EndGroup();

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

		GUI.BeginGroup(clippedArea);*/
        GUI.EndGroup();

		float zoom = 1 / s;

		Rect		clippedArea = position;
		clippedArea.position = Vector2.zero;
		Vector2		clippedAreaCenter = clippedArea.center;
		// clippedArea = position.ScaleSizeBy(s, Vector2.zero);
		Vector2 decal = clippedArea.size * 2;
		clippedArea.position -= decal;
		clippedAreaCenter += decal;
		clippedArea.size += decal * 2;
		clippedArea.y += 21;

		// EditorGUI.DrawRect(clippedArea, Color.green * .5f);

		GUI.BeginGroup(clippedArea);

		previousMatrices.Push(GUI.matrix);
		/*Matrix4x4 translation = Matrix4x4.Translate(clippedArea.center);
		Matrix4x4 scale = Matrix4x4.Scale(new Vector3(zoom, zoom, 1.0f));
		Matrix4x4 tr = Matrix4x4.Translate(position.size / 2);
		GUI.matrix = scale * tr * translation.inverse;*/

		EditorGUIUtility.ScaleAroundPivot(Vector2.one * s, clippedAreaCenter);

		Rect r2 = position;
		r2.y += 21;

		EditorGUI.DrawRect(r2, Color.green * .6f);
	}

	public static void EndZoomArea()
	{
		GUI.matrix = previousMatrices.Pop();
		GUI.EndGroup();
		GUI.BeginGroup(new Rect(0, 21, Screen.width, Screen.height));

		/*GUI.EndGroup();

		GUI.matrix = oldGUIMatrix;

		GUI.BeginGroup(oldPosition);*/
		
	}
}