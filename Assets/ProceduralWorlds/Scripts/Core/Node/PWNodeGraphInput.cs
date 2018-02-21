using UnityEngine;
using UnityEditor;

namespace PW.Core
{
	[System.SerializableAttribute]
	public class PWNodeGraphInput : PWNode
	{

		[PWOutput]
		public PWArray< object >	outputValues = new PWArray< object >();
		
		public override void OnNodeCreation()
		{
			name = "Graph input";
		}

		//no need to process this graph, datas are assigned form PWNodeGraphExternal
	}
}
