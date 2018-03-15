using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using ProceduralWorlds;
using ProceduralWorlds.Core;

namespace ProceduralWorlds.Editor
{
	public abstract partial class BaseNodeEditor
	{
		void BindEvents()
		{
			if (graphEditor != null)
			{
				graphEditor.OnClickNowhere += OnClickedOutside;
				graphEditor.OnLinkStartDragged += LinkStartDragCallback;
				graphEditor.OnLinkStopDragged += LinkStopDragCallback;
				graphEditor.OnLinkCanceled += LinkCanceledCallback;

				graphRef.OnLinkCreated += LinkCreatedCallback;
			}

			OnDraggedLinkOverAnchor += DraggedLinkOverAnchorCallback;
			OnDraggedLinkQuitAnchor += DraggedLinkQuitAnchorCallbck;
		}

		void UnBindEvents()
		{
			if (graphEditor != null)
			{
				graphEditor.OnClickNowhere -= OnClickedOutside;
				graphEditor.OnLinkStartDragged -= LinkStartDragCallback;
				graphEditor.OnLinkStopDragged -= LinkStopDragCallback;
				graphEditor.OnLinkCanceled -= LinkCanceledCallback;
	
				graphRef.OnLinkCreated -= LinkCreatedCallback;
			}

			OnDraggedLinkOverAnchor -= DraggedLinkOverAnchorCallback;
			OnDraggedLinkQuitAnchor -= DraggedLinkQuitAnchorCallbck;
		}
		
		void LinkCreatedCallback(NodeLink link)
		{
			ResetUnlinkableAnchors();
		}

		void LinkStartDragCallback(Anchor fromAnchor)
		{
			//disable non-linkable anchors:
			if (fromAnchor.nodeRef != nodeRef)
				DisableUnlinkableAnchors(fromAnchor);
		}

		void LinkStopDragCallback()
		{
			//reset anchor highlight
			ResetUnlinkableAnchors();

			//reset link highlight
			foreach (var anchorField in nodeRef.anchorFields)
				foreach (var anchor in anchorField.anchors)
					foreach (var link in anchor.links)
						link.ResetHighlight();
		}

		void LinkCanceledCallback()
		{
			//reset the highlight mode on anchors:
		}
		
		void DraggedLinkOverAnchorCallback(Anchor anchor)
		{
			if (!AnchorUtils.AnchorAreAssignable(editorEvents.startedLinkAnchor, anchor))
				return ;

			//update anchor highlight
			if (anchor.anchorType == AnchorType.Input)
			{
				if (anchor.linkCount >= 1)
				{
					//highlight links with delete color
					foreach (var link in anchor.links)
						link.highlight = LinkHighlightMode.Delete;
					anchor.highlighMode = AnchorHighlight.AttachReplace;
				}
				else
					anchor.highlighMode = AnchorHighlight.AttachNew;
			}
			else
			{
				//highlight our link with delete color
				foreach (var link in editorEvents.startedLinkAnchor.links)
					link.highlight = LinkHighlightMode.Delete;
				
				if (anchor.linkCount > 0)
					anchor.highlighMode = AnchorHighlight.AttachAdd;
				else
					anchor.highlighMode = AnchorHighlight.AttachNew;
			}
		}

		void DraggedLinkQuitAnchorCallbck(Anchor anchor)
		{
			anchor.highlighMode = AnchorHighlight.None;

			//reset link hightlight
			foreach (var link in anchor.links)
				link.ResetHighlight();
			foreach (var link in editorEvents.startedLinkAnchor.links)
				link.ResetHighlight();
		}
		
		void		OnClickedOutside()
		{
			if (Event.current.button == 0)
			{
				windowNameEdit = false;
				GUI.FocusControl(null);
			}
			if (Event.current.button == 0 && !Event.current.shift)
				nodeRef.isSelected = false;
			nodeRef.isDragged = false;
		}
		
		void		DisableUnlinkableAnchors(Anchor anchor)
		{
			List< AnchorField >	anchorFields;

			if (anchor.anchorType == AnchorType.Output)
				anchorFields = nodeRef.inputAnchorFields;
			else
				anchorFields = nodeRef.outputAnchorFields;
			
			foreach (var anchorField in anchorFields)
				anchorField.DisableIfUnlinkable(anchor);
		}

		//reset anchor colors and visibility after a link was dragged.
		void		ResetUnlinkableAnchors()
		{
			foreach (var anchorField in nodeRef.inputAnchorFields)
				anchorField.ResetLinkable();
			foreach (var anchorField in nodeRef.outputAnchorFields)
				anchorField.ResetLinkable();
		}

	}
}