using UnityEngine;
using ProceduralWorlds.Core;

namespace ProceduralWorlds.Node
{
	public class NodeSlider : BaseNode
	{
	
		[Output]
		public float	outValue = .5f;
		
		public float	min = 0;
		public float	max = 1;
	
		//no process needed, value already set by the slider.
	}
}
