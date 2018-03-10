using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW.Biomator;

namespace PW.Core
{
	public class PWNodeBiomeGraphOutput : PWNode
	{

		[PWInput("Biome")]
		public Biome				inputBiome;

		[PWInput]
		public PWArray< object >	inputValues = new PWArray< object >();

		public override void OnNodeCreation()
		{
			name = "Biome output";
		}
	}
}