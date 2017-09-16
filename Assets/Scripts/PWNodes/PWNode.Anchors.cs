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
		public void RenderAnchors()
		{
			var e = Event.current;

			int			windowHeaderSize = windowStyle.border.top + windowStyle.margin.top;
			int			windowHorizontalPadding = 15;
			Rect		inputAcnhorRect = new Rect(windowHorizontalPadding, windowHeaderSize, 120, -1);
			Rect		outputAnchorRect = new Rect(windowRect.width - windowHorizontalPadding, windowHeaderSize, -120, -1);

			foreach (var anchorField in inputAnchorFields)
				anchorField.Render(ref inputAcnhorRect);
			foreach (var anchorField in outputAnchorFields)
				anchorField.Render(ref outputAnchorRect);
		}
	}
}
