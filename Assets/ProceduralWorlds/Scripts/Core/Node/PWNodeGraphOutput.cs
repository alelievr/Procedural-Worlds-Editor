using UnityEditor;
using UnityEngine;

namespace PW.Core
{
	[System.SerializableAttribute]
	public class PWNodeGraphOutput : PWNode
	{

		//allow everything as output type
		[PWInput]
		public PWArray< object >	inputValues = new PWArray< object >();

		public override void OnNodeCreation()
		{
			name = "Graph output";
		}

		public override void OnNodeGUI()
		{
			PWGUI.PWArrayField(inputValues);
		}
	}
}
