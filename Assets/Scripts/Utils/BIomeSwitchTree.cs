using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace PW
{
	public class BiomeSwitchTree {

		private class BiomeSwitchNode
		{
			public Vector2			range;
			public bool				value;

			List< BiomeSwitchNode >	childs = null;
			Action					terraformer = null;
			
			public BiomeSwitchNode(Vector2 range, bool value)
			{
				this.value = value;
				this.range = range;
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

		public void BuildTree(PWNode node)
		{
			//TODO: build the tree with the graph
		}

	}
}
