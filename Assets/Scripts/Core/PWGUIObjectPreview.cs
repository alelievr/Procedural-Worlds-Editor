using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;

public class PWGUIObjectPreview {

	public PreviewRenderUtility		preview;

	private GameObject[]			renderObjects;
	private bool					showSceneHiddenObjects;

	public void Initialize(float cameraFieldOfView = 30f)
    {
        var flags = BindingFlags.Static | BindingFlags.NonPublic;
        var propInfo = typeof(Camera).GetProperty("PreviewCullingLayer", flags);
        int previewLayer = (int)propInfo.GetValue(null, new object[0]);

        preview = new PreviewRenderUtility(true);
		preview.m_CameraFieldOfView = cameraFieldOfView;
		preview.m_Camera.cullingMask = 1 << previewLayer;
	}

	public void UpdateObjects(params GameObject[] objs)
	{
		renderObjects = objs;
	}

	public void UpdateCamera(Transform transform)
	{

	}

	public void Render()
	{
		Render(EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true)));
	}

	public void Render(Rect rect)
	{
		preview.BeginPreview(rect, GUIStyle.none);

		foreach (var obj in renderObjects)
				obj.SetActive(true);

		preview.m_Camera.Render();

		foreach (var obj in renderObjects)
			if (obj as GameObject != null)
				obj.SetActive(false);

		preview.EndAndDrawPreview(rect);
	}

	public void Cleanup()
	{
		preview.Cleanup();
	}

}
