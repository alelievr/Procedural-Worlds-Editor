using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using PW.Core;
using PW.Node;

//Utils for PWNode class
namespace PW
{
	public partial class PWNode
	{
		public IEnumerable< PWAnchor > inputAnchors
		{
			get
			{
				foreach (var anchorField in inputAnchorFields)
					foreach (var anchor in anchorField.anchors)
						yield return anchor;
			}
		}
		
		public IEnumerable< PWAnchor > outputAnchors
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
			foreach (var fieldInfo in newNode.undoableFields)
			{
				var value = fieldInfo.GetValue(this);
				fieldInfo.SetValue(newNode, value);
			}
		}
	}
}