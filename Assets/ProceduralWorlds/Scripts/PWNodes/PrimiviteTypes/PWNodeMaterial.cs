using UnityEngine;
using PW.Core;

namespace PW.Node
{
	public class PWNodeMaterial : PWNode
	{

		[PWOutput("Material")]
		public Material			outputMaterial;

		public bool				showPreview;

		public override void OnNodeCreation()
		{
			renamable = true;
			name = "Material";
		}

	}
}
