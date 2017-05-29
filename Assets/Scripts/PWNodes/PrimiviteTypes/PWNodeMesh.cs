using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PW
{
	public class PWNodeMesh : PWNode {

		[PWOutput("mesh")]
		public Mesh			outputMesh;

		GameObject			meshRenderObject;
		PWGUIObjectPreview	objectPreview;

		[SerializeField]
		bool				showSceneHiddenObjects = false;

		public override void OnNodeCreate()
		{
			meshRenderObject = new GameObject("MeshNodeRenderer", typeof(MeshRenderer), typeof(MeshFilter));
			//TODO: init meshRenderer;

			UpdateHideFlags();
			externalName = "";
		}

		void UpdateHideFlags()
		{
			if (showSceneHiddenObjects)
				meshRenderObject.hideFlags = HideFlags.DontSave;
			else
				meshRenderObject.hideFlags = HideFlags.HideAndDontSave;
		}

		public override void OnNodeGUI()
		{
			EditorGUI.BeginChangeCheck();
			outputMesh = EditorGUILayout.ObjectField(outputMesh, typeof(Mesh), false) as Mesh;
			if (EditorGUI.EndChangeCheck())
				objectPreview.UpdateObjects(meshRenderObject);

			

			//TODO: draw mesh preview
		}

		public override void OnNodeDisable()
		{
			objectPreview.Cleanup();
		}

		//no process needed
	}
}
