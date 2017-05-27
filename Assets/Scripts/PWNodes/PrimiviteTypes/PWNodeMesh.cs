using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PW
{
	public class PWNodeMesh : PWNode {

		[PWOutput("mesh")]
		public Mesh			outputMesh;

		public override void OnNodeCreate()
		{
			externalName = "";
		}

		public override void OnNodeGUI()
		{

		}

		//no process needed
	}
}
