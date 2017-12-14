using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using PW.Node;
using UnityEditor;

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

	[System.Serializable]
	public enum PWTransferType
	{
		Reference,
		Copy,
	}

	[System.SerializableAttribute]
	public class PWAnchorField
	{
		//node instance where the anchor is.
		public PWNode						nodeRef;
		public PWGraph						graphRef { get { return nodeRef.graphRef; } }
		
		//list of rendered anchors:
		public List< PWAnchor >				anchors = new List< PWAnchor >();

		//name of the attached propery / name specified in PW I/O.
		public string						fieldName;
		//name given in the constructor of the attribute
		public string						name;
		//anchor type (input / output)
		public PWAnchorType					anchorType;
		//anchor field type
		public SerializableType				fieldType;
		//value of the field
		public object						fieldValue;
		//debug mode:
		public bool							debug = false;
		//anchor transfer type (when input datas are sent to this anchor)
		public PWTransferType				transferType = PWTransferType.Reference;

		//the visual offset of the anchor
		public int							offset = 0;
		//the visual padding between multiple anchor of the same field
		public int							padding = 0;
		//color palette of the anchor (by default)
		public PWColorSchemeName			colorSchemeName;
		//anchor custom color if set
		public Color						color = new Color(0, 0, 0, 0); //SerializableColor needed ?

		//properties for multiple anchors:
		//is this anchor a multiple anchor
		public bool							multiple = false;
		//allowed type input for multiple anchor
		public SerializableType[]			allowedTypes;
		//min allowed input values
		public int							minMultipleValues = 1;
		//max allowed values
		public int							maxMultipleValues = 1;

		//if the anchor value is required to compute result
		public bool							required = true;
		//if the anchor is selected
		public bool							selected = false;
		//if the anchor is linked to a field
		[System.NonSerialized]
		public bool							fieldValidated = false;

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

			foreach (var link in links)
				nodeRef.RemoveLink(link);
			anchors.RemoveAt(index);
		}

		public PWAnchor CreateNewAnchor()
		{
			PWAnchor	newAnchor = new PWAnchor();

			Debug.LogWarning("anchor create called !");
			newAnchor.Initialize(this);
			anchors.Add(newAnchor);
			return newAnchor;
		}

		public PWAnchor	GetAnchorFromGUID(string anchorGUID)
		{
			return anchors.FirstOrDefault(a => a.GUID == anchorGUID);
		}

		List< string >	deserializedAnchors = new List< string >();

		public void		OnAfterDeserialize(PWNode node)
		{
			if (anchors.Count == 0)
				Debug.LogWarning("Zero length anchors in anchorField in node " + nodeRef);
			
			Init(node);

			foreach (var anchor in anchors)
			{
				deserializedAnchors.Add(anchor.GUID);
				anchor.OnAfterDeserialized(this);
			}


			//add the number of entries corresponding to the number of anchors in the PWValues field
			/*if (multiple)
			{
				Debug.Log("fildValue: " + fieldValue);

				PWValues vs = (PWValues)fieldValue;

				vs.Clear();

				for (int i = 0; i < anchors.Count; i++)
					vs.AssignAt(i, null, fieldName, true);
			}*/
		}

	#region Anchor style
		
		[System.NonSerialized]
		static bool				styleLoaded = false;
		static Texture2D		errorIcon = null;
		static Texture2D		anchorTexture = null;
		static Texture2D		anchorDisabledTexture = null;
		
		static GUIStyle			inputAnchorLabelStyle = null;
		static GUIStyle			outputAnchorLabelStyle = null;
		static GUIStyle			boxAnchorStyle = null;

	#endregion

	#region Anchor rendering and event processing

		public void OnEnable()
		{
		}

		//called only when the anchorField is created
		public void Initialize(PWNode node)
		{
			Init(node);
		}

		//called each time the anchorField is deserialized;
		public void Init(PWNode node)
		{
			nodeRef = node;
		}

		public void LoadStylesAndAssets()
		{
			if (styleLoaded)
				return ;
			
			//styles:
			boxAnchorStyle = new GUIStyle(GUI.skin.box);
			boxAnchorStyle.padding = new RectOffset(0, 0, 1, 1);
			anchorTexture = GUI.skin.box.normal.background;
			anchorDisabledTexture = GUI.skin.box.active.background;
			inputAnchorLabelStyle = GUI.skin.FindStyle("InputAnchorLabel");
			outputAnchorLabelStyle = GUI.skin.FindStyle("OutputAnchorLabel");
			
			//assets:
			errorIcon = Resources.Load< Texture2D >("ic_error");
		}

		public void OnDisable() {}

		Dictionary< PWAnchorHighlight, Color > highlightModeToColor = new Dictionary< PWAnchorHighlight, Color > {
			{ PWAnchorHighlight.AttachAdd, Color.green },
			{ PWAnchorHighlight.AttachNew, Color.blue },
			{ PWAnchorHighlight.AttachReplace, Color.yellow },
		};

		//the anchor passed to ths function must be in the `anchors` list
		void RenderAnchor(PWAnchor anchor, ref Rect renderRect, int index)
		{
			//visual parameters for anchors:
			Vector2		anchorSize = new Vector2(13, 13);
			Vector2		margin = new Vector2(0, 2);

			if (anchor.forcedY != -1)
				renderRect.yMin = anchor.forcedY;
			
			anchor.rect = new Rect(renderRect.min + margin, anchorSize);

			//anchor name:
			string anchorName = name;

			if (!string.IsNullOrEmpty(name) && anchors.Count > 1)
				anchorName += " #" + index;

			//anchor color:
			if (anchor.color != new Color(0, 0, 0, 0))
				GUI.color = anchor.color;
			else
				GUI.color = PWColorTheme.GetAnchorColor(anchor.colorSchemeName);


			//highlight mode to GUI color:
			if (graphRef.editorEvents.isDraggingLink || graphRef.editorEvents.isDraggingNewLink)
			{
				if (anchor.highlighMode != PWAnchorHighlight.None)
				{
					if (anchor.isLinkable)
						GUI.color = highlightModeToColor[anchor.highlighMode];
					else
						GUI.color = PWColorTheme.disabledAnchorColor;
				}
			}
			// GUI.DrawTexture(singleAnchor.anchorRect, anchorDisabledTexture); //???

			//Draw the anchor:
			GUI.DrawTexture(anchor.rect, anchorTexture, ScaleMode.ScaleToFit);
			
			//Draw the anchor name if not null
			if (!string.IsNullOrEmpty(name))
			{
				Rect	anchorNameRect = anchor.rect;
				Vector2 textSize = GUI.skin.label.CalcSize(new GUIContent(anchorName));
				if (anchorType == PWAnchorType.Input)
					anchorNameRect.position += new Vector2(-6, -2);
				else
					anchorNameRect.position += new Vector2(-textSize.x + 4, -2);
				anchorNameRect.size = textSize + new Vector2(15, 4); //add the anchorLabel size
				GUI.depth = 10;
				GUI.Label(anchorNameRect, anchorName, (anchorType == PWAnchorType.Input) ? inputAnchorLabelStyle : outputAnchorLabelStyle);
			}

			//error display (required unlinked anchors)
			if (anchor.visibility == PWVisibility.Visible
					&& required
					&& anchor.anchorType == PWAnchorType.Input
					&& anchor.linkCount == 0)
			{
				Rect errorIconRect = new Rect(anchor.rect);
				errorIconRect.size = Vector2.one * 17;
				errorIconRect.position += new Vector2(-6, -10);
				GUI.color = Color.red;
				GUI.DrawTexture(errorIconRect, errorIcon);
				GUI.color = Color.white;
			}
			
			//debug:
			if (anchor.debug)
			{
				GUIContent debugContent = new GUIContent("c:" + anchor.linkCount + "|i:" + anchor.fieldIndex);

				Rect debugRect = anchor.rect;
					debugRect.xMax += 50;
				if (anchor.anchorType == PWAnchorType.Output)
					debugRect.x -= EditorStyles.label.CalcSize(debugContent).x;
				EditorGUI.DrawRect(debugRect, Color.white * .8f);
				GUI.Label(debugRect, debugContent);
			}
		}

		public void Render(ref Rect renderRect)
		{
			int index = 0;
			int	anchorDefaultPadding = 18;

			renderRect.y += offset;

			foreach (var anchor in anchors)
			{
				//render anchor if visible and linkable
				if (anchor.visibility == PWVisibility.Visible && anchor.isLinkable)
					RenderAnchor(anchor, ref renderRect, index++);

				//if anchor is not gone, increment the padding for next anchor
				if (anchor.visibility != PWVisibility.Gone)
					renderRect.y += padding + anchorDefaultPadding;
			}
		}

		//disable anchors which are unlinkable with the anchor in parameter
		public void DisableIfUnlinkable(PWAnchor anchorToLink)
		{
			foreach (var anchor in anchors)
			{
				if (!PWAnchorUtils.AnchorAreAssignable(anchorToLink, anchor))
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

		public void ProcessEvent(ref PWGraphEditorEventInfo editorEvents)
		{
			var e = Event.current;
			bool contains = false;

			foreach (var anchor in anchors)
				if (anchor.rect.Contains(e.mousePosition))
				{
					editorEvents.mouseOverAnchor = anchor;
					editorEvents.isMouseOverAnchorFrame = true;
					if (e.type == EventType.MouseDown && e.button == 0)
						editorEvents.isMouseClickOnAnchor = true;
					contains = true;
				}
			
			//clean anchor field if the old anchor was in this anchorField
			if (!contains)
			{
				if (anchors.Contains(editorEvents.mouseOverAnchor))
					editorEvents.mouseOverAnchor = null;
			}
		}

	#endregion
	}
}
