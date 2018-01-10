using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using PW.Core;
using PW.Node;

//Utils for PWNode class
namespace PW
{
	public partial class PWNode
	{
		PWAnchorType			InverAnchorType(PWAnchorType type)
		{
			if (type == PWAnchorType.Input)
				return PWAnchorType.Output;
			else if (type == PWAnchorType.Output)
				return PWAnchorType.Input;
			return PWAnchorType.None;
		}
		
		public static PWLinkType GetLinkTypeFromType(Type fieldType)
		{
			if (fieldType == typeof(Sampler2D))
				return PWLinkType.Sampler2D;
			if (fieldType == typeof(Sampler3D))
				return PWLinkType.Sampler3D;
			if (fieldType == typeof(Vector3) || fieldType == typeof(Vector3i))
				return PWLinkType.ThreeChannel;
			if (fieldType == typeof(Vector4))
				return PWLinkType.FourChannel;
			return PWLinkType.BasicData;
		}

		public void Duplicate()
		{
			Debug.Log("TODO :) !");
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