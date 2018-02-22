using UnityEngine;
using UnityEditor;
using PW.Core;

namespace PW.Node
{
	public class PWNodeGameObject : PWNode
	{

		[PWOutput("object")]
		public GameObject		outputGameObject;

		public bool					showPreview;

		public override void OnNodeCreation()
		{
			name = "GameObject";
			renamable = true;
		}

		//no process needed
	}
}
