using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralWorlds.Core
{
	[System.SerializableAttribute]
	public class Anchor
	{
		//GUID of the anchor, used to identify anchors in NodeLinkTable
		public string				GUID;


		//AnchorField instance attached to this anchor
		[System.NonSerialized]
		public AnchorField		anchorFieldRef;
		//Node instance attached to this anchor
		[System.NonSerialized]
		public BaseNode				nodeRef;

		//anchor connections:
		[System.NonSerialized]
		public List< NodeLink >	links = new List< NodeLink >();
		//list of link GUIDs
		public List< string >		linkGUIDs = new List< string >();


		//anchor name
		public string				name;
		//enabled ?
		public bool					enabled = true;
		//number of links connected to this anchor
		public int					linkCount { get { return links.Count; } }
		//index of the field, valid only if the attached field is a PWArray
		public int					fieldIndex = -1;
		//Contains the type in the PWArray at fieldIndex or anchorField.fieldType if field is not a PWArray
		[SerializeField] SerializableType	_fieldType;
		public SerializableType		fieldType { get { return (_fieldType == null) ? anchorFieldRef.fieldType : _fieldType; } set { _fieldType = value; } }


		//hightlight mode (for replace / new / delete link visualization)
		public AnchorHighlight	highlighMode = AnchorHighlight.None;
		//visual rect of the anchor (from node)
		public Rect					rect;
		//visual rect of the anchor (from the graph)
		public Rect					rectInGraph { get { return Utils.DecalRect(rect, nodeRef.rect.position + nodeRef.graphRef.panPosition); } }
		//anchor color
		public Color				color; //no need of SerializableColor ?
		//anchor visibility
		public Visibility			visibility = Visibility.Visible;
		//override default y anchor position
		public float				forcedY = -1;


		//anchor field accessors
		public AnchorType			anchorType { get { return anchorFieldRef.anchorType; } }
		public string				fieldName { get { return anchorFieldRef.fieldName; } }
		public ColorSchemeName	colorSchemeName { get { return anchorFieldRef.colorSchemeName; } }
		public bool					required { get { return anchorFieldRef.required; } }

		
		//Editor variable:
		[System.NonSerialized]
		public bool					isLinkable = true;

		//debug variables:
		public bool					debug;

		public void OnAfterDeserialized(AnchorField anchorField)
		{
			Init(anchorField);

			//we use the LinkTable in the graph to get the only instance of link stored
			//	to know why, take a look at the BaseGraph.cs file.
			var nodeLinkTable = nodeRef.graphRef.nodeLinkTable;

			//only used when a part of a link was not well destroyed, technically never
			var linkToRemove = new List< string >();

			//here we set the anchor references in the link cauz they can't be serialized.
			foreach (var linkGUID in linkGUIDs)
			{
				var linkInstance = nodeLinkTable.GetLinkFromGUID(linkGUID);
			
				//if link does not exists, skip it and add it to the remove list
				if (linkInstance == null)
				{
					linkToRemove.Add(linkGUID);
					continue ;
				}
				
				links.Add(linkInstance);
			}

			//removes ghost links (normally never appends)
			foreach (var linkGUID in linkToRemove)
			{
				Debug.LogError("[Anchor] Removing link GUID " + linkGUID + " from the link list cauz it was destroyed");

				linkGUIDs.Remove(linkGUID);
			}
			
			//propagate the OnAfterDeserialize event.
			foreach (var link in links)
				link.OnAfterDeserialize(this);
		}

		//remove the link reference stored inside this anchor
		public void RemoveLinkReference(NodeLink link)
		{
			if (!linkGUIDs.Remove(link.GUID))
				Debug.LogError("[Anchor] Tried to remove inexistant link GUID: " + link.GUID);
			if (!links.Remove(link))
				Debug.LogError("[Anchor] Tried to remove link: " + link);
			
			//set to null the object value if we're not anymore linked
			if (anchorType == AnchorType.Input && linkCount == 0)
				nodeRef.SetAnchorValue(this, null);
			
			//update anchor group
			anchorFieldRef.UpdateAnchors();
		}
		
		//remove all links attached to this anchor
		public void RemoveAllLinks()
		{
			int count = links.Count;

			for (int i = 0; i < count; i++)
				nodeRef.graphRef.RemoveLink(links[0]);
			
			//update anchorFieldRef (for multiple anchors)
			anchorFieldRef.UpdateAnchors();
		}

		public void AddLink(NodeLink link)
		{
			linkGUIDs.Add(link.GUID);
			links.Add(link);

			//update anchorFieldRef (for multiple anchors)
			anchorFieldRef.UpdateAnchors();
		}

		//called only once (when the anchor is created)
		public void Initialize(AnchorField anchorField)
		{
			GUID = System.Guid.NewGuid().ToString();
			Init(anchorField);

			//if we can have multiple anchors, we set the fieldIndex value
			if (anchorField.multiple)
				fieldIndex = anchorField.anchors.Count;
		}

		void Init(AnchorField anchorField)
		{
			if (anchorField == null)
			{
				Debug.LogWarning("Null anchor field passed to Anchor at deserialization");
				return ;
			}
			
			anchorFieldRef = anchorField;
			color = anchorField.color;
			nodeRef = anchorField.nodeRef;
		}

		override public string ToString()
		{
			return anchorType + " Anchor [" + GUID + "], node: " + nodeRef;
		}
	}
}