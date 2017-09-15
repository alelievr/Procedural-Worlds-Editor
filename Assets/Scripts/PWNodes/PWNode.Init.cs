using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using PW.Core;

//Initialization and data baking for PWNode
namespace PW
{
	public partial class PWNode
	{
		//backed datas about properties and nodes
		[System.Serializable]
		public class PropertyDataDictionary : SerializableDictionary< string, PWAnchorField > {}
		[SerializeField]
		protected PropertyDataDictionary			propertyDatas = new PropertyDataDictionary();

		[NonSerializedAttribute]
		protected Dictionary< string, FieldInfo >	bakedNodeFields = new Dictionary< string, FieldInfo >();
	
		void LoadAssets()
		{
			editIcon = Resources.Load< Texture2D >("ic_edit");
		}

		void LoadStyles()
		{
			renameNodeTextFieldStyle = GUI.skin.FindStyle("RenameNodetextField");
			innerNodePaddingStyle = GUI.skin.FindStyle("WindowInnerPadding");
			debugStyle = GUI.skin.FindStyle("Debug");
		}
		
		void BakeNodeFields()
		{
			System.Reflection.FieldInfo[] fInfos = GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

			bakedNodeFields.Clear();
			foreach (var fInfo in fInfos)
				bakedNodeFields[fInfo.Name] = fInfo;
		}

		void LoadFieldAttributes()
		{
			//TODO: generate one PWAnchorField per actual fields.
			//get input variables
			System.Reflection.FieldInfo[] fInfos = GetType().GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

			List< string > actualFields = new List< string >();
			foreach (var field in fInfos)
			{
				actualFields.Add(field.Name);
				if (!propertyDatas.ContainsKey(field.Name))
					propertyDatas[field.Name] = new PWAnchorField();
				
				PWAnchorField	anchorField = propertyDatas[field.Name];
				PWAnchorType	anchorType = PWAnchorType.None;
				string			name = "";
				Vector2			offset = Vector2.zero;
				int				multiPadding = 0;
				
				System.Object[] attrs = field.GetCustomAttributes(true);
				foreach (var attr in attrs)
				{
					PWInputAttribute		inputAttr = attr as PWInputAttribute;
					PWOutputAttribute		outputAttr = attr as PWOutputAttribute;
					PWColorAttribute		colorAttr = attr as PWColorAttribute;
					PWOffsetAttribute		offsetAttr = attr as PWOffsetAttribute;
					PWMultipleAttribute		multipleAttr = attr as PWMultipleAttribute;
					PWGenericAttribute		genericAttr = attr as PWGenericAttribute;
					PWNotRequiredAttribute	notRequiredAttr = attr as PWNotRequiredAttribute;

					if (inputAttr != null)
					{
						anchorType = PWAnchorType.Input;
						if (inputAttr.name != null)
						{
							name = inputAttr.name;
							anchorField.anchorName = inputAttr.name;
						}
					}
					if (outputAttr != null)
					{
						anchorType = PWAnchorType.Output;
						if (outputAttr.name != null)
						{
							name = outputAttr.name;
							anchorField.anchorName = outputAttr.name;
						}
					}
					if (colorAttr != null)
						anchorField.color = new SerializableColor(colorAttr.color);
					if (offsetAttr != null)
					{
						anchorField.offset = offsetAttr.offset;
						multiPadding = offsetAttr.multiPadding;
					}
					if (multipleAttr != null)
					{
						//check if field is PWValues type otherwise do not implement multi-anchor
						anchorField.allowedTypes = multipleAttr.allowedTypes;
						anchorField.minMultipleValues = multipleAttr.minValues;
						anchorField.maxMultipleValues = multipleAttr.maxValues;
					}
					if (genericAttr != null)
						anchorField.allowedTypes = genericAttr.allowedTypes;
					if (notRequiredAttr != null)
						anchorField.required = false;
				}
				if (anchorType == PWAnchorType.None) //field does not have a PW attribute
					propertyDatas.Remove(field.Name);
				else
				{
					anchorField.CreateNewAnchor();
					anchorField.fieldName = field.Name;
					anchorField.anchorType = anchorType;
					anchorField.fieldType = (SerializableType)field.FieldType;
					anchorField.nodeRef = this;
				}
			}

			UpdateDependentStatus();

			//remove inhexistants field dictionary entries (for renamed variables):
			var toRemoveKeys = new List< string >();
			foreach (var kp in propertyDatas)
				if (!actualFields.Contains(kp.Key))
					toRemoveKeys.Add(kp.Key);
			foreach (var toRemoveKey in toRemoveKeys)
				propertyDatas.Remove(toRemoveKey);
		}

