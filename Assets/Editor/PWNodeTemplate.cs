using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PW
{
	public class PWNodeTemplate : PWNode {
	
		public override void OnNodeCreate()
		{
			externalName = "your node name";
			//initialize here all unserializable datas used for GUI (like Texture2D, ...)
		}

		public override void OnNodeGUI()
		{
			//your node GUI
		}

		public override void OnNodeProcess()
		{
			//write here the process which take input, transform them and set output variables.
		}
		
	}
}