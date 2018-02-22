using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW.Core;
using PW.Biomator;

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
		
	}
}