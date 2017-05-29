using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;

public class PWGUIObjectPreview {

	public PreviewRenderUtility		preview;

	private GameObject[]			renderObjects = new GameObject[0];
	private bool					showSceneHiddenObjects;
	private int						previewLayer;
	private Bounds					renderBounds;
	private Rect					previewRect = new Rect(0, 0, 170, 170);
	private GameObject				firstObject = null;
	private Vector3					previewCenter;

	public void Initialize(float cameraFieldOfView = 30f)
    {
        var flags = BindingFlags.Static | BindingFlags.NonPublic;
        var propInfo = typeof(Camera).GetProperty("PreviewCullingLayer", flags);
        previewLayer = (int)propInfo.GetValue(null, new object[0]);

        preview = new PreviewRenderUtility(true);
		preview.m_CameraFieldOfView = cameraFieldOfView;
		preview.m_Camera.cullingMask = 1 << previewLayer;
		preview.m_Camera.farClipPlane = 10000;
		preview.m_Camera.clearFlags = CameraClearFlags.Skybox;
		preview.m_Camera.transform.position = new Vector3(4, 6, -4);
		preview.m_Camera.transform.rotation = Quaternion.Euler(45, -45, 0);
	}

	public void UpdateObjects(params GameObject[] objs)
	{
		if (objs.Length == 0)
			return ;
		
		renderObjects = objs;
		
		renderBounds = new Bounds(objs[0].transform.position, Vector3.zero);
		firstObject = objs[0];
		previewCenter = firstObject.transform.position;

		foreach (var obj in renderObjects)
		{
			obj.layer = previewLayer;
			
			var meshRenderer = obj.GetComponent< MeshRenderer >();

			if (meshRenderer != null)
				renderBounds.Encapsulate(meshRenderer.bounds);
		}
	}

	void RotateCamera()
	{
		var e = Event.current;
		
		if (previewRect.Contains(e.mousePosition))
		{
			if (e.type == EventType.mouseDrag && firstObject != null && e.button == 0)
			{
				preview.m_Camera.transform.RotateAround(firstObject.transform.position, Vector3.up, e.delta.x);
				preview.m_Camera.transform.RotateAround(firstObject.transform.position, Vector3.forward, e.delta.y);
				preview.m_Camera.transform.LookAt(firstObject.transform);
				e.Use();
			}
			if (e.type == EventType.mouseDrag && e.button == 2)
			{
				Vector3 panDirection = preview.m_Camera.transform.right * e.delta.x - preview.m_Camera.transform.up * e.delta.y;
				preview.m_Camera.transform.position += panDirection / 10;
				e.Use();
			}
			if (e.type == EventType.ScrollWheel)
			{
				preview.m_Camera.transform.position *= 1 + (e.delta.y / 10);
				Debug.Log("delta: " + (1 + (e.delta.y / 10)));
				e.Use();
			}
		}
	}

	public void Render()
	{
		Rect	r = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true), GUILayout.Height(0));

		if (Event.current.type == EventType.Repaint)
		{
			r.height = previewRect.width;
			previewRect = r;
		}
		r.height = previewRect.height;
		GUILayout.Space(r.height);
		Render(r);

		RotateCamera();
	}

	public void Render(Rect rect)
	{
		preview.BeginPreview(rect, GUIStyle.none);

		foreach (var obj in renderObjects)
			obj.SetActive(true);

		preview.m_Camera.Render();

		foreach (var obj in renderObjects)
			obj.SetActive(false);

		preview.EndAndDrawPreview(rect);
	}

	public void Cleanup()
	{
		if (preview != null)
		{
			preview.Cleanup();
		}
	}

}
