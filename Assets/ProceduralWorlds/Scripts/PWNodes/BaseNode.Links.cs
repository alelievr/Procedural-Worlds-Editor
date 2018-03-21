using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using ProceduralWorlds;
using ProceduralWorlds.Core;

//Link API
namespace ProceduralWorlds
{
	public partial class BaseNode
	{
	
		public void RemoveLink(NodeLink link)
		{
			//call this function will fire an event which trigger the graph and causes anchors to remove link datas.
			// and remove the link
			graphRef.RemoveLink(link);
		}

		public void		RemoveAllLinksFromAnchor(string fieldName)
		{
			foreach (var anchor in anchorFieldDictionary[fieldName].anchors)
				RemoveAllLinksFromAnchor(anchor);
		}

		public void		RemoveAllLinksFromAnchor(Anchor anchor)
		{
			anchor.RemoveAllLinks();
		}

		public void		RemoveLinkFromAnchors(Anchor fromAnchor, Anchor toAnchor)
		{
			NodeLink link = fromAnchor.links.FirstOrDefault(l => l.toAnchor == toAnchor);

			if (link != null)
				RemoveLink(link);
		}
		
		public void		RemoveLinkFromNode(BaseNode node)
		{
			List< NodeLink > linkToRemove = new List< NodeLink >();

			//search in input anchors:
			foreach (var anchor in inputAnchors)
				linkToRemove.AddRange(anchor.links.Where(l => l.toNode == node));
			
			//search in output anchors:
			foreach (var anchor in outputAnchors)
				linkToRemove.AddRange(anchor.links.Where(l => l.fromNode == node));

			foreach (var link in linkToRemove)
				RemoveLink(link);
		}
		
		public void		RemoveAllLinks()
		{
			foreach (var anchorField in anchorFields)
			{
				int i = 0;

				while (i < anchorField.anchors.Count)
				{
					if (anchorField.anchors[i].linkCount != 0)
					{
						anchorField.anchors[i].RemoveAllLinks();
					}
					i++;
				}
			}
		}

		public void		AddLink(NodeLink link)
		{
			if (link.fromNode == this)
			{
				link.fromAnchor.AddLink(link);
				OnNodeAnchorLink(link.fromAnchor.fieldName, link.fromAnchor.fieldIndex);
			}
			else if (link.toNode == this)
			{
				link.toAnchor.AddLink(link);
				OnNodeAnchorLink(link.toAnchor.fieldName, link.toAnchor.fieldIndex);
			}
			else
				Debug.LogError("[Node] tried to link a node with a link that didn't reference this node");
			
			UpdateWorkStatus();
		}

		public IEnumerable< NodeLink >	GetOutputLinks()
		{
			return	from oaf in outputAnchorFields
					from oa in oaf.anchors
					from l in oa.links
					select l;
		}

		public IEnumerable< NodeLink >	GetInputLinks()
		{
			return	from oaf in inputAnchorFields
					from oa in oaf.anchors
					from l in oa.links
					select l;
		}
	}
}
