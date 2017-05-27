using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PW
{
	public class PWNodeGameObject : PWNode {

		[PWOutput("object")]
		public GameObject		outi;

		public override void OnNodeCreate()
		{
			externalName = "GameObject";
		}

		public override void OnNodeGUI()
		{
			//TODO: GO input
		}

		//no process needed
	}
}
