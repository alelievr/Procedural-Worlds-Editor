using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using PW.Core;
using PW.Node;

namespace PW
{
	[System.SerializableAttribute]
	public partial class PWNode : ScriptableObject
	{
		//Node datas:
		public Rect					rect = new Rect(400, 400, 200, 50);
		public int					id;
		public bool					renamable = false;
		public int					computeOrder = 0;
		public float				processTime = 0f;
		public string				classAQName;
		public PWColorSchemeName	colorSchemeName;
		new public string			name;

		//AnchorField lists
		public List< PWAnchorField >	inputAnchorFields = new List< PWAnchorField >();
		public List< PWAnchorField >	outputAnchorFields = new List< PWAnchorField >();
		public IEnumerable< PWAnchorField > anchorFields { get { foreach (var ia in inputAnchorFields) yield return ia; foreach (var ao in outputAnchorFields) yield return ao; } }

		//GUI utils to provide custom fields for Samplers, Range ...
		[SerializeField]
		protected PWGUIManager	PWGUI = new PWGUIManager();


		//Useful state bools:
		protected bool			realMode { get { return graphRef.IsRealMode(); } }
		public bool				isDragged = false;
		[System.NonSerialized]
		public bool				ready = false;
		//tell if the node have required unlinked input and so can't Process()
		public bool				canWork = false;
		public bool				isSelected = false;


		//Graph datas accessors:
		protected Vector3		chunkPosition { get { return graphRef.chunkPosition; } }
		protected int			chunkSize { get { return graphRef.chunkSize; } }
		protected int			seed { get { return graphRef.seed; } }
		protected float			step { get { return graphRef.step; } }
		protected PWGraphEditorEventInfo editorEvents { get { return graphRef.editorEvents; } }


		//Debug variables:
		[SerializeField]
		public bool				debug = false;

	#region Internal Node datas and style

		Vector2						graphPan { get { return graphRef.panPosition; } }
		[SerializeField]
		int							maxAnchorRenderHeight = 0;


		//Utils
		[System.NonSerialized]
		protected DelayedChanges	delayedChanges = new DelayedChanges();
		protected Event				e { get { return Event.current; } }


		[System.NonSerialized]
		public PWGraph				graphRef;
		protected PWMainGraph		mainGraphRef { get { return graphRef as PWMainGraph; } }
		protected PWBiomeGraph		biomeGraphRef { get { return graphRef as PWBiomeGraph; } }
		//TODO: data and mesh graphs

		public bool					windowNameEdit = false;

		//Serialization system:
		[System.NonSerialized]
		private	bool				deserializationAlreadyNotified = false;
		//tell if the node was enabled
		[System.NonSerialized]
		private bool				isEnabled = false;

	#endregion
	
		public delegate void					ReloadAction(PWNode from);
		public delegate void					AnchorAction(PWAnchor anchor);
		public delegate void					MessageReceivedAction(PWNode from, object message);
		public delegate void					LinkAction(PWNodeLink link);

		//fired when the node received a NotifyReload() or the user pressed Reload button in editor.
		public event ReloadAction				OnReload;
		//fired jstu after OnReload event;
		public event ReloadAction				OnPostReload;
		//fired when the node receive a SendMessage()
		protected event MessageReceivedAction	OnMessageReceived;

		//fired when this node was linked
		protected event AnchorAction			OnAnchorLinked;
		//fired when this node was unlinked
		protected event AnchorAction			OnAnchorUnlinked;
		//fired when the dragged link is above an anchor
		protected event AnchorAction			OnDraggedLinkOverAnchor;
		//fired when the dragged link quit the zone above the anchor
		protected event AnchorAction			OnDraggedLinkQuitAnchor;

		//fired only when realMode is false, just after OnNodeProcess is called;
		public event Action						OnPostProcess;

		//event relay, to simplify custom graph events:
		// public event Action< int >				OnChunkSizeChanged;
		// public event Action< int >				OnChunkPositionChanged;

		//TODO: send graphRef.RaiseOnNodeSelected when the node select itself

		//default notify reload will be sent to all node childs
		//also fire a Process event for the target nodes
		public void NotifyReload()
		{
			var nodes = graphRef.GetNodeChildsRecursive(this).ToList();

			foreach (var node in nodes)
				node.Reload(this);
			
			//add our node to the process pass
			nodes.Add(this);

			graphRef.ProcessNodes(nodes);
		}
		
		//send reload event to all node of the specified type
		public void NotifyReload(Type targetType)
		{
			var nodes = from node in graphRef.nodes
						where node.GetType() == targetType
						select node;
			
			foreach (var node in nodes)
				node.Reload(this);
		}

		//send reload to all nodes with a computeOrder smaller than minComputeOrder.
		public void NotifyReload(int minComputeOrder)
		{
			var nodes = from node in graphRef.nodes
						where node.computeOrder >= minComputeOrder
						select node;

			foreach (var node in nodes)
				node.Reload(this);
		}

		public void NotifyReload(PWNode node)
		{
			node.Reload(this);
		}

		public void NotifyReload(IEnumerable< PWNode > nodes)
		{
			foreach (var node in nodes)
				node.Reload(this);
		}

		public void Reload(PWNode from)
		{
			if (OnReload != null)
				OnReload(from);
			if (OnPostReload != null)
				OnPostReload(from);
		}

		public void SendMessage(PWNode target, object message)
		{
			target.OnMessageReceived(this, message);
		}
		
		public void SendMessage(Type targetType, object message)
		{
			var nodes = from node in graphRef.nodes
						where node.GetType() == targetType
						select node;
						
			foreach (var node in nodes)
				node.OnMessageReceived(this, message);
		}
		
