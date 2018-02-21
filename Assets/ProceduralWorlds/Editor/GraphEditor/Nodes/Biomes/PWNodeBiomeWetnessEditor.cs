using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW.Node;
using UnityEditor;

namespace PW.Editor
{
	[CustomEditor(typeof(PWNodeBiomeWetness))]
	public class PWNodeBiomeWetnessEditor : PWNodeEditor
	{
		public PWNodeBiomeWetness node;

		public override void OnNodeEnable()
		{
			node = target as PWNodeBiomeWetness;
		}

		public override void OnNodeGUI()
		{

		}
	}
}