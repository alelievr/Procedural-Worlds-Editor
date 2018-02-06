using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using PW.Core;
using PW.Node;

//Utils for PWNode class
namespace PW
{
	public partial class PWNode
	{
		public void Duplicate(PWGraph attachedGraph)
		{
			var newNode = ScriptableObject.Instantiate(this);

			foreach (var af in newNode.anchorFields)
			{
				af.anchors.ForEach(a => {
					foreach (var link in a.links)
						a.RemoveLinkReference(link);
				});
			}

			if (attachedGraph != null)
			{
				attachedGraph.AddInitializedNode(newNode);
			}

			newNode.UpdateWorkStatus();

			newNode.rect.position += new Vector2(20, 20);
		}

		/// <summary>
		/// Warning: this function must be called before to modify a node property to register it's value
		/// </summary>
		/// <example>
		/// Like this:
		/// <code>
		/// float property = 0;
		/// 
		/// public override void OnNodeGUI() {
		///		EditorGUI.BeginChangeCheck();
		/// 	float property2 = EditorGUILayout.Slider(property, 0, 10);
		///		if (EditorGUI.EncChangeCheck())
		///			RecordUndo();
		///		property = property2;
		/// }
		/// </code>
		/// </example>
		public void RecordUndo()
		{
			Undo.RecordObject(this, "Updated property in " + name);
		}
	}
}