		//retarget "Reload" button in the editor to the internal event OnReload:
		void ReloadCallback() { OnReload(null); }

		void ForceReloadCallback() { /*TODO*/ }

		void LinkDragged(PWNodeLink link)
		{
			//disable non-linkable anchors:
			DisableUnlinkableAnchors(link);
		}

		void LinkCanceled(PWNodeLink link)
		{
			//reset the highlight mode on anchors:
			ResetUnlinkableAnchors();
		}

		void DraggedLinkOverAnchorCallback(PWAnchor anchor)
		{
			if (anchor.anchorType == PWAnchorType.Input)
			{
				if (anchor.linkCount == 1)
					anchor.highlighMode = PWAnchorHighlight.AttachReplace;
				else
					anchor.highlighMode = PWAnchorHighlight.AttachNew;
			}
			else
			{
				if (anchor.linkCount > 0)
					anchor.highlighMode = PWAnchorHighlight.AttachAdd;
				else
					anchor.highlighMode = PWAnchorHighlight.AttachNew;
			}
			
			//TODO: snap the dragged link
		}

		void DraggedLinkQuitAnchorCallbck(PWAnchor anchor)
		{
			//TODO: update the anchor highlight
			anchor.highlighMode = PWAnchorHighlight.None;
		}

		void AnchorLinkedCallback(PWAnchor anchor)
		{
			//TODO: create the link

			//update canWork bool
			UpdateWorkStatus();
			// OnLinkCreated(link);
		}

		void AnchorUnlinkedCallback(PWAnchor anchor)
		{
			//TODO: unlink the anchor, remove the link.

			//raise internal event 
			// OnLinkRemoved(link);
		}

		void LinkSelectedCallback(PWNodeLink link)
		{
			Debug.Log("Link slected");
		}

		void LinkUnselectedCallback(PWNodeLink link)
		{
			Debug.Log("link unselected");
		}

		void NodeSelectedCallback(PWNode node)
		{
			if (node == this)
				selected = true;
		}

		void NodeUnselectedCallback(PWNode node)
		{
			if (node == this)
				selected = false;
		}

		void BindEvents()
		{
			//graph events:
			//if the node is used in a PWMainGraph:
			if (mainGraphRef != null)
			{
				mainGraphRef.OnReload += ReloadCallback;
				mainGraphRef.OnForceReload += ForceReloadCallback;
			}
			graphRef.OnClickNowhere += OnClickedOutside;
			graphRef.OnLinkDragged += LinkDragged;
			graphRef.OnLinkCanceled += LinkCanceled;
			graphRef.OnNodeSelected += NodeSelectedCallback;
			graphRef.OnNodeUnselected += NodeUnselectedCallback;

			//node events:
			OnDraggedLinkOverAnchor += DraggedLinkOverAnchorCallback;
			OnDraggedLinkQuitAnchor += DraggedLinkQuitAnchorCallbck;
			OnLinkSelected += LinkSelectedCallback;
			OnLinkUnselected += LinkUnselectedCallback;
			OnAnchorLinked += AnchorLinkedCallback;
			OnAnchorUnlinked += AnchorUnlinkedCallback;
		}

		void UnBindEvents()
		{
			//if the node is used in a PWMainGraph:
			if (mainGraphRef != null)
			{
				mainGraphRef.OnReload -= ReloadCallback;
				mainGraphRef.OnForceReload -= ForceReloadCallback;
			}
			graphRef.OnClickNowhere -= OnClickedOutside;
			graphRef.OnLinkDragged -= LinkDragged;
			graphRef.OnLinkCanceled -= LinkCanceled;
			graphRef.OnNodeSelected -= NodeSelectedCallback;
			graphRef.OnNodeUnselected -= NodeUnselectedCallback;
			
			//node events:
			OnDraggedLinkOverAnchor -= DraggedLinkOverAnchorCallback;
			OnDraggedLinkQuitAnchor -= DraggedLinkQuitAnchorCallbck;
			OnLinkSelected -= LinkSelectedCallback;
			OnLinkUnselected -= LinkUnselectedCallback;
			OnAnchorUnlinked -= AnchorUnlinkedCallback;
			OnAnchorLinked -= AnchorLinkedCallback;
		}
	}
}