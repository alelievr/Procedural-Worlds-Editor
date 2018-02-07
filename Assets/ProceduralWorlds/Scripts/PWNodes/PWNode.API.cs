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

		public void				SetAnchorEnabled(string fieldName, bool enabled, int index = 0, bool removeLink = true)
		{
			PWAnchor	anchor = GetAnchorFromField(fieldName, index);

			if (anchor != null)
			{
				if (enabled == false && removeLink)
					anchor.RemoveAllLinks();
				anchor.enabled = enabled;
			}
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

		public void				SetAnchorVisibility(string fieldName, PWVisibility visibility, int index = 0, bool removeLink = true)
		{
			PWAnchor	anchor = GetAnchorFromField(fieldName, index);

			if (anchor != null)
			{
				if (visibility != PWVisibility.Visible && removeLink)
					anchor.RemoveAllLinks();
				anchor.visibility = visibility;
			}
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

		public void				SetAnchorValue(string fieldName, object value, int index = 0)
		{
			SetAnchorValue(GetAnchorFromField(fieldName, index), value);
		}

		public void				SetAnchorValue(PWAnchor anchor, object value)
		{
			if (anchor != null && anchorFieldInfoMap.ContainsKey(anchor.fieldName))
			{
				var fieldInfo = anchorFieldInfoMap[anchor.fieldName];

				if (anchor.anchorFieldRef.multiple)
				{
					var SetValue = fieldInfo.FieldType.GetMethod("AssignAt");
					SetValue.Invoke(fieldInfo.GetValue(this), new object[]{anchor.fieldIndex, value, anchor.name, true});
				}
				else
					fieldInfo.SetValue(this, value);
			}
			else
				Debug.LogError("Can't set value " + value + " to anchor " + anchor);
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
					if (link.toNode != null)
						yield return link.toNode;
		}

		public IEnumerable< PWNode >	GetInputNodes()
		{
			foreach (var anchor in inputAnchors)
				foreach (var link in anchor.links)
					if (link.fromNode != null)
						yield return link.fromNode;
		}

		public PWAnchor					GetAnchor(string fieldName, int index = 0)
		{
			return GetAnchorFromField(fieldName, index);
		}

		public object					GetAnchorValue(string fieldName, int index)
		{
			PWAnchor	anchor = GetAnchorFromField(fieldName, index);
			return GetAnchorValue(anchor);
		}

		public object					GetAnchorValue(PWAnchor anchor)
		{
			if (anchor != null && anchorFieldInfoMap.ContainsKey(anchor.fieldName))
			{
				var fieldInfo = anchorFieldInfoMap[anchor.fieldName];

				if (anchor.anchorFieldRef.multiple)
				{
					var at = fieldInfo.FieldType.GetMethod("At");
					return at.Invoke(fieldInfo.GetValue(this), new object[]{anchor.fieldIndex});
				}
				else
					return fieldInfo.GetValue(this);
			}
			else
				Debug.LogError("Can't get value from anchor " + anchor);
			
			return null;
		}

		public IEnumerable< PWNode >	GetNodesAttachedToAnchor(PWAnchor anchor)
		{
			return (anchor.anchorType == PWAnchorType.Input) ?
				from l in anchor.links select l.fromNode : 
				from l in anchor.links select l.toNode;
		}
		
		public void	RemoveSelf()
		{
			RemoveAllLinks();

			//the node instance will be removed by the editor at the same time that it's asset
		
			ScriptableObject.DestroyImmediate(this, true);
		}
	}
}
