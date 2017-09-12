using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using PW.Core;

//Rendering and event processing of anchors:
namespace PW
{
	public partial class PWNode
	{
		void ProcessAnchor(
			PWAnchorData data,
			PWAnchorData.PWAnchorMultiData singleAnchor,
			ref Rect inputAnchorRect,
			ref Rect outputAnchorRect,
			ref PWAnchorInfo ret,
			int index = -1)
		{
			Rect anchorRect = (data.anchorType == PWAnchorType.Input) ? inputAnchorRect : outputAnchorRect;

			singleAnchor.anchorRect = anchorRect;
			if (singleAnchor.forcedY != -1)
			{
				singleAnchor.anchorRect.y = singleAnchor.forcedY;
				anchorRect.y = singleAnchor.forcedY;
			}

			if (!ret.mouseAbove)
			{
				ret = new PWAnchorInfo(data.fieldName, PWUtils.DecalRect(singleAnchor.anchorRect, graphDecal + windowRect.position),
					singleAnchor.color, data.type,
					data.anchorType, nodeId, singleAnchor.id,
					data.classAQName, index,
					data.generic, data.allowedTypes,
					singleAnchor.linkType, singleAnchor.linkCount);
			}
			if (anchorRect.Contains(Event.current.mousePosition))
				ret.mouseAbove = true;
		}

		void ProcessAnchors()
		{
			int		anchorWidth = 13;
			int		anchorHeight = 13;
			int		anchorMargin = 2;

			Rect	inputAnchorRect = new Rect(3, 20 + anchorMargin, anchorWidth, anchorHeight);
			Rect	outputAnchorRect = new Rect(windowRect.size.x - anchorWidth - 3, 20 + anchorMargin, anchorWidth, anchorHeight);

			//if there is more values in PWValues than the available anchor count, create new anchors:
			ForeachPWAnchorDatas((data) => {
				if (data.anchorType == PWAnchorType.Input && data.multiple && data.anchorInstance != null && !data.forcedAnchorNumber)
				{
					if (((PWValues)data.anchorInstance).Count >= data.multi.Count)
						data.AddNewAnchor(data.fieldName.GetHashCode() + data.multi.Count, false);
				}
			});

			ForeachPWAnchors((data, singleAnchor, i) => {
				//process anchor event and calcul rect position if visible
				if (singleAnchor.visibility != PWVisibility.Gone)
				{
					if (singleAnchor.visibility != PWVisibility.Gone && i <= 0)
					{
						if (data.anchorType == PWAnchorType.Input)
							inputAnchorRect.position += data.offset;
						else if (data.anchorType == PWAnchorType.Output)
							outputAnchorRect.position += data.offset;
					}
					if (singleAnchor.visibility != PWVisibility.Gone && i != -1)
					{
						if (data.anchorType == PWAnchorType.Input)
							inputAnchorRect.position += new Vector2(0, data.multiPadding);
						else if (data.anchorType == PWAnchorType.Output)
							outputAnchorRect.position += new Vector2(0, data.multiPadding);
					}
					if (singleAnchor.visibility == PWVisibility.Visible)
						ProcessAnchor(data, singleAnchor, ref inputAnchorRect, ref outputAnchorRect, ref ret, i);
					if (singleAnchor.visibility != PWVisibility.Gone && singleAnchor.forcedY == -1)
					{
						if (data.anchorType == PWAnchorType.Input)
							inputAnchorRect.position += Vector2.up * (18 + anchorMargin);
						else if (data.anchorType == PWAnchorType.Output)
							outputAnchorRect.position += Vector2.up * (18 + anchorMargin);
					}
				}
			});
			maxAnchorRenderHeight = (int)Mathf.Max(inputAnchorRect.position.y - 20, outputAnchorRect.position.y - 20);
			anchorUnderMouse = ret;
		}

		public PWAnchorInfo GetAnchorUnderMouse()
		{
			return anchorUnderMouse;
		}
		
