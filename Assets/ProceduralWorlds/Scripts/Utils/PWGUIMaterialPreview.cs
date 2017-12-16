using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PW
{
	public class PWGUIMaterialPreview
	{
	
		PWGUIMeshPreview		objectPreview;

		Mesh					previewMesh;
		Material				previewMaterial;

		bool					showSceneHiddenObjects;

		public PWGUIMaterialPreview(Material mat = null)
		{
			objectPreview = new PWGUIMeshPreview(30, CameraClearFlags.Color, 2);

			//getting access to the primitive sphere mesh
			//TODO: possibility to choose between different meshes (and meshes in ressources ?)
			GameObject tmp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			previewMesh = tmp.GetComponent< MeshFilter >().sharedMesh;
			GameObject.DestroyImmediate(tmp);
		}

		public void SetMaterial(Material mat)
		{
			previewMaterial = mat;
		}

		public void Render()
		{
			objectPreview.Render(previewMesh, previewMaterial);
		}

		public void Render(Rect r)
		{
			objectPreview.Render(r, previewMesh, previewMaterial);
		}

		public void Cleanup()
		{
			objectPreview.Cleanup();
		}
	}
}