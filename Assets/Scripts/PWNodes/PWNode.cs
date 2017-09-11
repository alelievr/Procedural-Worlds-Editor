// #define DEBUG_NODE
// #define DEBUG_ANCHOR
// #define HIDE_ANCHOR_LABEL

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
		public string	nodeTypeName;
		public Rect		windowRect = new Rect(400, 400, 200, 50);
		public int		id;
		public bool		renamable = false;
		public int		computeOrder = 0;
		public int		viewHeight = 0;
		public bool		specialButtonClick = false;
		public bool		isDragged = false;
		public Vector3	chunkPosition = Vector3.zero;
		public int		chunkSize = 16;
		public int		seed;
		public float	step;
		public float	processTime = 0f;
		new public string name;
		[SerializeField]
		public PWGUIManager	PWGUI = new PWGUIManager();

		//useful state bools
		public bool		isDependent { get; private set; }
		public bool		realMode { get { return graph.IsRealMode(); } }

	#region Internal Node datas and style
		static GUIStyle		boxAnchorStyle = null;
		static GUIStyle 	renameNodeTextFieldStyle = null;
		static GUIStyle		inputAnchorLabelStyle = null;
		static GUIStyle		outputAnchorLabelStyle = null;
		#if DEBUG_NODE
		static GUIStyle		debugStyle = null;
		#endif
		public GUIStyle		windowStyle;
		public GUIStyle		windowSelectedStyle;
		public static GUIStyle	innerNodePaddingStyle = null;
		
		static Texture2D	errorIcon = null;
		static Texture2D	editIcon = null;
		static Texture2D	anchorTexture = null;
		static Texture2D	anchorDisabledTexture = null;
		static Texture2D	nodeAutoProcessModeIcon = null;
		static Texture2D	nodeRequestForProcessIcon = null;

		// static Color		anchorAttachAddColor = new Color(.1f, .1f, .9f);
		// static Color		anchorAttachNewColor = new Color(.1f, .9f, .1f);
		// static Color		anchorAttachReplaceColor = new Color(.9f, .1f, .1f);

		[SerializeField]
		Vector2				graphDecal;
		[SerializeField]
		int					maxAnchorRenderHeight = 0;
		[SerializeField]
		string				firstInitialization;

		[NonSerialized]
		protected DelayedChanges	delayedChanges = new DelayedChanges();

		//node links and deps
		[SerializeField]
		List< PWLink >				links = new List< PWLink >();
		[SerializeField]
		List< PWDependency >		depencendies = new List< PWDependency >(); //List< nodeId, anchorId >

		[System.NonSerialized]
		public PWMainGraph		graph;

		[System.NonSerialized]
		public bool		unserializeInitialized = false;

		public bool		windowNameEdit = false;
		public bool		selected = false;

	#endregion
	
		public delegate void			Reload(PWNode from);
		public delegate void			NodeLink();
		public delegate void			MessageReceived(PWNode from, object message);

		private event Reload			OnReload;
		private event MessageReceived	OnMessageReceived;
		private event NodeLink			OnNodeLinked;
		private event NodeLink			OnNodeUnlinked;

		//default notify reload will be sent to all node childs.
		public void NotifyReload()
		{
			var nodes = graph.GetNodeChildsRecursive(this);

			foreach (var node in nodes)
				node.OnReload(this);
		}
		
		//send reload event to all node of the specified type
		public void NotifyReload(Type targetType)
		{
			var nodes = from node in graph.nodes
						where node.GetType() == targetType
						select node;
			
			foreach (var node in nodes)
				node.OnReload(this);
		}

		//send reload to all nodes with a computeOrder smaller than minComputeOrder.
		public void NotifyReload(int minComputeOrder)
		{
			var nodes = from node in graph.nodes
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
			var nodes = from node in graph.nodes
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

	#region OnEnable, OnDisable, data initialization and baking

		public void OnEnable()
		{
			LoadAssets();
			
			LoadFieldAttributes();

			BakeNodeFields();

			BindEvents();

			delayedChanges.Clear();
			
			//Will be called only at the creation of the node.
			if (firstInitialization == null)
			{
				OnNodeCreation();

				firstInitialization = "initialized";
			}

			OnNodeEnable();
		}

		public void OnDisable()
		{
			UnBindEvents();
			OnNodeDisable();
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
	
	#region Editor utils

		static bool			AnchorAreAssignable(Type fromType, PWAnchorType fromAnchorType, bool fromGeneric, SerializableType[] fromAllowedTypes, PWAnchorInfo to, bool verbose = false)
		{
			bool ret = false;

			if ((fromType != typeof(PWValues) && to.fieldType != typeof(PWValues)) //exclude PWValues to simple assignation (we need to check with allowedTypes)
				&& (fromType.IsAssignableFrom(to.fieldType) || fromType == typeof(object) || to.fieldType == typeof(object)))
			{
				if (verbose)
					Debug.Log(fromType.ToString() + " is assignable from " + to.fieldType.ToString());
				return true;
			}

			if (fromGeneric || to.generic)
			{
				if (verbose)
					Debug.Log("from type is generic");
				SerializableType[] types = (fromGeneric) ? fromAllowedTypes : to.allowedTypes;
				Type secondType = (fromGeneric) ? to.fieldType : fromType;
				foreach (Type firstT in types)
					if (fromGeneric && to.generic)
					{
						if (verbose)
							Debug.Log("to type is generic");
							
						if (firstT == typeof(object))
						{
							ret = true;
							break ;
						}

						foreach (Type toT in to.allowedTypes)
						{
							if (verbose)
								Debug.Log("checking assignable from " + firstT + " to " + toT);

							if (toT == typeof(object))
							{
								ret = true;
								break ;
							}

							if (firstT.IsAssignableFrom(toT))
								ret = true;
						}
					}
					else
					{
						if (verbose)
							Debug.Log("checking assignable from " + firstT + " to " + secondType);
						if (firstT.IsAssignableFrom(secondType))
							ret = true;
					}
			}
			else
			{
				if (verbose)
					Debug.Log("non-generic types, checking assignable from " + fromType + " to " + to.fieldType);
				if (fromType.IsAssignableFrom(to.fieldType) || to.fieldType.IsAssignableFrom(fromType))
					ret = true;
			}
			if (verbose)
				Debug.Log("result: " + ret);
			return ret;
		}

		public static bool		AnchorAreAssignable(PWAnchorInfo from, PWAnchorInfo to, bool verbose = false)
		{
			if (from.anchorType == to.anchorType)
				return false;
			return AnchorAreAssignable(from.fieldType, from.anchorType, from.generic, from.allowedTypes, to, verbose);
		}

		public void		HighlightLinkableAnchorsTo(PWAnchorInfo toLink)
		{
			PWAnchorType anchorType = InverAnchorType(toLink.anchorType);

			ForeachPWAnchors((data, singleAnchor, i) => {
				//Hide anchors and highlight when mouse hover
				// Debug.Log(data.nodeId + ":" + data.fieldName + ": " + AnchorAreAssignable(data.type, data.anchorType, data.generic, data.allowedTypes, toLink, true));
				if (data.nodeId != toLink.nodeId
					&& data.anchorType == anchorType
					&& AnchorAreAssignable(data.type, data.anchorType, data.generic, data.allowedTypes, toLink, false))
				{
					if (data.multiple)
					{
						//display additional anchor to attach on next rendering
						data.displayHiddenMultipleAnchors = true;
					}
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
				else if (singleAnchor.visibility == PWVisibility.Visible
					&& singleAnchor.id != toLink.anchorId
					&& singleAnchor.linkCount == 0)
					singleAnchor.visibility = PWVisibility.InvisibleWhenLinking;
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

		public void UpdateGraphDecal(Vector2 graphDecal)
		{
			this.graphDecal = graphDecal;
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

		public void			SetReloadReuqestedNode(PWNode n)
		{
			reloadRequestFromNode = n;
		}

		protected Type		GetReloadRequestType()
		{
			if (reloadRequestFromNode != null)
				return reloadRequestFromNode.GetType();
			return null;
		}

		protected PWNode	GetReloadRequestNode()
		{
			return reloadRequestFromNode;
		}

	#endregion

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

	#region Utils and Miscellaneous

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
			return "[" + GetType() + "] " + externalName;
		}

	#endregion
    }
}
