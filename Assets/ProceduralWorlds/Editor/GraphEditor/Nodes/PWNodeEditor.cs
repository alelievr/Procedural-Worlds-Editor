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
		protected PWGraph					graphRef { get { return nodeRef.graphRef; } }
		protected PWBiomeGraph				biomeGraphRef { get { return nodeRef.graphRef as PWBiomeGraph; } }
		protected PWMainGraph				mainGraphRef { get { return nodeRef.graphRef as PWMainGraph; } }
		protected PWGraphEditorEventInfo	editorEvents { get { return editorEvents; } }
		protected Vector2					graphPan { get { return nodeRef.graphRef.panPosition; } }
		protected Rect						rect { get { return nodeRef.rect; } }
		protected PWGraphEditor				graphEditor;

		//state bools
		public bool						windowNameEdit = false;
		
		public delegate void			AnchorAction(PWAnchor anchor);
		
		//fired when the dragged link is above an anchor
		protected event AnchorAction	OnDraggedLinkOverAnchor;
		//fired when the dragged link quit the zone above the anchor
		protected event AnchorAction	OnDraggedLinkQuitAnchor;

		public static Dictionary< PWNode, PWNodeEditor >	openedNodeEdiors = new Dictionary< PWNode, PWNodeEditor >();

		[System.NonSerialized]
		PWNode						nodeRef;

		[System.NonSerialized]
		bool						guiEnabled = false;

		void OnEnable()
		{
			nodeRef = target as PWNode;
			
			if (nodeRef == null)
			{
				Debug.Log("Destroying null target node editor !");
				DestroyImmediate(this);
				return ;
			}

			delayedChanges.Clear();

			graphEditor = EditorWindow.focusedWindow as PWGraphEditor;

			//set the PWGUI current nodeRef:
			PWGUI.SetNode(nodeRef);
			
			//add our editor to the list:
			openedNodeEdiors[nodeRef] = this;

			BindEvents();
			OnNodeEnable();
		}

		void OnGUIEnable()
		{
			LoadHeaderResouces();
			LoadCoreResources();
			LoadAnchorResources();

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
			openedNodeEdiors.Remove(nodeRef);

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