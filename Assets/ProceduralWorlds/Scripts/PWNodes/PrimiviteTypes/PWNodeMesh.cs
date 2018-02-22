using UnityEngine;
using UnityEditor;
using PW.Core;

namespace PW.Node
{
	public class PWNodeMesh : PWNode
	{

		[PWOutput("mesh")]
		public Mesh			outputMesh;

		public bool			displayPreview;

		public override void OnNodeCreation()
		{
			renamable = true;
			name = "mesh";
		}
	}
}
