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
        GUI.EndGroup();
		
		Rect originalCenter = position;
		originalCenter.position = Vector2.zero;
		Matrix4x4 lhs = Matrix4x4.TRS(originalCenter.center, Quaternion.identity, new Vector3(s, s, 1f)) * Matrix4x4.TRS(-originalCenter.center, Quaternion.identity, Vector3.one);
		Matrix4x4 trsMatrix = lhs * GUI.matrix;

		Rect		clippedArea = position;
		clippedArea.position = Vector2.zero;
		Vector2	clippedAreaCenter = clippedArea.center;
		Vector2 decal = clippedArea.size * 2;
		clippedArea.position -= decal;
		clippedAreaCenter += decal;
		clippedArea.size += decal * 2;
		clippedArea.y += 21;

		previousMatrices.Push(GUI.matrix);

		GUI.matrix = trsMatrix;
		
		GUI.BeginGroup(clippedArea);

	}

	public static void EndZoomArea()
	{
		GUI.matrix = previousMatrices.Pop();
		GUI.EndGroup();
		GUI.BeginGroup(new Rect(0, 21, Screen.width, Screen.height));
	}
}