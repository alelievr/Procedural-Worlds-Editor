using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Biomator;

namespace ProceduralWorlds.Core
{
	public class NodeBiomeGraphOutput : BaseNode
	{

		[Input("Biome")]
		public Biome				inputBiome;

		[Input]
		public PWArray< object >	inputValues = new PWArray< object >();

		public override void OnNodeCreation()
		{
			name = "Biome output";
		}
	}
}