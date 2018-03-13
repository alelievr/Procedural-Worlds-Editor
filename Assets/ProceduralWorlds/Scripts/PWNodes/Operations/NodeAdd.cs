using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using ProceduralWorlds.Core;

namespace ProceduralWorlds.Node
{
	public class NodeAdd : BaseNode
	{
	
		[Input]
		public PWArray< float >	values = new PWArray< float >();
	
		[Output]
		public float	fOutput;
	
		bool			intify = false;
	
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
			
			if (intify)
				fOutput = Mathf.RoundToInt(fOutput);
		}
	}
}
