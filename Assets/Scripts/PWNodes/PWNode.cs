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
		public Rect				windowRect = new Rect(400, 400, 200, 50);
		public int				id;
		public bool				renamable = false;
		public int				computeOrder = 0;
		public float			processTime = 0f;
		public string			classQAName;
		new public string		name;

		//AnchorField lists
		public List< PWAnchorField >	inputAnchorFields;
		public List< PWAnchorField >	outputAnchorFields;

		//GUI utils to provide custom fields for Samplers, Range ...
		[SerializeField]
		protected PWGUIManager	PWGUI = new PWGUIManager();


		//Useful state bools:
		protected bool			isDependent { get; private set; }
		protected bool			realMode { get { return graphRef.IsRealMode(); } }
		public bool				isDragged = false;
		//tell if the node have required unlinked input and so can't Process()
		public bool				canWork = false;


		//Graph datas accessors:
		protected Vector3		chunkPosition { get { return graphRef.chunkPosition; } }
		protected int			chunkSize { get { return graphRef.chunkSize; } }
		protected int			seed { get { return graphRef.seed; } }
		protected float			step { get { return graphRef.step; } }


		//Debug variables:
		[SerializeField]
		protected bool			nodeDebug = false;
		[SerializeField]
		protected bool			linkDebug = false;
		[SerializeField]
		protected bool			anchorDebug = false;

	#region Internal Node datas and style
		static GUIStyle 			renameNodeTextFieldStyle = null;
		static GUIStyle				debugStyle = null;
		public GUIStyle				windowStyle;
		public GUIStyle				windowSelectedStyle;
		public static GUIStyle		innerNodePaddingStyle = null;
		
		static Texture2D			editIcon = null;

		// static Color		anchorAttachAddColor = new Color(.1f, .1f, .9f);
		// static Color		anchorAttachNewColor = new Color(.1f, .9f, .1f);
		// static Color		anchorAttachReplaceColor = new Color(.9f, .1f, .1f);

		Vector2						graphPan { get { return graphRef.panPosition; } }
		[SerializeField]
		int							maxAnchorRenderHeight = 0;

		[System.NonSerialized]
		protected DelayedChanges	delayedChanges = new DelayedChanges();

		[System.NonSerialized]
		public PWGraph				graphRef;
		protected PWMainGraph		mainGraphRef { get { return graphRef as PWMainGraph; } }
		protected PWBiomeGraph		biomeGraphRef { get { return graphRef as PWBiomeGraph; } }
		//TODO: data and mesh graphs

		public bool					windowNameEdit = false;
		public bool					selected = false;

	#endregion
	
		public delegate void					ReloadAction(PWNode from);
		public delegate void					NodeLinkAction(PWAnchor anchor);
		public delegate void					MessageReceivedAction(PWNode from, object message);
		public delegate void					LinkAction(PWNodeLink link);

		//fired when the node received a NotifyReload() or the user pressed Reload button in editor.
		public event ReloadAction				OnReload;
		//fired jstu after OnReload event;
		public event ReloadAction				OnPostReload;
		//fired when the node receive a SendMessage()
		protected event MessageReceivedAction	OnMessageReceived;

		//fired when this node was linked
		public event NodeLinkAction				OnAnchorLinked;
		//fired when this node was unlinked
		protected event NodeLinkAction			OnAnchorUnlinked;
		//fired when the dragged link is above an anchor
		protected event NodeLinkAction			OnDraggedLinkOverAnchor;
		//fired when the dragged link quit the zone above the anchor
		protected event NodeLinkAction			OnDraggedLinkQuitAnchor;
		
		//fired when a link attached to this node is created
		private event LinkAction				OnLinkCreated;
		//fired when a link is removed
		public event LinkAction					OnLinkRemoved;

		//fired when a link attached to this node is selected
		public event LinkAction					OnLinkSelected;
		//fired when a link attached to this node is unselected
		public event LinkAction					OnLinkUnselected;

		//fired only when realMode is false, just after OnNodeProcess is called;
		public event Action						OnPostProcess;

		//event relay, to simplify custom graph events:
		// public event Action< int >				OnChunkSizeChanged;
		// public event Action< int >				OnChunkPositionChanged;

		//TODO: send graphRef.RaiseOnNodeSelected when the node select itself

		//default notify reload will be sent to all node childs.
		public void NotifyReload()
		{
			var nodes = graphRef.GetNodeChildsRecursive(this);

			foreach (var node in nodes)
				node.Reload(this);
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
			OnReload(from);
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

		public void OnAfterDeserialize(PWGraph graph)
		{
			this.graphRef = graph;
			
			foreach (var anchorField in inputAnchorFields)
				anchorField.OnAfterDeserialize(this);
			foreach (var anchorField in outputAnchorFields)
				anchorField.OnAfterDeserialize(this);
		}

	#region OnEnable, OnDisable, Initialize

		public void OnEnable()
		{
			LoadAssets();

			LoadStyles();
			
			LoadFieldAttributes();

			BakeNodeFields();

			BindEvents();

			UpdateWorkStatus();

			//set the PWGUI current node:
			PWGUI.SetNode(this);

			//load the class QA name:
			classQAName = GetType().AssemblyQualifiedName;

			delayedChanges.Clear();
			
			OnNodeEnable();

			OnAnchorEnable();
		}

		//called only once, when the node is created
		public void Initialize(PWGraph graph)
		{
			//generate "unique" id for node:
			byte[] bytes = System.Guid.NewGuid().ToByteArray();
			id = (int)bytes[0] | (int)bytes[1] << 8 | (int)bytes[2] << 16 | (int)bytes[3] << 24;

			//set the graph reference:
			graphRef = graph;

			OnNodeCreation();
		}

		public void OnDisable()
		{
			UnBindEvents();
			OnNodeDisable();
			OnAnchorDisable();
		}

	#endregion

	#region inheritence virtual API

		public virtual void OnNodeCreation() {}

		public virtual void OnNodeEnable() {}

		public virtual void OnNodeProcess() {}

		public virtual void OnNodeDisable() {}

		public virtual void OnNodeDelete() {}

		public virtual void OnNodeProcessOnce() {}

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

		void		DisableUnlinkableAnchors(PWNodeLink link)
		{
			List< PWAnchorField >	anchorFields;
			PWAnchor				anchor;

			if (link.fromAnchor != null)
			{
				anchorFields = inputAnchorFields;
				anchor = link.toAnchor;
			}
			else
			{
				anchorFields = outputAnchorFields;
				anchor = link.fromAnchor;
			}
			
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
	
		bool		UpdateWorkStatus()
		{
			foreach (var anchorField in inputAnchorFields)
				if (anchorField.required)
				{
					if (anchorField.anchors.Count < anchorField.minMultipleValues)
						return false;
					
					foreach (var anchor in anchorField.anchors)
						if (anchor.linkCount == 0)
							return false;
				}
			return true;
		}

		public void	RemoveSelf()
		{
			RemoveAllLinks();
			Destroy(this);
		}

		void		OnClickedOutside()
		{
			if (Event.current.button == 0)
			{
				windowNameEdit = false;
				GUI.FocusControl(null);
			}
			if (Event.current.button == 0 && !Event.current.shift)
				selected = false;
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
