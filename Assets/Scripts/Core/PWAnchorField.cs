using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PW.Core
{
	[System.SerializableAttribute]
	public enum PWAnchorType
	{
		Input,
		Output,
		None,
	}

	[System.SerializableAttribute]
	public enum PWAnchorHighlight
	{
		None,
		AttachNew,				//link will be attached to unlinked anchor
		AttachReplace,			//link will replace actual anchor link
		AttachAdd,				//link will be added to anchor links
	}

	[System.SerializableAttribute]
	public class PWAnchorField
	{
		//node instance where the anchor is.
		public PWNode						nodeRef;
		
		//list of rendered anchors:
		public List< PWAnchor >				anchors = new List< PWAnchor >();
		
		//instance of the field
		public object						fieldInstance;

		//name of the attached propery / name specified in PW I/O.
		public string						fieldName;
		//anchor type (input / output)
		public PWAnchorType					anchorType;
		//anchor field type
		public SerializableType				fieldType;

		//anchor name if specified in PWInput or PWOutput else null
		public string						anchorName;
		//the visual offset of the anchor
		public Vector2						offset;
		//the visual padding between multiple anchor of the same field
		public int							multiPadding;

		//properties for multiple anchors:
		public SerializableType[]			allowedTypes;
		//min allowed input values
		public int							minMultipleValues;
		//max allowed values
		public int							maxMultipleValues;

		//if the anchor value is required to compute result
		public bool							required;
		//if the anchor is selected
		public bool							selected;

		public void RemoveAllAnchors()
		{
			anchors.Clear();
		}

		public void OnAfterDeserialize(PWNode node)
		{
			nodeRef = node;

			foreach (var anchor in anchors)
				anchor.OnBeforeDeserialized(this);
		}
	}
}
