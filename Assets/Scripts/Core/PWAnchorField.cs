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
		//name given in the constructor of the attribute
		public string						name;
		//anchor type (input / output)
		public PWAnchorType					anchorType;
		//anchor field type
		public SerializableType				fieldType;

		//anchor name if specified in PWInput or PWOutput else null
		public string						anchorName;
		//the visual offset of the anchor
		public int							offset;
		//the visual padding between multiple anchor of the same field
		public int							padding;

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

		public void		RemoveAnchor(string GUID)
		{
			if (anchors.RemoveAll(a => a.GUID == GUID) != 1)
				Debug.LogWarning("Failed to remove the anchor at GUID: " + GUID);
		}

		public PWAnchor CreateNewAnchor()
		{
			PWAnchor	newAnchor = new PWAnchor();

			newAnchor.Initialize(this);
			anchors.Add(newAnchor);
			return newAnchor;
		}

		public PWAnchor	GetAnchorFromGUID(string anchorGUID)
		{
			return anchors.FirstOrDefault(a => a.GUID == anchorGUID);
		}

		public void		OnAfterDeserialize(PWNode node)
		{
			Initialize(node);

			foreach (var anchor in anchors)
				anchor.OnAfterDeserialized(this);
		}

	#region Anchor style
		
		static bool				styleLoaded = false;
		static Texture2D		errorIcon = null;
		static Texture2D		anchorTexture = null;
		static Texture2D		anchorDisabledTexture = null;
		
		static GUIStyle			inputAnchorLabelStyle = null;
		static GUIStyle			outputAnchorLabelStyle = null;
		static GUIStyle			boxAnchorStyle = null;

	#endregion

	#region Anchor rendering

		public void OnEnable()
		{
			if (!styleLoaded)
				LoadStylesAndAssets();
		}

		public void Initialize(PWNode node)
		{
			nodeRef = node;
		}

		void LoadStylesAndAssets()
		{
			//styles:
			boxAnchorStyle = new GUIStyle(GUI.skin.box);
			boxAnchorStyle.padding = new RectOffset(0, 0, 1, 1);
			anchorTexture = GUI.skin.box.normal.background;
			anchorDisabledTexture = GUI.skin.box.active.background;
			inputAnchorLabelStyle = GUI.skin.FindStyle("InputAnchorLabel");
			outputAnchorLabelStyle = GUI.skin.FindStyle("OutputAnchorLabel");
			styleLoaded = true;
			
			//assets:
			errorIcon = Resources.Load< Texture2D >("ic_error");
		}

		public void OnDisable() {}

		Dictionary< PWAnchorHighlight, Color > highlightModeToColor = new Dictionary< PWAnchorHighlight, Color > {
			{ PWAnchorHighlight.None, Color.white },
			{ PWAnchorHighlight.AttachAdd, Color.green },
			{ PWAnchorHighlight.AttachNew, Color.blue },
			{ PWAnchorHighlight.AttachReplace, Color.yellow },
		};

		void RenderAnchor(PWAnchor anchor, ref Rect renderRect, int index)
		{
			//anchor name:
			string anchorName = name;

			if (!string.IsNullOrEmpty(name) && anchors.Count > 1)
				anchorName += " #" + index;

			//highlight mode to GUI color:
			if (anchor.isLinkable)
				GUI.color = highlightModeToColor[anchor.highlighMode];
			else
				GUI.color = PWColorScheme.GetColor(PWColorScheme.disabledAnchorColor);

			//Draw the anchor:
			GUI.DrawTexture(anchor.rect, anchorTexture, ScaleMode.ScaleToFit);
			
			//Draw the anchor name if not null
			if (string.IsNullOrEmpty(name))
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
			if (anchor.visibility == PWVisibility.Visible && required && anchor.linkCount == 0)
			{
				Rect errorIconRect = new Rect(anchor.rect);
				errorIconRect.size = Vector2.one * 17;
				errorIconRect.position += new Vector2(-6, -10);
				GUI.color = Color.red;
				GUI.DrawTexture(errorIconRect, errorIcon);
				GUI.color = Color.white;
			}
			
			//debug:
		}

		public void Render(ref Rect renderRect)
		{
			int index = 0;

			renderRect.y += offset;

			foreach (var anchor in anchors)
			{
				RenderAnchor(anchor, ref renderRect, index++);
				renderRect.y += padding;
			}
		}

	#endregion
	}
}
