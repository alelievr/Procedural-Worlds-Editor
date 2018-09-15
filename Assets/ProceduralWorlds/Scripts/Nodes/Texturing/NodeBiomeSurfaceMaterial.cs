using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Core;
using ProceduralWorlds.Biomator;

namespace ProceduralWorlds.Nodes
{
	public class NodeBiomeSurfaceMaterial : BaseNode
	{

		[Output]
		public BiomeSurfaceMaterial		surfaceMaterial = new BiomeSurfaceMaterial();

		public override void OnNodeCreation()
		{
			name = "Surface material";
		}
	
	}
}