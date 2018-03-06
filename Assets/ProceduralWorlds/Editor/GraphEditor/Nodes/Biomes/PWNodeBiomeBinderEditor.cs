using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW.Node;
using UnityEditor;

namespace PW.Editor
{
	[CustomEditor(typeof(PWNodeBiomeBinder))]
	public class PWNodeBiomeBinderEditor : PWNodeEditor
	{
		public override void OnNodeGUI()
		{
			//nothing to draw here, maybe this node will be merged with the biome input node
		}
	}
}