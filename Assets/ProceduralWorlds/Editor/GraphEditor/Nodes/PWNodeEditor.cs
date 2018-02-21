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
		[SerializeField]
		int							maxAnchorRenderHeight = 0;

		//Utils
		protected Event				e { get { return Event.current; } }
		[System.NonSerialized]
		protected DelayedChanges	delayedChanges = new DelayedChanges();

		//Getters
		PWGraph						graphRef { get { return node.graphRef; } }
		PWGraphEditorEventInfo		editorEvents { get { return editorEvents; } }
		protected PWGUIManager		PWGUI { get { return node.PWGUI; } }
		Vector2						graphPan { get { return node.graphRef.panPosition; } }
		PWGraphEditor				graphEditor;

		//state bools
		public bool					isSelected = false;
		public bool					isDragged = false;
		public bool					windowNameEdit = false;
		
		public delegate void				AnchorAction(PWAnchor anchor);
		
		//fired when the dragged link is above an anchor
		protected event AnchorAction		OnDraggedLinkOverAnchor;
		//fired when the dragged link quit the zone above the anchor
		protected event AnchorAction		OnDraggedLinkQuitAnchor;

		PWNode						node;

		bool						guiEnabled = false;

		void OnEnable()
		{
			node = target as PWNode;
			delayedChanges.Clear();

			BindEvents();
			OnNodeEnable();
		}

		void OnGUIEnable()
		{
			LoadHeaderResouces();
			LoadCoreResources();

			guiEnabled = true;
		}

		public override void OnInspectorGUI()
		{
			if (!guiEnabled)
				OnGUIEnable();
			
			RenderNode();
			delayedChanges.Update();
		}
	
		void OnDisable()
		{
			OnNodeDisable();
			UnBindEvents();
		}
		
		public abstract void OnNodeEnable();
		public abstract void OnNodeDisable();
		public abstract void OnNodeGUI();

		public virtual void OnNodePreProcess() {}
		public virtual void OnNodePostProcess() {}
		public virtual void OnNodeUnitTest() {}
	}
}