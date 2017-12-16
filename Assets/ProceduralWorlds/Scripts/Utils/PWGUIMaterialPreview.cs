using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PW
{
	public class PWGUIMaterialPreview {
	
		PWGUIMeshPreview		objectPreview;

		GameObject				previewGO;

		bool					showSceneHiddenObjects;

		public PWGUIMaterialPreview(Material mat = null)
		{
			objectPreview = new PWGUIMeshPreview(30, CameraClearFlags.Color, 2);
			previewGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			previewGO.hideFlags = HideFlags.HideAndDontSave;
			if (mat)
				previewGO.GetComponent< MeshRenderer >().material = mat;
			objectPreview.UpdateObjects(previewGO);
		}

		public void SetMaterial(Material mat)
		{
			previewGO.GetComponent< MeshRenderer >().material = mat;
		}

		public void	UpdateShowSceneHiddenObjects(bool show)
		{
			if (showSceneHiddenObjects != show)
			{
				showSceneHiddenObjects = show;
				UpdateHideFlags();
			}
		}
		
		void UpdateHideFlags()
		{
			if (previewGO == null)
				return ;
			
			if (showSceneHiddenObjects)
				previewGO.hideFlags = HideFlags.DontSave;
			else
				previewGO.hideFlags = HideFlags.HideAndDontSave;
		}

		public void Render()
		{
			objectPreview.Render();
		}

		public void Render(Rect r)
		{
			objectPreview.Render(r);
		}

		public void Cleanup()
		{
			objectPreview.Cleanup();
		}
	}
}