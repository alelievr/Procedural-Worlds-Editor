using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW.Core;

namespace PW.Node
{
	public class PWNodeBiomeSurfaceColor : PWNode
	{

		[PWOutput]
		public BiomeSurfaceColor	surfaceColor = new BiomeSurfaceColor();

		//Called only when the node is created (not when it is enabled/loaded)
		public override void OnNodeCreation()
		{
			name = "Surface color";
		}

		public override void OnNodeEnable()
		{
			//initialize here all unserializable datas used for GUI (like Texture2D, ...)
		}

		public override void OnNodeGUI()
		{
			PWGUI.ColorPicker("Base color", ref surfaceColor.baseColor);

			// PWGUI.ColorPicker("Color over param", ref surfaceColor.colorOverParam);
		}

		public override void OnNodeProcess()
		{
			//write here the process which take inputs, transform them and set outputs.
		}
		
	}
}