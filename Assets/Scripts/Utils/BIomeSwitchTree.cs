using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System;
using Debug = UnityEngine.Debug;

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
				while (childs.Count < count)
					childs.Add(new BiomeSwitchNode());
			}

			public BiomeSwitchNode GetChildAt(int i, bool createIfNotExists = false)
			{
				if (createIfNotExists && i >= childs.Count)
					SetChildCount(i + 1);
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
			if (node == null)
				return ;
			
			// Debug.Log("evaluated node: " + node);
			
			//TODO: anchor to multiple PWNodeBiomeSwitch management
			if (node.GetType() == typeof(PWNodeBiomeSwitch))
			{
				PWNodeBiomeSwitch	bSwitch = node as PWNodeBiomeSwitch;
				int					outputLinksCount = bSwitch.GetLinks().Count;
				int					childIndex = 0;

				currentNode.SetChildCount(outputLinksCount);
				switch (bSwitch.switchMode)
				{
					case PWBiomeSwitchMode.Water:
						int?	terrestrialAnchorId = node.GetAnchorId(PWAnchorType.Output, 0);
						int?	aquaticAnchorId = node.GetAnchorId(PWAnchorType.Output, 1);

						//get all nodes on the first anchor:
						if (terrestrialAnchorId != null)
						{
							var nodes = node.GetNodesAttachedToAnchor(terrestrialAnchorId.Value);
							for (int i = 0; i < nodes.Count; i++)
								currentNode.GetChildAt(childIndex++).SetSwitchValue(false, bSwitch.switchMode);
						}
						if (aquaticAnchorId != null)
						{
							var nodes = node.GetNodesAttachedToAnchor(aquaticAnchorId.Value);
							for (int i = 0; i < nodes.Count; i++)
								currentNode.GetChildAt(childIndex++).SetSwitchValue(true, bSwitch.switchMode);
						}

						break ;
					default:
						// Debug.Log("swicth data count for node " + node.nodeId + ": " + bSwitch.switchDatas.Count);

						for (int anchorIndex = 0; anchorIndex < bSwitch.switchDatas.Count; anchorIndex++)
						{
							int? anchorId = node.GetAnchorId(PWAnchorType.Output, anchorIndex);

							if (anchorId == null)
								continue ;

							var attachedNodesToAnchor = node.GetNodesAttachedToAnchor(anchorId.Value);

							// if (attachedNodesToAnchor.Count == 0)
								// Debug.LogWarning("nothing attached to the biome switch output " + anchorIndex);

							foreach (var attachedNode in attachedNodesToAnchor)
							{
								var child = currentNode.GetChildAt(childIndex++);
								var sData = bSwitch.switchDatas[anchorIndex];

								child.SetSwitchValue(sData.min, sData.max, bSwitch.switchMode);
							}
						}
						break ;
				}
				childIndex = 0;
				foreach (var outNode in node.GetOutputNodes())
				{
					BuildTreeInternal(outNode, currentNode.GetChildAt(childIndex, true), depth + 1);
					if (outNode.GetType() == typeof(PWNodeBiomeSwitch))
						childIndex++;
				}
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
			Stopwatch st = new Stopwatch();
			st.Start();
			root = new BiomeSwitchNode();
			BuildTreeInternal(node, root, 0);
			st.Stop();

			Debug.Log("built tree time: " + st.ElapsedMilliseconds);
			
			DumpBuiltTree();
		}

		void DumpBiomeTree(BiomeSwitchNode node, int depth = 0)
		{
			Debug.Log("node at depth " + depth + ": " + node);

			foreach (var child in node.GetChilds())
				DumpBiomeTree(child, depth + 1);
		}

		void DumpBuiltTree()
		{
			BiomeSwitchNode current = root;
			
			Debug.Log("built tree:");
			DumpBiomeTree(root);
			string	childs = "";
			foreach (var child in current.GetChilds())
				childs += child + " | ";
			Debug.Log("swicth line1: " + childs);

			childs = "";
			foreach (var child in current.GetChilds())
				foreach (var childOfChild in child.GetChilds())
					childs += childOfChild + " | ";
			Debug.Log("swicth line2: " + childs);
		}
	}
}
