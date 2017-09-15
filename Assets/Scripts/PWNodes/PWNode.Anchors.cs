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

		void RenderAnchor(PWAnchorData data, PWAnchorData.PWAnchorMultiData singleAnchor, int index)
		{
			#if !HIDE_ANCHOR_LABEL
				if (!String.IsNullOrEmpty(anchorName))
				{
				}
			#endif
			GUI.color = Color.white;
			
			if (!singleAnchor.enabled)
				GUI.DrawTexture(singleAnchor.anchorRect, anchorDisabledTexture);

			//reset the Highlight:
			singleAnchor.highlighMode = PWAnchorHighlight.None;


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

			int			windowHeaderSize = windowStyle.border.top + windowStyle.margin.top;
			int			windowHorizontalPadding = 15;
			Rect		inputAcnhorRect = new Rect(windowHorizontalPadding, windowHeaderSize, 120, -1);
			Rect		outputAnchorRect = new Rect(windowRect.width - windowHorizontalPadding, windowHeaderSize, -120, -1);

			foreach (var anchorField in inputAnchors)
				anchorField.Render(ref inputAcnhorRect);
			foreach (var anchorField in outputAnchors)
				anchorField.Render(ref outputAnchorRect);
		}
	}
}
