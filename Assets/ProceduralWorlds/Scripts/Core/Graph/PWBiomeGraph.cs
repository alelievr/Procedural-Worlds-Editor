using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PW.Core
{
	public class PWBiomeGraph : PWGraph
	{
		
		public override void Initialize()
		{
			base.Initialize();
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