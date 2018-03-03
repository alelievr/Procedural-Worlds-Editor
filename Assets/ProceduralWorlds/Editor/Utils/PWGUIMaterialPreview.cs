using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PW.Editor
{
	public class PWGUIMaterialPreview
	{
	
		readonly PWGUIMeshPreview	objectPreview;

		readonly Mesh				previewMesh;

		bool						showSceneHiddenObjects;

		public PWGUIMaterialPreview(PrimitiveType previewPrimitive = PrimitiveType.Sphere)
		{
			objectPreview = new PWGUIMeshPreview();

			GameObject tmp = GameObject.CreatePrimitive(previewPrimitive);
			previewMesh = tmp.GetComponent< MeshFilter >().sharedMesh;
			GameObject.DestroyImmediate(tmp);
		}

		public void Render(Material mat)
		{
			objectPreview.Render(previewMesh, mat);
		}

		public void Render(Rect r, Material mat)
		{
			objectPreview.Render(r, previewMesh, mat);
		}

		public void Cleanup()
		{
			objectPreview.Cleanup();
		}
	}
}