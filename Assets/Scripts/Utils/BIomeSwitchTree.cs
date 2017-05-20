using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace PW
{
	public class BiomeSwitchTree {

		BiomeSwitchNode		root = null;
		BiomeSwitchNode		currentTreeNode;

		private class BiomeSwitchNode
		{
			public Vector2			range;
			public bool				value;
			bool					initialized = false;

			List< BiomeSwitchNode >	childs = null;
			Action					terraformer = null;
			
			public BiomeSwitchNode() {}

			public void SetSwitchValue(Vector2 range, bool value)
			{
				this.value = value;
				this.range = range;
				initialized = true;
			}

			public BiomeSwitchNode GetNext(float value)
			{
				//find the value in range in childs and return it
				return null;
			}

			public BiomeSwitchNode GetNext(bool val)
			{
				if (val == value)
					return childs[0];
				return childs[1];
			}
		}

		public void BuildTree(PWNode node, int depth = 0)
		{
			var outputNodes = node.GetOutputNodes();

			if (depth == 0)
				currentTreeNode = root = new BiomeSwitchNode();

			foreach (var outNode in outputNodes)
			{
				if (node.GetType() == typeof(PWNodeBiomeSwitch))
				{
					PWNodeBiomeSwitch bSwitch = node as PWNodeBiomeSwitch;
					//TODO
					// currentTreeNode.SetSwitchValue(bSwitch.);
				}
				else if (node.GetType() != typeof(PWNodeBiomeBinder))
					BuildTree(outNode, depth++);
				else
					break ;
			}
		}

	}
}
