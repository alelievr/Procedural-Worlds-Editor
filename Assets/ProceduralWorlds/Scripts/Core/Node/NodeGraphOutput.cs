using UnityEngine;

namespace ProceduralWorlds.Core
{
	[System.SerializableAttribute]
	public class NodeGraphOutput : BaseNode
	{

		//allow everything as output type
		[Input]
		public PWArray< object >	inputValues = new PWArray< object >();

		public override void OnNodeCreation()
		{
			name = "Graph output";
		}
	}
}
