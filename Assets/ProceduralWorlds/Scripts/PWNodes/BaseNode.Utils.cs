using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using ProceduralWorlds.Core;
using ProceduralWorlds.Node;

//Utils for BaseNode class
namespace ProceduralWorlds
{
	public partial class BaseNode
	{
		public IEnumerable< Anchor > inputAnchors
		{
			get
			{
				foreach (var anchorField in inputAnchorFields)
					foreach (var anchor in anchorField.anchors)
						yield return anchor;
			}
		}
		
		public IEnumerable< Anchor > outputAnchors
		{
			get
			{
				foreach (var anchorField in outputAnchorFields)
					foreach (var anchor in anchorField.anchors)
						yield return anchor;
			}
		}
		
		public void Duplicate()
		{
			var newNode = graphRef.CreateNewNode(GetType(), rect.position + new Vector2(50, 50), name);

			//copy internal datas to the new node:
			foreach (var field in newNode.undoableFields)
			{
				var value = field.GetValue(this);
				field.SetValue(newNode, value);
			}
		}
	}
}