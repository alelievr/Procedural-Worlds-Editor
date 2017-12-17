using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace PW
{
	public class PWGUIMeshPreview
	{
	
		public PreviewRenderUtility	previewUtility;
		Camera						cam;

		Rect						previewRect = new Rect(0, 0, 170, 170);

		bool						isRotating = false;
		bool						isPanning = false;

		Event						e { get { return Event.current; } }
	
		public PWGUIMeshPreview(float cameraFieldOfView = 30f, CameraClearFlags clearFlags = CameraClearFlags.Color, float distance = 2.3f)
		{
			previewUtility = new PreviewRenderUtility(true);
			#if UNITY_2017
				cam = previewUtility.camera;
				previewUtility.cameraFieldOfView = cameraFieldOfView;
			#else
				cam = previewUtility.m_Camera;
				previewUtility.m_CameraFieldOfView = cameraFieldOfView;
			#endif
			cam.farClipPlane = 10000;
			cam.nearClipPlane = 0.001f;
			cam.clearFlags = clearFlags;
			cam.transform.position = (new Vector3(0, 0, -8)).normalized * distance;

			cam.transform.rotation = Quaternion.Euler(0, 0, 0);
		}
	
		void RotateCamera()
		{
			if (previewRect.Contains(e.mousePosition))
			{
				if (e.type == EventType.MouseDown)
				{
					if (e.button == 0 && !e.command)
						isRotating = true;
					if (e.button == 2 || (e.button == 0 && e.command))
						isPanning = true;
					e.Use();
				}
				if (e.type == EventType.ScrollWheel)
				{
					cam.transform.position *= 1 + (e.delta.y / 40);
					e.Use();
				}
			}
			if (e.rawType == EventType.MouseUp)
			{
				isPanning = false;
				isRotating = false;
			}
			if (isRotating && e.rawType == EventType.MouseDrag)
			{
				cam.transform.RotateAround(Vector3.zero, cam.transform.up, e.delta.x);
				cam.transform.RotateAround(Vector3.zero, cam.transform.right, e.delta.y);
				e.Use();
			}
			if (isPanning && e.rawType == EventType.MouseDrag)
			{
				Vector3 panDirection = cam.transform.right * e.delta.x - cam.transform.up * e.delta.y;
				cam.transform.position -= panDirection / 50;
				e.Use();
			}
		}
	
		public void Render(Mesh mesh, Material mat = null)
		{
			Rect	r = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true), GUILayout.Height(0));
	
			if (Event.current.type == EventType.Repaint)
			{
				r.height = previewRect.width;
				previewRect = r;
			}
			r.height = previewRect.height;
			GUILayout.Space(r.height);
			Render(r, mesh, mat);
	
			RotateCamera();
		}
	
		public void Render(Rect rect, Mesh mesh, Material mat = null)
		{
			if (e.type == EventType.Repaint)
			{
				previewUtility.BeginPreview(rect, GUIStyle.none);
				{
					previewUtility.DrawMesh(mesh, Vector3.zero, Quaternion.identity, mat, 0);
					previewUtility.Render(true, true);
				}
				previewUtility.EndAndDrawPreview(rect);
			}
		}
	
		public void Cleanup()
		{
			if (previewUtility != null)
				previewUtility.Cleanup();
		}
	}

}