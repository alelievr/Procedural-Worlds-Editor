using UnityEditor;
using UnityEngine;
using System;
using PW.Node;
using PW.Core;
using PW;

namespace PW.Editor
{
	[CustomEditor(typeof(PWNode))]
	public abstract partial class PWNodeEditor : UnityEditor.Editor
	{
		[System.NonSerialized]
		protected DelayedChanges	delayedChanges = new DelayedChanges();
		protected PWGUIManager		PWGUI { get { return node.PWGUI; } }
		
		Vector2						graphPan { get { return node.graphRef.panPosition; } }
		[SerializeField]
		int							maxAnchorRenderHeight = 0;

		//Utils
		protected Event				e { get { return Event.current; } }

		//Getters
		PWGraph						graphRef { get { return node.graphRef; } }

		public bool					isSelected = false;
		public bool					isDragged = false;
		public bool					windowNameEdit = false;

		PWNode		node;

		void OnEnable()
		{
			node = target as PWNode;
			delayedChanges.Clear();
			OnNodeEnable();
		}

		public override void OnInspectorGUI()
		{
			EditorGUILayout.LabelField("OLOL");
			OnNodeGUI();
			delayedChanges.Update();
		}
	
		void OnDisable()
		{
			OnNodeDisable();
		}
		
		void		OnClickedOutside()
		{
			if (Event.current.button == 0)
			{
				windowNameEdit = false;
				GUI.FocusControl(null);
			}
			if (Event.current.button == 0 && !Event.current.shift)
				isSelected = false;
			isDragged = false;
		}

		public abstract void OnNodeEnable();
		public abstract void OnNodeDisable();
		public abstract void OnNodeGUI();

		public virtual void OnNodePreProcess() {}
		public virtual void OnNodePostProcess() {}
		public virtual void OnNodeUnitTest() {}
	}
}