		public void SendMessage(IEnumerable< PWNode > nodes, object message)
		{
			foreach (var node in nodes)
				node.OnMessageReceived(this, message);
		}

		public void OnAfterGraphDeserialize(PWGraph graph)
		{
			this.graphRef = graph;

			if (isEnabled)
				OnAfterNodeAndGraphDeserialized();
		}

	#region OnEnable, OnDisable, Initialize

		public void OnEnable()
		{
			isEnabled = true;

			LoadAssets();
			
			LoadFieldAttributes();

			UpdateAnchorProperties();

			if (debug)
				Debug.Log("Node OnEnable: " + GetType());

			//set the PWGUI current node:
			PWGUI.SetNode(this);

			//load the class QA name:
			classAQName = GetType().AssemblyQualifiedName;

			delayedChanges.Clear();

			if (graphRef != null)
				OnAfterNodeAndGraphDeserialized();
		}

		//here both our node and graph have been deserialized, we can now use it's datas
		void OnAfterNodeAndGraphDeserialized()
		{
			if (deserializationAlreadyNotified)
				return ;
			
			BindEvents();

			deserializationAlreadyNotified = true;

			//call the AfterDeserialize functions for anchors
			foreach (var anchorField in inputAnchorFields)
				anchorField.OnAfterDeserialize(this);
			foreach (var anchorField in outputAnchorFields)
				anchorField.OnAfterDeserialize(this);

			UpdateWorkStatus();
			
			//send OnEnabled events
			OnNodeEnable();
			OnAnchorEnable();

			//tell to the graph that this node is ready to work
			this.ready = true;
			graphRef.NotifyNodeReady(this);
		}

		//called only once, when the node is created
		public void Initialize(PWGraph graph)
		{
			if (debug)
				Debug.LogWarning("Node " + GetType() + "Initialize !");

			//generate "unique" id for node:
			byte[] bytes = System.Guid.NewGuid().ToByteArray();
			id = (int)bytes[0] | (int)bytes[1] << 8 | (int)bytes[2] << 16 | (int)bytes[3] << 24;

			//set the node name:
			name = PWNodeTypeProvider.GetNodeName(GetType());

			//set the name of the scriptableObject
			base.name = name;

			//set the graph reference:
			graphRef = graph;

			//initialize anchors
			OnAfterNodeAndGraphDeserialized();

			//call virtual NodeCreation method
			OnNodeCreation();
		}

		public void OnDisable()
		{
			foreach (var anchor in anchorFields)
				anchor.OnDisable();

			if (debug)
				Debug.Log("Node " + GetType() + " Disable");
			
			UnBindEvents();

			//if the node was properly enabled, we call it's onDisable functions
			if (graphRef != null)
			{
				OnNodeDisable();
				OnAnchorDisable();
			}
		}

	#endregion

	#region inheritence virtual API

		public virtual void OnNodeCreation() {}

		public virtual void OnNodeEnable() {}

		public virtual void OnNodeProcess() {}

		public virtual void OnNodeDisable() {}

		public virtual void OnNodeDelete() {}

		public virtual void OnNodeProcessOnce() {}

		public virtual void OnNodeUnitTest() {}

		public virtual bool	OnNodeAnchorLink(string propName, int index) { return true; }

		public virtual void OnNodeAnchorUnlink(string propName, int index) {}

	#endregion

		void		OnAnchorEnable()
		{
			foreach (var anchorField in inputAnchorFields)
				anchorField.OnEnable();
			foreach (var anchorField in outputAnchorFields)
				anchorField.OnEnable();
		}

		void		OnAnchorDisable()
		{
			foreach (var anchorField in inputAnchorFields)
				anchorField.OnDisable();
			foreach (var anchorField in outputAnchorFields)
				anchorField.OnDisable();
		}

		void		CreateAnchorField(PWAnchorType type)
		{
			PWAnchorField newAnchorField = new PWAnchorField();

			newAnchorField.Initialize(this);
			newAnchorField.OnEnable();
		}

		void		DisableUnlinkableAnchors(PWAnchor anchor)
		{
			List< PWAnchorField >	anchorFields;

			if (anchor.anchorType == PWAnchorType.Output)
				anchorFields = inputAnchorFields;
			else
				anchorFields = outputAnchorFields;
			
			foreach (var anchorField in anchorFields)
				anchorField.DisableIfUnlinkable(anchor);
		}

		void		ResetUnlinkableAnchors()
		{
			foreach (var anchorField in inputAnchorFields)
				anchorField.ResetLinkable();
			foreach (var anchorField in outputAnchorFields)
				anchorField.ResetLinkable();
		}
	
		void		UpdateWorkStatus()
		{
			canWork = false;

			foreach (var anchorField in inputAnchorFields)
			{
				if (anchorField.required && !anchorField.multiple)
				{
					foreach (var anchor in anchorField.anchors)
						if (anchor.linkCount == 0)
						{
							// Debug.Log("node " + GetType() + " can't work cauz it's anchor " + anchor.fieldName + " is not linked");
							return ;
						}
				}
			}
			
			canWork = true;
		}

		public void	RemoveSelf()
		{
			RemoveAllLinks();

			//the node instance will be removed by the editor at the same time that it's asset
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

	#region Unused (for the moment) overrided functions
		public void	OnDestroy()
		{
			// Debug.Log("node " + nodeTypeName + " detroyed !");
		}

		public void	OnGUI()
		{
			EditorGUILayout.LabelField("You are in the wrong window !");
		}
		
		public void OnInspectorGUI()
		{
			EditorGUILayout.LabelField("nope !");
		}
	#endregion

		public override string ToString()
		{
			return "node " + name + "[" + GetType() + "]";
		}
    }
}
