using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
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
		protected PWGUIManager		PWGUI = new PWGUIManager();

		//Getters
		protected PWGraph					graphRef { get { return node.graphRef; } }
		protected PWBiomeGraph				biomeGraphRef { get { return node.graphRef as PWBiomeGraph; } }
		protected PWMainGraph				mainGraphRef { get { return node.graphRef as PWMainGraph; } }
		protected PWGraphEditorEventInfo	editorEvents { get { return editorEvents; } }
		protected Vector2					graphPan { get { return node.graphRef.panPosition; } }
		protected Rect						rect { get { return node.rect; } }
		protected PWGraphEditor				graphEditor;

		//state bools
		public bool						windowNameEdit = false;
		
		public delegate void			AnchorAction(PWAnchor anchor);
		
		//fired when the dragged link is above an anchor
		protected event AnchorAction	OnDraggedLinkOverAnchor;
		//fired when the dragged link quit the zone above the anchor
		protected event AnchorAction	OnDraggedLinkQuitAnchor;

		public static Dictionary< PWNode, PWNodeEditor >	openedNodeEdiors = new Dictionary< PWNode, PWNodeEditor >();

		PWNode						node;

		bool						guiEnabled = false;

		void OnEnable()
		{
			node = target as PWNode;
			delayedChanges.Clear();

			//set the PWGUI current node:
			PWGUI.SetNode(node);
			
			//add our editor to the list:
			openedNodeEdiors[node] = this;

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
			//remove our editor:
			openedNodeEdiors.Remove(node);

			OnNodeDisable();
			UnBindEvents();
		}
		
		public virtual void OnNodeEnable() {}
		public virtual void OnNodeDisable() {}
		public abstract void OnNodeGUI();

		public virtual void OnNodePreProcess() {}
		public virtual void OnNodePostProcess() {}
		public virtual void OnNodeUnitTest() {}
	}
}