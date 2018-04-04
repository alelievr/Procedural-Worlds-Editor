using UnityEngine;
using ProceduralWorlds.Core;

namespace ProceduralWorlds.Nodes
{
	public class NodeMaterial : BaseNode
	{

		[Output("Material")]
		public Material			outputMaterial;

		public bool				showPreview;

		public override void OnNodeCreation()
		{
			renamable = true;
			name = "Material";
		}

	}
}
