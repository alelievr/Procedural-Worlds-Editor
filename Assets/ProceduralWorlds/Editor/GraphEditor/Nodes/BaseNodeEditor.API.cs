using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;
using ProceduralWorlds;
using ProceduralWorlds.Node;

namespace ProceduralWorlds.Editor
{
	public abstract partial class BaseNodeEditor
	{
		//default notify reload will be sent to all node childs
		//also fire a Process event for the target nodes
		public void NotifyReload()
		{
			var nodes = graphRef.GetNodeChildsRecursive(nodeRef);

			foreach (var node in nodes)
				openedNodeEdiors[node].OnNodePreProcess();
			
			//add our node to the process pass
			nodes.Add(nodeRef);

			graphRef.ProcessNodes(nodes);
			
			foreach (var editorKP in openedNodeEdiors)
				editorKP.Value.OnNodePostProcess();
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