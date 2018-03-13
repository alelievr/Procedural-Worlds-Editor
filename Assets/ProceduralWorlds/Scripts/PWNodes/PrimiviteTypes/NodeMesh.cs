using UnityEngine;
using ProceduralWorlds.Core;

namespace ProceduralWorlds.Node
{
	public class NodeMesh : BaseNode
	{

		[Output("mesh")]
		public Mesh			outputMesh;

		public bool			displayPreview;

		public override void OnNodeCreation()
		{
			renamable = true;
			name = "mesh";
		}
	}
}
