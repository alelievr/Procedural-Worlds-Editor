using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW.Core;

namespace PW.Node
{
	public class PWNodeColor : PWNode {

		[PWOutput]
		public Color		outputColor;
	
		public override void OnNodeCreation()
		{
			name = "Color";
		}

		//no process needed
	}
}