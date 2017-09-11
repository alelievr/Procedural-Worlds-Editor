using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW.Core;

namespace PW
{
	public partial class PWNode {
	
		public void				UpdatePropEnabled(string propertyName, bool enabled, int index = 0)
		{
			if (propertyDatas.ContainsKey(propertyName))
			{
				var anchors = propertyDatas[propertyName].multi;
				if (anchors.Count <= index)
					return ;
				anchors[index].enabled = true;
			}
		}

		public void				UpdatePropName(string propertyName, string newName, int index = 0)
		{
			if (propertyDatas.ContainsKey(propertyName))
			{
				var anchors = propertyDatas[propertyName].multi;
				if (anchors.Count <= index)
					return ;
				anchors[index].name = newName;
			}
		}

		public void				UpdatePropBackgroundColor(string propertyName, Color newColor, int index = 0)
		{
			if (propertyDatas.ContainsKey(propertyName))
			{
				var anchors = propertyDatas[propertyName].multi;
				if (anchors.Count <= index)
					return ;
				anchors[index].color = (SerializableColor)newColor;
			}
		}

		public void				UpdatePropVisibility(string propertyName, PWVisibility visibility, int index = 0)
		{
			if (propertyDatas.ContainsKey(propertyName))
			{
				var anchors = propertyDatas[propertyName].multi;
				if (anchors.Count <= index)
					return ;
				anchors[index].visibility = visibility;
			}
			UpdateDependentStatus();
		}

		public void				UpdatePropPosition(string propertyName, float y, int index = -1)
		{
			if (propertyDatas.ContainsKey(propertyName))
			{
				var anchors = propertyDatas[propertyName].multi;
				if (anchors.Count <= index)
					return ;
				anchors[index].forcedY = y;
			}
			UpdateDependentStatus();
		}

		public void				UpdateMultiProp(string propertyName, int newCount, params string[] newNames)
		{
			if (!propertyDatas.ContainsKey(propertyName))
				return ;
			if (!propertyDatas[propertyName].multiple)
				return ;
				
			var prop = propertyDatas[propertyName];
			
			if (prop.multi.Count == newCount && (newNames == null || newNames.Length == 0)) //alreadythe good name of anchors.
				return ;

			prop.forcedAnchorNumber = true;
			prop.RemoveAllAnchors();
			prop.minMultipleValues = 0;
			prop.anchorInstance = bakedNodeFields[propertyName].GetValue(this);
			
			PWValues values = prop.anchorInstance as PWValues;
			if (prop.anchorInstance != null)
				values.Clear();

			for (int i = 0; i < newCount; i++)
			{
				prop.AddNewAnchor(prop.fieldName.GetHashCode() + i);
				if (newNames != null && i < newNames.Length)
					prop.multi[i].name = newNames[i];
				prop.multi[i].color = (SerializableColor)FindColorFromtypes(prop.allowedTypes);
			}
		}

		public bool				RequestRemoveLink(string propertyName, int index = 0)
		{
			if (!propertyDatas.ContainsKey(propertyName))
				return false;
			
			var prop = propertyDatas[propertyName];

			if (index >= prop.multi.Count || index < 0)
				return false;

			if (currentGraph == null)
				return false;

			var links = GetLinks(prop.multi[index].id);
			var toRemove = new List< PWLink >();
			foreach (var link in links)
				toRemove.Add(link);
			foreach (var link in toRemove)
			{
				var linkedNode = FindNodeById(link.distantNodeId);

				linkedNode.DeleteLink(link.distantAnchorId, this, link.localAnchorId);
				DeleteLink(link.localAnchorId, linkedNode, link.distantAnchorId);
			}
			return true;
		}

		public int				GetPropLinkCount(string propertyName, int index = 0)
		{
			if (propertyDatas.ContainsKey(propertyName))
			{
				var anchors = propertyDatas[propertyName].multi;
				if (anchors.Count <= index)
					return -1;
				return anchors[index].linkCount;
			}
			return -1;
		}

