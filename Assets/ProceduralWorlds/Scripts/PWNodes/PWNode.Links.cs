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
	
		public void RemoveLink(PWNodeLink link)
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

		public void		RemoveAllLinksFromAnchor(PWAnchor anchor)
		{
			anchor.RemoveAllLinks();
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
				foreach (var anchor in anchorField.anchors)
					anchor.RemoveAllLinks();
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
	}
}
