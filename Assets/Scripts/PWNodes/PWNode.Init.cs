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
		public class PropertyDataDictionary : SerializableDictionary< string, PWAnchorData > {}
		[SerializeField]
		protected PropertyDataDictionary			propertyDatas = new PropertyDataDictionary();

		[NonSerializedAttribute]
		protected Dictionary< string, FieldInfo >	bakedNodeFields = new Dictionary< string, FieldInfo >();
	
		void LoadAssets()
		{
			Func< string, Texture2D > CreateTexture2DFromFile = (string ressourcePath) => {
				return Resources.Load< Texture2D >(ressourcePath);
			};
			
			if (errorIcon == null)
			{
				errorIcon = CreateTexture2DFromFile("ic_error");
				editIcon = CreateTexture2DFromFile("ic_edit");
				nodeAutoProcessModeIcon = CreateTexture2DFromFile("ic_autoProcess");
				nodeRequestForProcessIcon = CreateTexture2DFromFile("ic_requestForProcess");
			}
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
					propertyDatas[field.Name] = new PWAnchorData(field.Name, field.Name.GetHashCode());
				
				PWAnchorData	data = propertyDatas[field.Name];
				Color			backgroundColor = PWColorScheme.GetAnchorColorByType(field.FieldType);
				PWAnchorType	anchorType = PWAnchorType.None;
				string			name = "";
				Vector2			offset = Vector2.zero;
				int				multiPadding = 0;
				
				//default anchor values
				data.multiple = false;
				data.generic = false;
				data.required = true;

				data.anchorInstance = field.GetValue(this);
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
							data.anchorName = inputAttr.name;
						}
					}
					if (outputAttr != null)
					{
						anchorType = PWAnchorType.Output;
						if (outputAttr.name != null)
						{
							name = outputAttr.name;
							data.anchorName = outputAttr.name;
						}
					}
					if (colorAttr != null)
						backgroundColor = colorAttr.color;
					if (offsetAttr != null)
					{
						offset = offsetAttr.offset;
						multiPadding = offsetAttr.multiPadding;
					}
					if (multipleAttr != null)
					{
						//check if field is PWValues type otherwise do not implement multi-anchor
						data.generic = true;
						data.multiple = true;
						data.allowedTypes = multipleAttr.allowedTypes;
						data.minMultipleValues = multipleAttr.minValues;
						data.maxMultipleValues = multipleAttr.maxValues;
					}
					if (genericAttr != null)
					{
						data.allowedTypes = genericAttr.allowedTypes;
						data.generic = true;
					}
					if (notRequiredAttr != null)
						data.required = false;
				}
				if (anchorType == PWAnchorType.None) //field does not have a PW attribute
					propertyDatas.Remove(field.Name);
				else
				{
					if (data.required && anchorType == PWAnchorType.Output)
						data.required = false;
					//protection against stupid UpdateMultiProp remove all anchors
					if (data.multiple && data.multi.Count == 0)
						data.AddNewAnchor(backgroundColor, field.Name.GetHashCode());
					data.classAQName = GetType().AssemblyQualifiedName;
					data.fieldName = field.Name;
					data.anchorType = anchorType;
					data.type = (SerializableType)field.FieldType;
					data.first.color = (SerializableColor)backgroundColor;
					data.first.linkType = GetLinkTypeFromType(field.FieldType);
					data.first.name = name;
					data.offset = offset;
					data.nodeId = id;
					data.multiPadding = multiPadding;

					//add missing values to instance of list:

					if (data.multiple && data.anchorInstance != null)
					{
						//add minimum number of anchors to render:
						if (data.multipleValueCount < data.minMultipleValues)
							for (int i = data.multipleValueCount; i < data.minMultipleValues; i++)
								data.AddNewAnchor(backgroundColor, field.Name.GetHashCode() + i + 1);

						var PWValuesInstance = data.anchorInstance as PWValues;

						if (PWValuesInstance != null)
							while (PWValuesInstance.Count < data.multipleValueCount)
								PWValuesInstance.Add(null);
					}
					else if (data.multiple)
						Debug.LogWarning("Uninitialized PWmultiple field in class " + GetType() + ": " + data.fieldName);
				}
			}

			UpdateDependentStatus();

			//remove inhexistants dictionary entries:
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

		//retarget OnNodeLinkedCallback if this node was linked:
		void NodeLinkedCallback(PWNodeLink link)
		{
			if (link.fromNode == this || link.toNode == this)
				OnNodeLinked();
		}

		//retarget OnNodeUnlinkedCallback if this node was linked:
		void NodeUnlinkedCallback(PWNodeLink link)
		{
			if (link.fromNode == this || link.toNode == this)
				OnNodeUnlinked();
		}

		void BindEvents()
		{
			//if the node is used in a PWMainGraph:
			if (mainGraphRef != null)
			{
				mainGraphRef.OnReload += ReloadCallback;
				mainGraphRef.OnForceReload += ForceReloadCallback;
			}
			graphRef.OnClickNowhere += OnClickedOutside;
			graphRef.OnNodeLinked += NodeLinkedCallback;
			graphRef.OnNodeUnlinked += NodeUnlinkedCallback;
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
			graphRef.OnNodeLinked -= NodeLinkedCallback;
			graphRef.OnNodeUnlinked -= NodeUnlinkedCallback;
		}
	}
}