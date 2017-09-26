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

		public void ProcessAnchorEvents()
		{
			Event					e = Event.current;
			PWGraphEditorEventInfo	editorEvents = graphRef.editorEvents;
			PWGraphEditorEventInfo	oldEventInfos = editorEvents;

			//process events on every anchors:
			foreach (var anchorField in inputAnchorFields)
				anchorField.ProcessEvent(ref editorEvents);
			foreach (var anchorField in outputAnchorFields)
				anchorField.ProcessEvent(ref editorEvents);

			//link anchor event is we release the mouse with a draggingLink.
			if (e.type == EventType.MouseUp && editorEvents.isDraggingLink)
				OnAnchorLinked(editorEvents.mouseOverAnchor);

			if (editorEvents.isDraggingLink)
			{
				if (oldEventInfos.mouseOverAnchor != editorEvents.mouseOverAnchor)
				{
					if (editorEvents.mouseOverAnchor == null)
						OnDraggedLinkQuitAnchor(oldEventInfos.mouseOverAnchor);
					else
						OnDraggedLinkOverAnchor(editorEvents.mouseOverAnchor);
				}
			}
		}
	}
}
