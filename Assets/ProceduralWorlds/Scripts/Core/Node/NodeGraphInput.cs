using UnityEngine;

namespace ProceduralWorlds.Core
{
	[System.SerializableAttribute]
	public class NodeGraphInput : BaseNode
	{

		[Output]
		public PWArray< object >	outputValues = new PWArray< object >();
		
		public override void OnNodeCreation()
		{
			name = "Graph input";
		}

		//no need to process this graph, datas are assigned form NodeGraphExternal
	}
}
