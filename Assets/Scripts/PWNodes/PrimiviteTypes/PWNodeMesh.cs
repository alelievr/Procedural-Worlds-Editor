using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PW
{
	public class PWNodeMesh : PWNode {

		[PWOutput("mesh")]
		public Mesh			outputMesh;

		PWGUIObjectPreview	objectPreview;

		public override void OnNodeCreate()
		{
			externalName = "";
			//init objectPreview
		}

		public override void OnNodeGUI()
		{
			EditorGUI.BeginChangeCheck();
			outputMesh = EditorGUILayout.ObjectField(outputMesh, typeof(Mesh), false) as Mesh;
			if (EditorGUI.EndChangeCheck())
				objectPreview.UpdateObjects(outputMesh);

			

			//TODO: draw mesh preview
		}

		public override void OnNodeDisable()
		{
			objectPreview.Cleaup();
		}

		//no process needed
	}
}
