using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using ProceduralWorlds.Core;

namespace ProceduralWorlds.Nodes
{
	public class NodeAdd : BaseNode
	{
	
		[Input]
		public PWArray< float >	values = new PWArray< float >();
	
		[Output]
		public float	fOutput;
	
		public bool	roundToInt = false;
	
		public override void OnNodeCreation()
		{
			//override window width
			rect.width = 150;
		}

		public override void OnNodeProcess()
		{
			fOutput = 0;
			foreach (var val in values)
				fOutput += val;
			
			if (roundToInt)
				fOutput = Mathf.RoundToInt(fOutput);
		}
	}
}
