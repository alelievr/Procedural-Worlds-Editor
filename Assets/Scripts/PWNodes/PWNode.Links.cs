using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using PW;
using PW.Core;

//rendering and processing of links and dependencies
namespace PW
{
	public partial class PWNode
	{
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
	}
}
