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
		PWGraphEditorEventInfo	oldEventInfos;

		public void RenderAnchors()
		{
			var e = Event.current;

			int			windowHeaderSize = nodeStyle.border.top + nodeStyle.margin.top;
			int			windowHorizontalPadding = 15;
			Rect		inputAcnhorRect = new Rect(windowHorizontalPadding, windowHeaderSize, 120, -1);
			Rect		outputAnchorRect = new Rect(rect.width - windowHorizontalPadding, windowHeaderSize, -120, -1);

			foreach (var anchorField in inputAnchorFields)
				anchorField.Render(ref inputAcnhorRect);
			foreach (var anchorField in outputAnchorFields)
				anchorField.Render(ref outputAnchorRect);
		}

		public void ProcessAnchorEvents()
		{
			Event					e = Event.current;
			PWGraphEditorEventInfo	editorEvents = graphRef.editorEvents;
			
			oldEventInfos = editorEvents.Clone(oldEventInfos);

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
