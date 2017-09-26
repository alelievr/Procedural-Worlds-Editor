using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using PW;
using PW.Core;

//rendering and processing of links and dependencies
namespace PW
{
	public partial class PWNode
	{
		const float	tanPower = 50;

		PWLinkType		GetLinkType(Type from, Type to)
		{
			if (from == typeof(Sampler2D) || to == typeof(Sampler2D))
				return PWLinkType.Sampler2D;
			if (from == typeof(Sampler3D) || to == typeof(Sampler3D))
				return PWLinkType.Sampler3D;
			if (from == typeof(float) || to == typeof(float))
				return PWLinkType.BasicData;
			if (from.IsSubclassOf(typeof(ChunkData)) || to.IsSubclassOf(typeof(ChunkData)))
				return PWLinkType.ChunkData;
			if (from == typeof(Vector2) || to == typeof(Vector2))
				return PWLinkType.TwoChannel;
			if (from == typeof(Vector3) || to == typeof(Vector3))
				return PWLinkType.ThreeChannel;
			if (from == typeof(Vector4) || to == typeof(Vector4))
				return PWLinkType.FourChannel;
			return PWLinkType.BasicData;
		}
		
		public void RemoveLink(PWNodeLink link)
		{
			//fire this event will trigger the graph and it will call anchor to remove link datas.
			OnLinkRemoved(link);
		}

		public void		RemoveAllLinksFromAnchor(string fieldName)
		{
			foreach (var anchor in anchorFields[fieldName].anchors)
				RemoveAllLinksFromAnchor(anchor);
		}

		public void		RemoveAllLinksFromAnchor(PWAnchor anchor)
		{
			foreach (var link in anchor.links)
				RemoveLink(link);
		}

		public void		RemoveLinkFromAnchors(PWAnchor fromAnchor, PWAnchor toAnchor)
		{
			PWNodeLink link = fromAnchor.links.FirstOrDefault(l => l.toAnchor == toAnchor);

			if (link != null)
				RemoveLink(link);
		}
		
		public void		RemoveLinkFromNode(PWNode node)
		{
			List< PWNodeLink > linkToRemove = new List< PWNodeLink >();

			//search in input anchors:
			foreach (var anchorField in inputAnchorFields)
				foreach (var anchor in anchorField.anchors)
					linkToRemove.AddRange(anchor.links.Where(l => l.toNode == node));
					
			//search in output anchors:
			foreach (var anchorField in outputAnchorFields)
				foreach (var anchor in anchorField.anchors)
					linkToRemove.AddRange(anchor.links.Where(l => l.fromNode == node));

			foreach (var link in linkToRemove)
				RemoveLink(link);
		}
		
		public void		RemoveAllLinks()
		{
			foreach (var anchorField in inputAnchorFields.Concat(outputAnchorFields))
				foreach (var anchor in anchorField.anchors)
					foreach (var link in anchor.links)
						RemoveLink(link);
		}

		public IEnumerable< PWNodeLink >	GetOutputLinks()
		{
			//TODO: bake this to avoid GC
			return	from oaf in outputAnchorFields
					from oa in oaf.anchors
					from l in oa.links
					select l;
		}

		public IEnumerable< PWNodeLink >	GetInputLinks()
		{
			//TODO: bake this to avoid GC
			return	from oaf in inputAnchorFields
					from oa in oaf.anchors
					from l in oa.links
					select l;
		}

		public void RenderLinks()
		{
			Handles.BeginGUI();
			foreach (var anchorField in outputAnchorFields)
				foreach (var anchor in anchorField.anchors)
					foreach (var link in anchor.links)
						DrawNodeCurve(link);
			Handles.EndGUI();
		}
		
		void DrawNodeCurve(PWAnchor anchor, Vector2 endPoint)
		{
			Vector3 startPos = new Vector3(anchor.rect.x + anchor.rect.width, anchor.rect.y + anchor.rect.height / 2, 0);
			DrawSelectedBezier(startPos, endPoint, startPos + Vector3.right * tanPower, Vector3.zero, anchor.colorPalette, 4, PWLinkHighlight.None);
		}
		
		void DrawNodeCurve(PWNodeLink link)
		{
			if (link == null)
			{
				Debug.LogError("[PWGraphEditor] attempt to draw null link !");
				return ;
			}
	
			Event e = Event.current;
	
			if (link.controlId == -1)
				link.controlId = GUIUtility.GetControlID(FocusType.Passive);
	
			//TODO: integrate eventMasks
	
			Rect start = link.fromAnchor.rect;
			Rect end = link.toAnchor.rect;
			Vector3 startPos = new Vector3(start.x + start.width, start.y + start.height / 2, 0);
			Vector3 endPos = new Vector3(end.x, end.y + end.height / 2, 0);
			
			Vector3 startDir = Vector3.right;
			Vector3 endDir = Vector3.left;
	
			Vector3 startTan = startPos + startDir * tanPower;
			Vector3 endTan = endPos + endDir * tanPower;
	
			switch (e.GetTypeForControl(link.controlId))
			{
				case EventType.MouseDown:
					if (HandleUtility.nearestControl == link.controlId && (e.button == 0 || e.button == 1))
					{
						GUIUtility.hotControl = link.controlId;
						//unselect all others links:
						graphRef.RaiseOnLinkSelected(link);
						link.selected = true;
						link.highlight = PWLinkHighlight.Selected;
					}
					break ;
			}
			if (HandleUtility.nearestControl == link.controlId)
			{
				Debug.Log("Bezier curve take the control ! Will it break the window focus ?");
			}
	
			HandleUtility.AddControl(link.controlId, HandleUtility.DistancePointBezier(e.mousePosition, startPos, endPos, startTan, endTan) / 1.5f);
			if (e.type == EventType.Repaint)
			{
				DrawSelectedBezier(startPos, endPos, startTan, endTan, link.colorPalette, 4, link.highlight);
	
				if (link != null && link.highlight == PWLinkHighlight.DeleteAndReset)
					link.highlight = PWLinkHighlight.None;
				
				if (link != null && !link.selected && link.highlight == PWLinkHighlight.Selected)
					link.highlight = PWLinkHighlight.None;
			}
		}
	
		void	DrawSelectedBezier(Vector3 startPos, Vector3 endPos, Vector3 startTan, Vector3 endTan, PWColorPalette colorPalette, int width, PWLinkHighlight highlight)
		{
			switch (highlight)
			{
				case PWLinkHighlight.Selected:
					Handles.DrawBezier(startPos, endPos, startTan, endTan, PWColorScheme.GetColor("selectedNode"), null, width + 3);
	;				break ;
				case PWLinkHighlight.Delete:
				case PWLinkHighlight.DeleteAndReset:
					Handles.DrawBezier(startPos, endPos, startTan, endTan, new Color(1f, .0f, .0f, 1), null, width + 2);
					break ;
			}
			Color c = PWColorScheme.GetLinkColor(colorPalette);
			Handles.DrawBezier(startPos, endPos, startTan, endTan, c, null, width);
		}
	}
}
