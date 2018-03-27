using UnityEngine;
using ProceduralWorlds.Core;

namespace ProceduralWorlds.Nodes
{
	public class NodeSlider : BaseNode
	{
	
		[Output]
		public float	outValue = .5f;

		public float	sliderValue;
		
		public float	min = 0;
		public float	max = 1;
	
		public override void OnNodeProcessOnce()
		{
			outValue = sliderValue;
		}
	}
}
