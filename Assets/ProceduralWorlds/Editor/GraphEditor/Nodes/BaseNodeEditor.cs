using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using ProceduralWorlds.Nodes;
using ProceduralWorlds.Core;
using ProceduralWorlds;

namespace ProceduralWorlds.Editor
{
	[CustomEditor(typeof(BaseNode))]
	public abstract partial class BaseNodeEditor : UnityEditor.Editor
	{
		[SerializeField]
		int								maxAnchorRenderHeight;

		//Utils
		protected Event					e { get { return Event.current; } }
		[System.NonSerialized]
		protected DelayedChanges		delayedChanges = new DelayedChanges();
		protected ProceduralWorldsGUI	PWGUI = new ProceduralWorldsGUI();

		//Getters
		protected BaseGraph					graphRef { get { return nodeRef.graphRef; } }
		protected BiomeGraph				biomeGraphRef { get { return nodeRef.graphRef as BiomeGraph; } }
		protected WorldGraph				worldGraphRef { get { return nodeRef.graphRef as WorldGraph; } }
		protected BaseGraphEditorEventInfo	editorEvents { get { return graphRef.editorEvents; } }
		protected Vector2					graphPan { get { return nodeRef.graphRef.panPosition; } }
		protected Rect						rect { get { return nodeRef.rect; } }
		protected BaseGraphEditor			graphEditor;

		//state bools
		public bool						windowNameEdit;
		public bool						isInsideGraph;
		public bool						nodeInspectorGUIOverloaded { get; private set; }
		
		public delegate void			AnchorAction(Anchor anchor);
		
		//fired when the dragged link is above an anchor
		protected event AnchorAction	OnDraggedLinkOverAnchor;
		//fired when the dragged link quit the zone above the anchor
		protected event AnchorAction	OnDraggedLinkQuitAnchor;

		[System.NonSerialized]
		BaseNode						nodeRef;

		[System.NonSerialized]
		bool							guiEnabled;

		[System.NonSerialized]
		UndoRedoHelper					undoRedoHelper;

		void OnEnable()
		{
			nodeRef = target as BaseNode;
			
			if (nodeRef == null)
			{
				DestroyImmediate(this);
				return ;
			}

			undoRedoHelper = new UndoRedoHelper(nodeRef);
			undoRedoHelper.LoadUndoableFields();
			
			nodeInspectorGUIOverloaded = GetType().GetMethod("OnNodeInspectorGUI").DeclaringType == GetType();

			delayedChanges.Clear();

			//set the PWGUI current nodeRef:
			PWGUI.SetNode(nodeRef);
		}

		public void Initialize(BaseGraphEditor graphEditor)
		{
			this.graphEditor = graphEditor;

			BindEvents();
			OnNodeEnable();
		}

		void OnGUIEnable()
		{
			using (PWGUISkin.Get())
			{
				LoadHeaderResouces();
				LoadCoreResources();
				LoadAnchorResources();
			}

			guiEnabled = true;
		}

		public override void OnInspectorGUI()
		{
			//if we loose the reference of our node, auto-destruct
			if (nodeRef == null)
				DestroyImmediate(this);
			
			if (!guiEnabled)
				OnGUIEnable();
			
			if (isInsideGraph)
				RenderNode();
			else
				RenderInspector();
				
			delayedChanges.Update();
		}
	
		void OnDisable()
		{
			if (nodeRef == null)
				return ;
			
			OnNodeDisable();
			UnBindEvents();
		}
		
		public virtual void OnNodeEnable() {}
		public virtual void OnNodeDisable() {}
		public abstract void OnNodeGUI();
		public virtual void OnNodeInspectorGUI() {}

		public virtual void OnNodePreProcess() {}
		public virtual void OnNodePostProcess() {}
		public virtual void OnNodeUnitTest() {}
	}
}