		public Rect?			GetAnchorRect(string propertyName, int index = 0)
		{
			if (propertyDatas.ContainsKey(propertyName))
			{
				var anchors = propertyDatas[propertyName].multi;
				if (anchors.Count <= index)
					return null;
				return PWUtils.DecalRect(anchors[index].anchorRect, graphDecal + windowRect.position);
			}
			return null;
		}

		public Rect?			GetAnchorRect(int id)
		{
			foreach (var propKP in propertyDatas)
				foreach (var anchor in propKP.Value.multi)
					if (anchor.id == id)
						return PWUtils.DecalRect(anchor.anchorRect, graphDecal + windowRect.position);
			return null;
		}

		public PWAnchorData		GetAnchorData(string propName)
		{
			if (propertyDatas.ContainsKey(propName))
				return propertyDatas[propName];
			return null;
		}

		public int?				GetAnchorId(PWAnchorType anchorType, int index)
		{
			var multiAnchorList =	from p in propertyDatas
									where p.Value.anchorType == anchorType
									select p.Value.multi;
			
			int i = 0;
			foreach (var anchorList in multiAnchorList)
				foreach (var anchor in anchorList)
					if (anchor.visibility == PWVisibility.Visible)
					{
						if (index == i)
							return anchor.id;
						i++;
					}
			return null;
		}

		public PWNode			GetFirstNodeAttachedToAnchor(int anchorId)
		{
			return GetNodesAttachedToAnchor(anchorId).FirstOrDefault();
		}

		public List< PWNode >	GetNodesAttachedToAnchor(int anchorId)
		{
			return links.Where(l => l.localAnchorId == anchorId).Select(l => FindNodeById(l.distantNodeId)).ToList();
		}

		public List< PWNode > 	GetOutputNodes()
		{
			var nodes = links.Select(l => FindNodeById(l.distantNodeId)).Where(n => n != null);
			List< PWNode > finalList = new List< PWNode >();

			foreach (var node in nodes)
			{
				if (node.GetType() == typeof(PWNodeGraphExternal))
					finalList.AddRange((node as PWNodeGraphExternal).graphInput.GetOutputNodes());
				else
					finalList.Add(node);
			}
			return finalList;
		}

		public List< PWNode >	GetInputNodes()
		{
			var nodes = depencendies.Select(d => FindNodeById(d.nodeId)).Where(n => n != null);
			List< PWNode > finalList = new List< PWNode >();
			
			foreach (var node in nodes)
			{
				if (node.GetType() == typeof(PWNodeGraphExternal))
					finalList.AddRange(((node as PWNodeGraphExternal).graphOutput.GetInputNodes()));
				else
					finalList.Add(node);
			}
			return nodes.ToList();
		}

		protected PWGraphTerrainType	GetTerrainOutputMode()
		{
			return graph.outputType;
		}

		protected string				GetGraphName()
		{
			return graph.externalName;
		}

		protected string				GetGraphPath()
		{
			if (graph.assetPath != null)
				return System.IO.Path.GetDirectoryName(graph.assetPath);
			return null;
		}

		//anchor:
		public PWAnchorData		GetAnchorData(int id, out PWAnchorData.PWAnchorMultiData singleAnchorData)
		{
			int				index;
			
			return GetAnchorData(id, out singleAnchorData, out index);
		}

		public PWAnchorData	GetAnchorData(int id, out PWAnchorData.PWAnchorMultiData singleAnchorData, out int index)
		{
			PWAnchorData					ret = null;
			PWAnchorData.PWAnchorMultiData	s = null;
			int								retIndex = 0;

			ForeachPWAnchors((data, singleAnchor, i) => {
				if (singleAnchor.id == id)
				{
					s = singleAnchor;
					ret = data;
					retIndex = i;
				}
			}, true);
			index = retIndex;
			singleAnchorData = s;
			return ret;
		}
	}
}
