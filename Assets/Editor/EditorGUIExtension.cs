using UnityEditor;
using UnityEngine;

public static class EditorGUIExtension
{
	const float			editorWindowTabHeight = 21.0f;
    static Matrix4x4	oldGUIMatrix;

	public static Rect BeginZoomArea(float zoom, Rect screen)
	{	
		Rect clippedArea = screen.ScaleSizeBy(1.0f / zoom, screen.min);
		clippedArea.y += editorWindowTabHeight;

		oldGUIMatrix = GUI.matrix;

		EditorGUIUtility.ScaleAroundPivot(Vector2.one * zoom, screen.center);
		
		return clippedArea;
	}

	public static void EndZoomArea()
	{
		GUI.matrix = oldGUIMatrix;
	}
}