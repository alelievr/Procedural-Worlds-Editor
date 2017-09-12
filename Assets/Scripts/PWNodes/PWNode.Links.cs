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
		public PWAnchor		inputAnchors;
		public PWAnchor		outputAnchors;

		//links:
		public PWLink GetLink(int anchorId, int targetNodeId, int targetAnchorId)
		{
			return links.FirstOrDefault(l => l.localAnchorId == anchorId
				&& l.distantNodeId == targetNodeId
				&& l.distantAnchorId == targetAnchorId);
		}

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

		public bool		AttachLink(PWAnchorInfo from, PWAnchorInfo to)
		{
			//quit if types are not compatible
			if (!AnchorAreAssignable(from, to))
				return false;
			if (from.anchorType == to.anchorType)
				return false;
			if (from.nodeId == to.nodeId)
				return false;
			
			//if link was revoked by the node's code
			if (!OnNodeAnchorLink(from.fieldName, from.propIndex))
				return false;

			//we store output links:
			if (from.anchorType == PWAnchorType.Output)
			{
				links.Add(new PWLink(
					to.nodeId, to.anchorId, to.fieldName, to.classAQName, to.propIndex,
					from.nodeId, from.anchorId, from.fieldName, from.classAQName, from.propIndex, GetAnchorDominantColor(from, to),
					GetLinkType(from.fieldType, to.fieldType))
				);
				//mark local output anchors as linked:
				ForeachPWAnchors((data, singleAnchor, i) => {
					if (singleAnchor.id == from.anchorId)
						singleAnchor.linkCount++;
				});
			}
			else //input links are stored as depencencies:
			{
				ForeachPWAnchors((data, singleAnchor, i) => {
					if (singleAnchor.id == from.anchorId)
					{
						singleAnchor.linkCount++;
						//if data was added to multi-anchor:
						if (data.multiple && data.anchorInstance != null)
						{
							if (i == data.multipleValueCount)
								data.AddNewAnchor(data.fieldName.GetHashCode() + i + 1);
						}
					}
				});
				depencendies.Add(new PWDependency(to.nodeId, to.anchorId, from.anchorId));
			}
			return true;
		}

		public void AttachLink(string myAnchor, PWNode target, string targetAnchor)
		{
			if (!propertyDatas.ContainsKey(myAnchor) || !target.propertyDatas.ContainsKey(targetAnchor))
			{
				Debug.LogWarning("property not found: \"" + targetAnchor + "\" in " + target);
				return ;
			}

			PWAnchorData fromAnchor = propertyDatas[myAnchor];
			PWAnchorData toAnchor = target.propertyDatas[targetAnchor];

			PWAnchorInfo from = new PWAnchorInfo(
					fromAnchor.fieldName, new Rect(), fromAnchor.first.color,
					fromAnchor.type, fromAnchor.anchorType, fromAnchor.nodeId,
					fromAnchor.first.id, fromAnchor.classAQName,
					(fromAnchor.multiple) ? 0 : -1, fromAnchor.generic, fromAnchor.allowedTypes,
					fromAnchor.first.linkType, fromAnchor.first.linkCount
			);
			PWAnchorInfo to = new PWAnchorInfo(
				toAnchor.fieldName, new Rect(), toAnchor.first.color,
				toAnchor.type, toAnchor.anchorType, toAnchor.nodeId,
				toAnchor.first.id, toAnchor.classAQName,
				(toAnchor.multiple) ? 0 : -1, toAnchor.generic, toAnchor.allowedTypes,
				toAnchor.first.linkType, toAnchor.first.linkCount
			);

			AttachLink(from, to);
		}
		
		public void		DeleteAllLinkOnAnchor(int anchorId)
		{
			links.RemoveAll(l => {
				bool delete = l.localAnchorId == anchorId;
				if (delete)
					OnNodeAnchorUnlink(l.localName, l.localIndex);
				return delete;
			});
			if (DeleteDependencies(d => d.connectedAnchorId == anchorId) == 0)
			{
				PWAnchorData.PWAnchorMultiData singleAnchorData;
				GetAnchorData(anchorId, out singleAnchorData);
				singleAnchorData.linkCount = 0;
			}
		}

		public void		DeleteLink(int myAnchorId, PWNode distantWindow, int distantAnchorId)
		{
			links.RemoveAll(l => {
				bool delete = l.localAnchorId == myAnchorId && l.distantNodeId == distantWindow.id && l.distantAnchorId == distantAnchorId;
				if (delete)
					OnNodeAnchorUnlink(l.localName, l.localIndex);
				return delete;
			});
			//delete dependency and if it's not a dependency, decrement the linkCount of the link.
			if (DeleteDependencies(d => d.nodeId == distantWindow.id && d.connectedAnchorId == myAnchorId && d.anchorId == distantAnchorId) == 0)
			{
				PWAnchorData.PWAnchorMultiData	singleAnchorData;
				
				GetAnchorData(myAnchorId, out singleAnchorData);
				if (singleAnchorData != null)
					singleAnchorData.linkCount--;
			}
		}
		
		public void		DeleteLinkByWindowTarget(int targetNodeId)
		{
			PWAnchorData.PWAnchorMultiData singleAnchorData;
			for (int i = 0; i < links.Count; i++)
				if (links[i].distantNodeId == targetNodeId)
				{
					OnNodeAnchorUnlink(links[i].localName, links[i].localIndex);
					GetAnchorData(links[i].localAnchorId, out singleAnchorData);
					if (singleAnchorData == null)
						continue ;
					singleAnchorData.linkCount--;
					links.RemoveAt(i--);
				}
		}
		
		public void		DeleteAllLinks(bool callback = true)
		{
			if (callback)
				foreach (var l in links)
					OnNodeAnchorUnlink(l.localName, l.localIndex);
			links.Clear();
			depencendies.Clear();

			ForeachPWAnchors((data, singleAnchor, i) => {
				singleAnchor.linkCount = 0;
			});
		}
		
		//deps:
		int		DeleteDependencies(Func< PWDependency, bool > pred)
		{
			PWAnchorData.PWAnchorMultiData	singleAnchor;
			PWAnchorData.PWAnchorMultiData	multiAnchor;
			PWAnchorData					data;
			int								index;
			int								nDeleted = 0;

			depencendies.RemoveAll(d => {
				bool delete = pred(d);
				if (delete)
				{
					data = GetAnchorData(d.connectedAnchorId, out singleAnchor, out index);
					if (data == null)
						return delete;
					OnNodeAnchorUnlink(data.fieldName, index);
					singleAnchor.linkCount--;
					nDeleted++;
					if (data.multiple)
					{
						PWValues vals = bakedNodeFields[data.fieldName].GetValue(this) as PWValues;
						vals.AssignAt(index, null, null);
						for (int i = vals.Count - 1; i != 0 && i >= data.minMultipleValues ; i--)
						{
							int id = data.fieldName.GetHashCode() + i;
							if (GetAnchorData(id, out multiAnchor) != null && multiAnchor.linkCount == 0)
							{
								vals.RemoveAt(i);
								data.multi.RemoveAt(i);
								data.multipleValueCount--;
								if (GetAnchorData(id + 1, out multiAnchor) != null)
									multiAnchor.id--;
							}
							else if (GetAnchorData(id, out multiAnchor) != null)
								break ;
						}
					}
					else
						bakedNodeFields[data.fieldName].SetValue(this, null);
				}
				return delete;
			});
			return nDeleted;
		}

		public void		DeleteDependency(int targetNodeId, int distantAnchorId)
		{
			DeleteDependencies(d => d.nodeId == targetNodeId && d.anchorId == distantAnchorId);
		}

		public void		DeleteDependenciesByWindowTarget(int targetNodeId)
		{
			DeleteDependencies(d => d.nodeId == targetNodeId);
		}
		
		public List< Pair < int, int > >	GetAnchorConnections(int anchorId)
		{
			return depencendies.Where(d => d.connectedAnchorId == anchorId)
					.Select(d => new Pair< int, int >(d.nodeId, d.anchorId))
					.Concat(links.Where(l => l.localAnchorId == anchorId)
						.Select(l => new Pair< int, int >(l.distantNodeId, l.distantAnchorId))
					).ToList();
		}

		public List< PWDependency >	GetDependencies()
		{
			return depencendies;
		}

		public IEnumerable< PWDependency > GetDependencies(int anchorId)
		{
			return depencendies.Where(d => d.connectedAnchorId == anchorId);
		}

		public PWDependency			GetDependency(int dependencyAnchorId, int nodeId, int anchorId)
		{
			return depencendies.FirstOrDefault(d => d.connectedAnchorId == dependencyAnchorId && d.anchorId == anchorId && d.nodeId == nodeId);
		}
	}
}
