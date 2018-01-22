using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PW.Core
{
	public class PWBiomeGraph : PWGraph
	{

		public override void InitializeInputAndOutputNodes()
		{
			inputNode = CreateNewNode< PWNodeBiomeGraphInput >(new Vector2(-100, 0));
			outputNode = CreateNewNode< PWNodeBiomeGraphOutput >(new Vector2(100, 0));
		}

		public override void OnEnable()
		{
			graphType = PWGraphType.Biome;
			base.OnEnable();
		}

		public override void OnDisable()
		{
			base.OnDisable();
		}
	
	}
}