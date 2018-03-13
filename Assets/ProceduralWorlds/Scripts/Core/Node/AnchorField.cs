using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProceduralWorlds.Node;
using System;

namespace ProceduralWorlds.Core
{
	[System.Serializable]
	public enum AnchorType
	{
		None,
		Input,
		Output,
	}

	[System.Serializable]
	public enum AnchorHighlight
	{
		None,
		AttachNew,				//link will be attached to unlinked anchor
		AttachReplace,			//link will replace actual anchor link
		AttachAdd,				//link will be added to anchor links
	}

	[System.Serializable]
	public class AnchorField
	{
		//node instance where the anchor is.
		public BaseNode						nodeRef;
		public BaseGraph						graphRef { get { return nodeRef.graphRef; } }
		
		//list of rendered anchors:
		public List< Anchor >				anchors = new List< Anchor >();

		//name of the attached propery / name specified in PW I/O.
		public string						fieldName;
		//name given in the constructor of the attribute
		public string						name;
		//anchor type (input / output)
		public AnchorType					anchorType = AnchorType.None;
		//anchor field type
		public SerializableType				fieldType;
		//value of the field
		public object						fieldValue;
		//debug mode:
		public bool							debug = false;

		//the visual offset of the anchor
		public int							offset = 0;
		//the visual padding between multiple anchor of the same field
		public int							padding = 0;
		//color palette of the anchor (by default)
		public ColorSchemeName			colorSchemeName;
		//anchor custom color if set
		public Color						color = new Color(0, 0, 0, 0); //SerializableColor needed ?

		//properties for multiple anchors:
		//is this anchor a multiple anchor
		public bool							multiple = false;

		//if the anchor value is required to compute result
		public bool							required = true;
		//if the anchor is selected
		public bool							selected = false;
		//if the anchor is linked to a field
		[System.NonSerialized]
		public bool							fieldValidated = false;
		
		//called only when the anchorField is created
		public void Initialize(BaseNode node)
		{
			Init(node);
		}

		//called each time the anchorField is deserialized;
		public void Init(BaseNode node)
		{
			nodeRef = node;
		}

		public void		RemoveAnchor(string GUID)
		{
			int index = anchors.FindIndex(a => a.GUID == GUID);

			if (index == -1)
				Debug.LogWarning("Failed to remove the anchor at GUID: " + GUID);
			RemoveAnchor(index);
		}

		public void		RemoveAnchor(int index)
		{
			var links = anchors[index].links;

			while (links.Count > 0)
				nodeRef.RemoveLink(links.First());
			anchors.RemoveAt(index);
		}

		public Anchor CreateNewAnchor()
		{
			Anchor	newAnchor = new Anchor();

			newAnchor.Initialize(this);
			anchors.Add(newAnchor);
			return newAnchor;
		}

		public Anchor	GetAnchorFromGUID(string anchorGUID)
		{
			return anchors.FirstOrDefault(a => a.GUID == anchorGUID);
		}

		List< string >	deserializedAnchors = new List< string >();

		public void		OnAfterDeserialize(BaseNode node)
		{
			if (anchors.Count == 0)
				Debug.LogWarning("Zero length anchors in anchorField in node " + nodeRef);
			
			Init(node);

			foreach (var anchor in anchors)
			{
				deserializedAnchors.Add(anchor.GUID);
				anchor.OnAfterDeserialized(this);
			}

			UpdateAnchors();
		}

		public override string ToString()
		{
			return "{d:" + anchorType + ", node: " + nodeRef + ", field: " + fieldName + "}";
		}

		#region AnchorField API

		//Update multiple anchor count, must be called when a link is created/removed on this anchorField
		public void UpdateAnchors()
		{
			//if this anchor field is a multiple input, check if all our anchors are linked
			// and if so, add a new one
			if (multiple && anchorType == AnchorType.Input)
			{
				if (anchors.All(a => a.linkCount > 0))
					CreateNewAnchor();

				//if there are more than 1 unlinked anchor, delete the others:
				if (anchors.Count(a => a.linkCount == 0) > 1)
					RemoveDuplicatedUnlinkedAnchors();
			}
		}

		void RemoveDuplicatedUnlinkedAnchors()
		{
			bool first = true;
			List< string > anchorsToRemove = new List< string >();

			foreach (var anchor in anchors)
				if (anchor.linkCount == 0)
				{
					if (first)
						first = false;
					else
						anchorsToRemove.Add(anchor.GUID);
				}
			foreach (var guid in anchorsToRemove)
				RemoveAnchor(guid);
		}

		//disable anchors which are unlinkable with the anchor in parameter
		public void DisableIfUnlinkable(Anchor anchorToLink)
		{
			foreach (var anchor in anchors)
			{
				if (!AnchorUtils.AnchorAreAssignable(anchorToLink, anchor))
					anchor.isLinkable = false;
			}
		}

		//disable all anchor to mark them as unlinkable.
		public void DisableLinkable()
		{
			foreach (var anchor in anchors)
				anchor.isLinkable = false;
		}

		//reset the anchor state, re-enable it if it was disable by DisableIfUnlinkable()
		public void ResetLinkable()
		{
			foreach (var anchor in anchors)
				anchor.isLinkable = true;
		}

		#endregion
	}
}
