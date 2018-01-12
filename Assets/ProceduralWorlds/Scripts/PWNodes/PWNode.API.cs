using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using PW.Core;

namespace PW
{
	public partial class PWNode
	{

		PWAnchor				GetAnchorFromField(string fieldName, int index = 0)
		{
			if (anchorFieldDictionary.ContainsKey(fieldName))
			{
				var anchorField = anchorFieldDictionary[fieldName];
				if (index < anchorField.anchors.Count)
					return anchorField.anchors[index];
			}
			return null;
		}

		public void				SetAnchorEnabled(string fieldName, bool enabled, int index = 0)
		{
			PWAnchor	anchor = GetAnchorFromField(fieldName, index);

			if (anchor != null)
				anchor.enabled = enabled;
		}
	
		public void				SetAnchorName(string fieldName, string newName, int index = 0)
		{
			PWAnchor	anchor = GetAnchorFromField(fieldName, index);

			if (anchor != null)
				anchor.name = newName;
		}

		public void				SetAnchorColor(string fieldName, Color newColor, int index = 0)
		{
			PWAnchor	anchor = GetAnchorFromField(fieldName, index);

			if (anchor != null)
				anchor.color = newColor;
		}

		public void				SetAnchorVisibility(string fieldName, PWVisibility visibility, int index = 0)
		{
			PWAnchor	anchor = GetAnchorFromField(fieldName, index);

			if (anchor != null)
				anchor.visibility = visibility;
		}

		public void				SetMultiAnchor(string fieldName, int newCount, params string[] newNames)
		{
			if (!anchorFieldDictionary.ContainsKey(fieldName))
			{
				Debug.LogError("Unknown anchor \"" + fieldName + "\" in node " + this);
				return ;
			}

			var anchorField = anchorFieldDictionary[fieldName];

			if (anchorField.anchors.Count > newCount)
			{
				while (anchorField.anchors.Count != newCount)
					anchorField.RemoveAnchor(anchorField.anchors.Count - 1);
			}
			else if (anchorField.anchors.Count < newCount)
			{
				while (anchorField.anchors.Count != newCount)
					anchorField.CreateNewAnchor();
			}

			for (int i = 0; i < anchorField.anchors.Count; i++)
			{
				if (newNames != null && newNames.Length > i)
					anchorField.anchors[i].name = newNames[i];
				else
					anchorField.anchors[i].name = "";
			}
		}

		public void				SetAnchorPosition(string fieldName, int y, int index = 0)
		{
			PWAnchor	anchor = GetAnchorFromField(fieldName, index);

			if (anchor != null)
				anchor.forcedY = y;
		}

		public int				GetAnchorLinkCount(string fieldName, int index = 0)
		{
			PWAnchor	anchor = GetAnchorFromField(fieldName, index);

			if (anchor == null)
				return -1;
			else
				return anchor.linkCount;
		}

		public Rect?			GetAnchorRect(string fieldName, int index = 0)
		{
			PWAnchor	anchor = GetAnchorFromField(fieldName, index);

			if (anchor == null)
				return null;
			else
				return anchor.rect;
		}

		public IEnumerable< PWNode > 	GetOutputNodes()
		{
			foreach (var outputAnchor in outputAnchors)
				foreach (var link in outputAnchor.links)
					yield return link.toNode;
		}

		public IEnumerable< PWNode >	GetInputNodes()
		{
			foreach (var anchor in inputAnchors)
				foreach (var link in anchor.links)
					yield return link.fromNode;
		}

		public PWAnchor					GetAnchor(string fieldName, int index = 0)
		{
			return GetAnchorFromField(fieldName, index);
		}

		public IEnumerable< PWNode >	GetNodesAttachedToAnchor(PWAnchor anchor)
		{
			//TODO bake these data to prevent GC
			return (anchor.anchorType == PWAnchorType.Input) ?
				from l in anchor.links select l.toNode : 
				from l in anchor.links select l.fromNode;
		}
	}
}
