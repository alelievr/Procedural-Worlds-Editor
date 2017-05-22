using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace PW
{
	public class BiomeSwitchTree {

		BiomeSwitchNode		root = null;

		private enum SwitchMode
		{
			Unknown,
			Float,
			Bool,
		}

		private class BiomeSwitchNode
		{
			public float			min;
			public float			max;
			public bool				value;
			public SwitchMode		mode;
			public PWBiomeSwitchMode	biomeSwitchMode;

			List< BiomeSwitchNode >	childs = new List< BiomeSwitchNode >();
			Action					terraformer = null;
			
			public BiomeSwitchNode()
			{
				mode = SwitchMode.Unknown;
			}

			public void SetSwitchValue(float min, float max, PWBiomeSwitchMode biomeSwitchMode)
			{
				this.min = min;
				this.max = max;
				this.mode = SwitchMode.Float;
				this.biomeSwitchMode = biomeSwitchMode;
			}
			
			public void SetSwitchValue(bool value, PWBiomeSwitchMode biomeSwitchMode)
			{
				this.value = value;
				this.mode = SwitchMode.Bool;
				this.biomeSwitchMode = biomeSwitchMode;
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

			public void SetChildCount(int count)
			{
				while (childs.Count != count)
					childs.Add(new BiomeSwitchNode());
			}

			public BiomeSwitchNode GetChildAt(int i)
			{
				return childs[i];
			}

			public IEnumerable< BiomeSwitchNode > GetChilds()
			{
				return childs;
			}

			public override string ToString()
			{
				if (mode == SwitchMode.Bool)
					return "[" + biomeSwitchMode + "]: " + value;
				else if (mode == SwitchMode.Float)
					return "[" + biomeSwitchMode + "]: " + min + " -> " + max;
				return "non-initialized switch";
			}
		}

		void BuildTreeInternal(PWNode node, BiomeSwitchNode currentNode, int depth)
		{
			Debug.Log("evaluated node: " + node);
			if (node == null)
				return ;
			
			//TODO: tree build support subgraphs
			if (node.GetType() == typeof(PWNodeBiomeSwitch))
			{
				PWNodeBiomeSwitch	bSwitch = node as PWNodeBiomeSwitch;

				switch (bSwitch.switchMode)
				{
					case PWBiomeSwitchMode.Water:
						currentNode.SetChildCount(2);

						currentNode.GetChildAt(0).SetSwitchValue(false, bSwitch.switchMode);
						currentNode.GetChildAt(1).SetSwitchValue(true, bSwitch.switchMode);

						break ;
					default:
						currentNode.SetChildCount(bSwitch.switchDatas.Count);

						for (int i = 0; i < bSwitch.switchDatas.Count; i++)
						{
							var child = currentNode.GetChildAt(i);
							var sData = bSwitch.switchDatas[i];
		
							child.SetSwitchValue(sData.min, sData.max, bSwitch.switchMode);
						}
						break ;
				}
				int childIndex = 0;
				foreach (var outNode in node.GetOutputNodes())
					BuildTreeInternal(outNode, currentNode.GetChildAt(childIndex++), depth + 1);
			}
			else if (node.GetType() != typeof(PWNodeBiomeBinder))
			{
				foreach (var outNode in node.GetOutputNodes())
					BuildTreeInternal(outNode, currentNode, depth++);
			}
			else
				return ;
		}

		public void BuildTree(PWNode node)
		{
			root = new BiomeSwitchNode();
			BuildTreeInternal(node, root, 0);

			DumpBuiltTree();
		}

		void DumpBuiltTree()
		{
			BiomeSwitchNode current = root;
			
			Debug.Log("built tree:");
			string	childs = "";
			foreach (var child in current.GetChilds())
				childs += child;
			Debug.Log("swicth line" + childs);
		}
	}
}
