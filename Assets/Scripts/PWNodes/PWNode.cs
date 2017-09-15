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
		protected bool			isDragged = false;
		protected bool			isDependent { get; private set; }
		protected bool			realMode { get { return graphRef.IsRealMode(); } }


		//Graph datas accessors:
		protected Vector3		chunkPosition { get { return mainGraphRef.chunkPosition; } }
		protected int			chunkSize { get { return mainGraphRef.chunkSize; } }
		protected int			seed { get { return graphRef.seed; } }
		protected float			step { get { return mainGraphRef.step; } }


		//Debug variables:
		[SerializeField]
		protected bool			nodeDebug = false;
		[SerializeField]
		protected bool			linkDebug = false;
		[SerializeField]
		protected bool			anchorDebug = false;

	#region Internal Node datas and style
		static GUIStyle 		renameNodeTextFieldStyle = null;
		static GUIStyle			debugStyle = null;
		public GUIStyle			windowStyle;
		public GUIStyle			windowSelectedStyle;
		public static GUIStyle	innerNodePaddingStyle = null;
		
		static Texture2D		editIcon = null;

		// static Color		anchorAttachAddColor = new Color(.1f, .1f, .9f);
		// static Color		anchorAttachNewColor = new Color(.1f, .9f, .1f);
		// static Color		anchorAttachReplaceColor = new Color(.9f, .1f, .1f);

		Vector2					graphPan { get { return graphRef.panPosition; } }
		[SerializeField]
		int						maxAnchorRenderHeight = 0;

		[NonSerialized]
		protected DelayedChanges	delayedChanges = new DelayedChanges();

		[System.NonSerialized]
		public PWGraph			graphRef;
		public PWMainGraph		mainGraphRef { get { return graphRef as PWMainGraph; } }
		public PWBiomeGraph		biomeGraphRef { get { return graphRef as PWBiomeGraph; } }
		//TODO: data and mesh graphs

		public bool				windowNameEdit = false;
		public bool				selected = false;

	#endregion
	
		public delegate void					ReloadAction(PWNode from);
		public delegate void					NodeLinkAction(PWAnchor anchor);
		public delegate void					MessageReceivedAction(PWNode from, object message);
		public delegate void					LinkAction(PWNodeLink link);

		//fired when the node received a NotifyReload() or the user pressed Reload button in editor.
		protected event ReloadAction			OnReload;
		//fired when the node receive a SendMessage()
		protected event MessageReceivedAction	OnMessageReceived;

		//fired when this node was linked
		protected event NodeLinkAction			OnAnchorLinked;
		//fired when this node was unlinked
		protected event NodeLinkAction			OnAnchorUnlinked;
		//fired when the dragged link is above an anchor
		protected event NodeLinkAction			OnDraggedLinkOverAnchor;
		//fired when the dragged link quit the zone above the anchor
		protected event NodeLinkAction			OnDraggedLinkQuitAnchor;

		//fired when a link is selected
		protected event LinkAction				OnLinkSelected;
		//fired when a link is unselected
		protected event LinkAction				OnLinkUnselected;

		//TODO: send graphRef.RaiseOnNodeSelected when the node select itself

		//default notify reload will be sent to all node childs.
		public void NotifyReload()
		{
			var nodes = graphRef.GetNodeChildsRecursive(this);

			foreach (var node in nodes)
				node.OnReload(this);
		}
		
		//send reload event to all node of the specified type
		public void NotifyReload(Type targetType)
		{
			var nodes = from node in graphRef.nodes
						where node.GetType() == targetType
						select node;
			
			foreach (var node in nodes)
				node.OnReload(this);
		}

		//send reload to all nodes with a computeOrder smaller than minComputeOrder.
		public void NotifyReload(int minComputeOrder)
		{
			var nodes = from node in graphRef.nodes
						where node.computeOrder >= minComputeOrder
						select node;

			foreach (var node in nodes)
				node.OnReload(this);
		}

		public void NotifyReload(PWNode node)
		{
			node.OnReload(this);
		}

		public void NotifyReload(IEnumerable< PWNode > nodes)
		{
			foreach (var node in nodes)
				node.OnReload(this);
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

		public virtual void OnNodeAwake() {}

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
				anchorField.DisableIfUnlinkable(acnhor);
		}

		void		ResetAnchorHighlight()
		{

		}
	
		void		HighlightLinkableAnchorsTo(PWAnchorInfo toLink)
		{
			PWAnchorType anchorType = InverAnchorType(toLink.anchorType);

			ForeachPWAnchors((data, singleAnchor, i) => {
				//Hide anchors and highlight when mouse hover
				// Debug.Log(data.nodeId + ":" + data.fieldName + ": " + AnchorAreAssignable(data.type, data.anchorType, data.generic, data.allowedTypes, toLink, true));
				if (data.nodeId != toLink.nodeId
					&& data.anchorType == anchorType
					&& AnchorAreAssignable(data.type, data.anchorType, data.generic, data.allowedTypes, toLink, false))
				{
					if (singleAnchor.anchorRect.Contains(Event.current.mousePosition))
						if (singleAnchor.visibility == PWVisibility.Visible)
						{
							singleAnchor.highlighMode = PWAnchorHighlight.AttachNew;
							if (singleAnchor.linkCount > 0)
							{
								//if anchor is locked:
								if (data.anchorType == PWAnchorType.Input)
									singleAnchor.highlighMode = PWAnchorHighlight.AttachReplace;
								else
									singleAnchor.highlighMode = PWAnchorHighlight.AttachAdd;
							}
						}
				}
			});
		}
		
		public void		DisplayHiddenMultipleAnchors(bool display = true)
		{
			ForeachPWAnchorDatas((data) => {
				if (data.multiple)
					data.displayHiddenMultipleAnchors = display;
			});
		}

		public bool		CheckRequiredAnchorLink()
		{
			bool	ret = true;

			ForeachPWAnchors((data, singleAnchor, i) => {
				if (data.required && singleAnchor.linkCount == 0
						&& (!data.multiple || (data.multiple && i < data.minMultipleValues)))
					ret = false;
			});
			return ret;
		}

		public void OnClickedOutside()
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

		public void AnchorBeingLinked(int anchorId)
		{
			ForeachPWAnchors((data, singleAnchor, i) => {
				if (singleAnchor.id == anchorId)
				{
					if (singleAnchor.linkCount == 0)
						singleAnchor.highlighMode = PWAnchorHighlight.AttachNew;
					else
						if (data.anchorType == PWAnchorType.Input)
							singleAnchor.highlighMode = PWAnchorHighlight.AttachReplace;
						else
							singleAnchor.highlighMode = PWAnchorHighlight.AttachAdd;
				}
			});
		}

	#region Unused (for the moment) overrided functions
		public void OnDestroy()
		{
			// Debug.Log("node " + nodeTypeName + " detroyed !");
		}

		public void OnGUI()
		{
			EditorGUILayout.LabelField("You are in the wrong window !");
		}
		
		public void OnInspectorGUI()
		{
			EditorGUILayout.LabelField("nope !");
		}
	#endregion

		void			ForeachPWAnchors(Action< PWAnchorData, PWAnchorData.PWAnchorMultiData, int > callback, bool showAdditional = false, bool instanceValueCount = true)
		{
			foreach (var PWAnchorData in propertyDatas)
			{
				var data = PWAnchorData.Value;
				if (data.multiple)
				{
					if (data.anchorInstance == null)
					{
						if (!bakedNodeFields.ContainsKey(data.fieldName))
						{
							Debug.LogError("key not found in bakedNodeFields: " + data.fieldName + " in node " + GetType());
							continue ;
						}
						data.anchorInstance = bakedNodeFields[data.fieldName].GetValue(this);
						if (data.anchorInstance == null)
							continue ;
						else
							data.multipleValueCount = (data.anchorInstance as PWValues).Count;
					}

					int anchorCount;
					if (instanceValueCount && !data.forcedAnchorNumber)
						anchorCount = Mathf.Max(data.minMultipleValues, ((PWValues)data.anchorInstance).Count);
					else
						anchorCount = data.multi.Count;
					if (data.anchorType == PWAnchorType.Input && !data.forcedAnchorNumber)
						if (data.displayHiddenMultipleAnchors || showAdditional)
							anchorCount++;
					for (int i = 0; i < anchorCount; i++)
					{
						// Debug.Log("i = " + i + ", anchor: " + data.fieldName + ", " + data.forcedAnchorNumber + ", " + GetType());
						//if multi-anchor instance does not exists, create it:
						if (data.displayHiddenMultipleAnchors && i == anchorCount - 1)
							data.multi[i].additional = true;
						else
							data.multi[i].additional = false;
						callback(data, data.multi[i], i);
					}
				}
				else
					callback(data, data.first, -1);
			}
		}

		void 			ForeachPWAnchorDatas(Action< PWAnchorData > callback)
		{
			foreach (var data in propertyDatas)
			{
				if (data.Value != null)
					callback(data.Value);
			}
		}

		public override string ToString()
		{
			return "node " + name + "[" + GetType() + "]";
		}
    }
}
