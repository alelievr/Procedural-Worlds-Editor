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