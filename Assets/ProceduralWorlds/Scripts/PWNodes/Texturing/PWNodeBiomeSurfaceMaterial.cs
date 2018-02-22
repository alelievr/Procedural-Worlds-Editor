using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW.Core;
using PW.Biomator;

namespace PW.Node
{
	public class PWNodeBiomeSurfaceMaterial : PWNode
	{

		[PWOutput]
		public BiomeSurfaceMaterial		surfaceMaterial = new BiomeSurfaceMaterial();

		public override void OnNodeCreation()
		{
			name = "Surface material";
		}
	
	}
}