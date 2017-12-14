using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace PW
{
	public class PWGUIObjectPreview {
	
		public readonly string		GameObjectPreviewName = "__PreviewObject";

		public PreviewRenderUtility	preview;
	
		private List< GameObject >	renderObjects = new List< GameObject >();
		private bool				showSceneHiddenObjects;
		private int					previewLayer;
		private Bounds				renderBounds;
		private Rect				previewRect = new Rect(0, 0, 170, 170);
		private GameObject			firstObject = null;
		private Camera				cam;
		// private Vector3			previewCenter;
	
		public PWGUIObjectPreview(float cameraFieldOfView = 30f, CameraClearFlags clearFlags = CameraClearFlags.Skybox, float distance = 10)
		{
			var flags = BindingFlags.Static | BindingFlags.NonPublic;
			var propInfo = typeof(Camera).GetProperty("PreviewCullingLayer", flags);
			previewLayer = (int)propInfo.GetValue(null, new object[0]);
	
			preview = new PreviewRenderUtility(true);
			#if UNITY_2017
				cam = preview.camera;
				preview.cameraFieldOfView = cameraFieldOfView;
			#else
				cam = preview.m_Camera;
				preview.m_CameraFieldOfView = cameraFieldOfView;
			#endif
			cam.cullingMask = 1 << previewLayer;
			cam.farClipPlane = 10000;
			cam.nearClipPlane = 0.001f;
			cam.clearFlags = clearFlags;
			cam.transform.position = (new Vector3(0, 0, -1)).normalized * distance;

			cam.transform.rotation = Quaternion.Euler(0, 0, 0);
		}
	
		void ExpandRenderObjects(GameObject parent, List< GameObject > toAddList)
		{
			Transform parentTransform = parent.transform;
			int		nChild = parentTransform.childCount;
	
			for (int i = 0; i < nChild; i++)
			{
				var child = parentTransform.GetChild(i).gameObject;
				if (child != null)
					toAddList.Add(child);
				ExpandRenderObjects(child, toAddList);
			}
		}
	
		//all object have to be instances.
		public void UpdateObjects(params GameObject[] objs)
		{
			if (objs.Length == 0 || objs[0] == null)
				return ;
			
			renderObjects = objs.Where(o => o != null).ToList();
	
			var toAdd = new List< GameObject >();
			foreach (var ro in renderObjects)
			{
				ro.name = GameObjectPreviewName;
				ExpandRenderObjects(ro, toAdd);
			}
			
			renderObjects = renderObjects.Concat(toAdd).ToList();
		
			renderBounds = new Bounds(objs[0].transform.position, Vector3.zero);
			firstObject = objs[0];
			// previewCenter = firstObject.transform.position;
	
			foreach (var robj in renderObjects)
			{
				var obj = robj;
	
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
					cam.transform.RotateAround(firstObject.transform.position, Vector3.up, e.delta.x);
					cam.transform.RotateAround(firstObject.transform.position, cam.transform.right, e.delta.y);
					cam.transform.LookAt(firstObject.transform);
					e.Use();
				}
				if (e.type == EventType.mouseDrag && e.button == 2)
				{
					Vector3 panDirection = cam.transform.right * e.delta.x - cam.transform.up * e.delta.y;
					cam.transform.position += panDirection / 10;
					e.Use();
				}
				if (e.type == EventType.ScrollWheel)
				{
					cam.transform.position *= 1 + (e.delta.y / 10);
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
				if (obj != null)
					obj.SetActive(true);
	
			cam.Render();
	
			foreach (var obj in renderObjects)
				if (obj != null)
					obj.SetActive(false);
	
			preview.EndAndDrawPreview(rect);
		}
	
		public void Cleanup()
		{
			foreach (var obj in renderObjects)
				GameObject.DestroyImmediate(obj);
			if (preview != null)
			{
				preview.Cleanup();
			}
		}
	}

}