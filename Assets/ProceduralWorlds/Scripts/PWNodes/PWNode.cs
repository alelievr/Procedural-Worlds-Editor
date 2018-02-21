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
		public Rect					visualRect;
		public int					id;
		public bool					renamable = false;
		public int					computeOrder = 0;
		public float				processTime = 0f;
		public string				classAQName;
		public PWColorSchemeName	colorSchemeName;
		new public string			name;

		//AnchorField lists
		[System.NonSerialized]
		public List< PWAnchorField >	inputAnchorFields = new List< PWAnchorField >();
		[System.NonSerialized]
		public List< PWAnchorField >	outputAnchorFields = new List< PWAnchorField >();
		public IEnumerable< PWAnchorField > anchorFields { get { foreach (var ia in inputAnchorFields) yield return ia; foreach (var ao in outputAnchorFields) yield return ao; } }

		//GUI utils to provide custom fields for Samplers, Range ...
		[SerializeField]
		public PWGUIManager		PWGUI = new PWGUIManager();


		//Useful state bools:
		protected bool			realMode { get { return graphRef.IsRealMode(); } }
		[System.NonSerialized]
		public bool				ready = false;
		//tell if the node have required unlinked input and so can't Process()
		public bool				canWork = false;
		public bool				isProcessing = false;
		//tell if the node was enabled
		[System.NonSerialized]
		private bool			isEnabled = false;
		//Serialization system:
		[System.NonSerialized]
		private	bool			deserializationAlreadyNotified = false;


		[System.NonSerialized]
		public PWGraph			graphRef;
		protected PWMainGraph	mainGraphRef { get { return graphRef as PWMainGraph; } }
		protected PWBiomeGraph	biomeGraphRef { get { return graphRef as PWBiomeGraph; } }


		//Graph datas accessors:
		protected Vector3		chunkPosition { get { return mainGraphRef.chunkPosition; } }
		protected int			chunkSize { get { return mainGraphRef.chunkSize; } }
		protected int			seed { get { return mainGraphRef.seed; } }
		protected float			step { get { return mainGraphRef.step; } }


		//Debug variables:
		[SerializeField]
		public bool				debug = false;
	
		public delegate void					AnchorAction(PWAnchor anchor);

		//fired when this node was linked
		protected event AnchorAction			OnAnchorLinked;
		//fired when this node was unlinked
		protected event AnchorAction			OnAnchorUnlinked;

		//fired only when realMode is false, just after OnNodeProcess is called;
		public event Action						OnPostProcess;

		//event relay, to simplify custom graph events:
		// public event Action< int >				OnChunkSizeChanged;
		// public event Action< int >				OnChunkPositionChanged;

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
			
			LoadFieldAttributes();

			LoadUndoableFields();

			UpdateAnchorProperties();

			if (debug)
				Debug.Log("Node OnEnable: " + GetType());

			//set the PWGUI current node:
			PWGUI.SetNode(this);

			//load the class QA name:
			classAQName = GetType().AssemblyQualifiedName;

			if (graphRef != null)
				OnAfterNodeAndGraphDeserialized();
		}

		//here both our node and graph have been deserialized, we can now use it's datas
		void OnAfterNodeAndGraphDeserialized(bool alertGraph = true)
		{
			if (deserializationAlreadyNotified)
			{
				if (alertGraph)
					graphRef.NotifyNodeReady(this);
				return ;
			}
			
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

			if (alertGraph)
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

			//Initialize the rest of the node
			OnAfterNodeAndGraphDeserialized(false);

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

		public virtual void OnNodeAnchorLink(string propName, int index) {}

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

		PWAnchorField	CreateAnchorField()
		{
			PWAnchorField newAnchorField = new PWAnchorField();

			newAnchorField.Initialize(this);
			newAnchorField.OnEnable();

			return newAnchorField;
		}
	
		//Look for required input anchors and check if there are linked, if not we set the canWork bool to false.
		void		UpdateWorkStatus()
		{
			canWork = false;

			foreach (var anchorField in inputAnchorFields)
			{
				if (anchorField.required && !anchorField.multiple)
				{
					foreach (var anchor in anchorField.anchors)
					{
						if (anchor.visibility != PWVisibility.Visible)
							continue ;
						
						//check if the input anchor is linked and exit if not
						if (anchor.linkCount == 0)
							return ;
					}
				}
			}
			
			canWork = true;
		}
		
		public void Process()
		{
			isProcessing = true;
			OnNodeProcess();
			if (!realMode)
				OnPostProcess();
			isProcessing = false;
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