		void RenderAnchor(PWAnchorData data, PWAnchorData.PWAnchorMultiData singleAnchor, int index)
		{
			//if anchor have not been processed:
			if (singleAnchor == null || boxAnchorStyle == null)
				return ;

			string anchorName = (data.multiple) ? singleAnchor.name : data.anchorName;

			/*if (data.multiple && data.anchorType == PWAnchorType.Input)
			{
				if (singleAnchor.additional)
					anchorName = "+";
				else
					anchorName += index;
			}*/

			switch (singleAnchor.highlighMode)
			{
				case PWAnchorHighlight.AttachAdd:
					GUI.color = anchorAttachAddColor;
					break ;
				case PWAnchorHighlight.AttachNew:
					GUI.color = anchorAttachNewColor;
					break ;
				case PWAnchorHighlight.AttachReplace:
					GUI.color = anchorAttachReplaceColor;
					break ;
				case PWAnchorHighlight.None:
				default:
					GUI.color = singleAnchor.color;
					break ;

			}
			GUI.DrawTexture(singleAnchor.anchorRect, anchorTexture, ScaleMode.ScaleToFit);

			#if !HIDE_ANCHOR_LABEL
				if (!String.IsNullOrEmpty(anchorName))
				{
					Rect	anchorNameRect = singleAnchor.anchorRect;
					Vector2 textSize = GUI.skin.label.CalcSize(new GUIContent(anchorName));
					if (data.anchorType == PWAnchorType.Input)
						anchorNameRect.position += new Vector2(-6, -2);
					else
						anchorNameRect.position += new Vector2(-textSize.x + 4, -2);
					anchorNameRect.size = textSize + new Vector2(15, 4); //add the anchorLabel size
					GUI.depth = 10;
					GUI.Label(anchorNameRect, anchorName, (data.anchorType == PWAnchorType.Input) ? inputAnchorLabelStyle : outputAnchorLabelStyle);
				}
			#endif
			GUI.color = Color.white;
			
			if (!singleAnchor.enabled)
				GUI.DrawTexture(singleAnchor.anchorRect, anchorDisabledTexture);

			//reset the Highlight:
			singleAnchor.highlighMode = PWAnchorHighlight.None;

			if (data.required && singleAnchor.linkCount == 0
				&& (!data.multiple || (data.multiple && index < data.minMultipleValues)))
			{
				Rect errorIconRect = new Rect(singleAnchor.anchorRect);
				errorIconRect.size = Vector2.one * 17;
				errorIconRect.position += new Vector2(-6, -10);
				GUI.color = Color.red;
				GUI.DrawTexture(errorIconRect, errorIcon);
				GUI.color = Color.white;
			}

			#if DEBUG_ANCHOR
				Rect anchorSideRect = singleAnchor.anchorRect;
				if (data.anchorType == PWAnchorType.Input)
				{
					anchorSideRect.size += new Vector2(100, 20);
				}
				else
				{
					anchorSideRect.position -= new Vector2(100, 0);
					anchorSideRect.size += new Vector2(100, 20);
				}
				GUI.Label(anchorSideRect, (long)singleAnchor.id + " | " + singleAnchor.linkCount);
			#endif
		}
		
		public void RenderAnchors()
		{
			var e = Event.current;

			//rendering anchors
			ForeachPWAnchors((data, singleAnchor, i) => {
				//draw anchor:
				if (singleAnchor.visibility != PWVisibility.Gone)
				{
					if (singleAnchor.visibility == PWVisibility.Visible)
						RenderAnchor(data, singleAnchor, i);
					if (singleAnchor.visibility == PWVisibility.InvisibleWhenLinking)
						singleAnchor.visibility = PWVisibility.Visible;
				}
			});
			
			//rendering node rename field	
			if (renamable)
			{
				Vector2	winSize = windowRect.size;
				Rect	renameRect = new Rect(0, 0, winSize.x, 18);
				Rect	renameIconRect = new Rect(winSize.x - 28, 3, 12, 12);
				string	renameNodeField = "renameWindow";

				GUI.color = Color.black * .9f;
				GUI.DrawTexture(renameIconRect, editIcon);
				GUI.color = Color.white;

				if (windowNameEdit)
				{
					GUI.SetNextControlName(renameNodeField);
					externalName = GUI.TextField(renameRect, externalName, renameNodeTextFieldStyle);
	
					if (e.type == EventType.MouseDown && !renameRect.Contains(e.mousePosition))
					{
						windowNameEdit = false;
						GUI.FocusControl(null);
					}
					if (GUI.GetNameOfFocusedControl() == renameNodeField)
					{
						if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.Escape)
						{
							windowNameEdit = false;
							GUI.FocusControl(null);
							e.Use();
						}
					}
				}
				
				if (renameIconRect.Contains(e.mousePosition))
				{
					if (e.type == EventType.Used) //used by drag
					{
						windowNameEdit = true;
						GUI.FocusControl(renameNodeField);
						var te = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
						if (te != null)
							te.SelectAll();
					}
					else if (e.type == EventType.MouseDown)
						windowNameEdit = false;
				}
			}
		}
	}
}
