using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using ProceduralWorlds.Core;
using ProceduralWorlds.Node;

namespace ProceduralWorlds
{
	[System.SerializableAttribute]
	public partial class BaseNode : ScriptableObject
	{
		public static readonly Rect	defaultNodeSize = new Rect(0, 0, 200, 50);

		//Node datas:
		public Rect					rect = defaultNodeSize;
		public Rect					visualRect;
		public int					id;
		public bool					renamable;
		public int					computeOrder;
		public float				processTime;
		public string				classAQName;
		public ColorSchemeName		colorSchemeName;
		public int					viewHeight;
		new public string			name;

		//AnchorField lists
		[System.NonSerialized]
		public List< AnchorField >	inputAnchorFields = new List< AnchorField >();
		[System.NonSerialized]
		public List< AnchorField >	outputAnchorFields = new List< AnchorField >();
		public IEnumerable< AnchorField > anchorFields { get { foreach (var ia in inputAnchorFields) yield return ia; foreach (var ao in outputAnchorFields) yield return ao; } }


		//Useful state bools:
		protected bool			realMode { get { return graphRef.IsRealMode(); } }
		[System.NonSerialized]
		public bool				ready;
		//Tell if the node have required unlinked input and so can't Process()
		public bool				canWork;
		//Is the node processing
		public bool				isProcessing;
		//Tell if the node was enabled
		[System.NonSerialized]
		private bool			isEnabled;
		//Is the node selected
		public bool				isSelected;
		//Is the node dragged
		public bool				isDragged;
		//Serialization system:
		[System.NonSerialized]
		private	bool			deserializationAlreadyNotified;
		//GUI option storage
		public ProceduralWorldsGUIStorage	PWGUIStorage = new ProceduralWorldsGUIStorage();


		[System.NonSerialized]
		public BaseGraph		graphRef;
		protected WorldGraph	worldGraphRef { get { return graphRef as WorldGraph; } }
		protected BiomeGraph	biomeGraphRef { get { return graphRef as BiomeGraph; } }


		//Graph datas accessors:
		protected Vector3		chunkPosition { get { return worldGraphRef.chunkPosition; } }
		protected int			chunkSize { get { return worldGraphRef.chunkSize; } }
		protected int			seed { get { return worldGraphRef.seed; } }
		protected float			step { get { return worldGraphRef.step; } }


		//Debug variables:
		[SerializeField]
		public bool				debug = false;
	
		public delegate void	AnchorAction(Anchor anchor);

		//fired only when realMode is false, just after OnNodeProcess is called;
		public event Action		OnPostProcess;


		public void OnAfterGraphDeserialize(BaseGraph graph)
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

			LoadClonableFields();

			UpdateAnchorProperties();

			if (debug)
				Debug.Log("Node OnEnable: " + GetType());

			//load the class QA name:
			classAQName = GetType().AssemblyQualifiedName;

			if (graphRef != null)
			{
				OnAfterNodeAndGraphDeserialized();
			}
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

			//tell to the graph that this node is ready to work
			this.ready = true;

			if (alertGraph)
				graphRef.NotifyNodeReady(this);
		}

		//called only once, when the node is created
		public void Initialize(BaseGraph graph)
		{
			if (debug)
				Debug.LogWarning("Node " + GetType() + "Initialize !");

			//generate "unique" id for node:
			byte[] bytes = System.Guid.NewGuid().ToByteArray();
			id = (int)bytes[0] | (int)bytes[1] << 8 | (int)bytes[2] << 16 | (int)bytes[3] << 24;

			//set the node name:
			name = NodeTypeProvider.GetNodeName(GetType());

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
			if (debug)
				Debug.Log("Node " + GetType() + " Disable");
			
			UnBindEvents();

			//if the node was properly enabled, we call it's onDisable functions
			if (graphRef != null)
				OnNodeDisable();
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

		AnchorField	CreateAnchorField()
		{
			AnchorField newAnchorField = new AnchorField();

			newAnchorField.Initialize(this);

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
						if (anchor.visibility != Visibility.Visible)
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
			if (!realMode && OnPostProcess != null)
				OnPostProcess();
			isProcessing = false;
		}

		public override string ToString()
		{
			return "node " + name + "<" + GetType() + ">(" + id + ")";
		}
    }
}
