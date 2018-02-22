using UnityEditor;
using UnityEngine;
using PW.Core;

namespace PW.Node
{
	public class PWNodeSlider : PWNode
	{
	
		[PWOutput]
		public float	outValue = .5f;
		
		public float	min = 0;
		public float	max = 1;
	
		//no process needed, value already set by the slider.
	}
}
