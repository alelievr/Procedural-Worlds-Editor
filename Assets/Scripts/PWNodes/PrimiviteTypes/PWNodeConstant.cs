using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PW
{
	public class PWNodeConstant : PWNode {

		[PWOutput("int")]
		public int		outi;

		[PWOutput("float")]
		public float	outf;

		[PWOutput("Vector2")]
		public Vector2	outv2;

		[PWOutput("Vector3")]
		public Vector3	outv3;

		[PWOutput("Vector4")]
		public Vector4	outv4;

		public override void OnNodeCreate()
		{
			externalName = "Constant";
		}

		public override void OnNodeGUI()
		{
			//TODO: display mode and hide/disconnect outputs
		}

		//no process needed
	}
}
