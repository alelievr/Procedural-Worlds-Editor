using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using System.Reflection;
using ProceduralWorlds.Core;

using ProceduralWorlds.Node;

//Initialization and data baking for BaseNode
namespace ProceduralWorlds
{
	public partial class BaseNode
	{
		//anchor 
		[System.Serializable]
		public class AnchorFieldDictionary : SerializableDictionary< string, AnchorField > {}
		[SerializeField]
		public AnchorFieldDictionary		anchorFieldDictionary = new AnchorFieldDictionary();

		[System.NonSerialized]
		Dictionary< string, FieldInfo >		anchorFieldInfoMap = new Dictionary< string, FieldInfo >();
		
		void LoadFieldAttributes()
		{
			//get input variables
			System.Reflection.FieldInfo[] fInfos = GetType().GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

			List< string > actualFields = new List< string >();

			anchorFieldInfoMap.Clear();
				
			foreach (var field in fInfos)
			{
				// reading field informations.

				actualFields.Add(field.Name);
				anchorFieldInfoMap[field.Name] = field;

				if (!anchorFieldDictionary.ContainsKey(field.Name))
					anchorFieldDictionary[field.Name] = CreateAnchorField();
				
				AnchorField	anchorField = anchorFieldDictionary[field.Name];
				
				//detect multi-anchor by checking for PWArray<T> type
				if (field.FieldType.IsGenericType)
				{
					if (field.FieldType.GetGenericTypeDefinition() == typeof(PWArray<>))
						anchorField.multiple = true;
				}
				
				System.Object[] attrs = field.GetCustomAttributes(true);
				foreach (var attr in attrs)
				{
					InputAttribute		inputAttr = attr as InputAttribute;
					OutputAttribute		outputAttr = attr as OutputAttribute;
					ColorAttribute		colorAttr = attr as ColorAttribute;
					OffsetAttribute		offsetAttr = attr as OffsetAttribute;
					NotRequiredAttribute	notRequiredAttr = attr as NotRequiredAttribute;

					if (inputAttr != null)
					{
						anchorField.anchorType = AnchorType.Input;
						if (inputAttr.name != null)
							anchorField.name = inputAttr.name;
					}
					if (outputAttr != null)
					{
						anchorField.anchorType = AnchorType.Output;
						if (outputAttr.name != null)
							anchorField.name = outputAttr.name;
					}
					if (colorAttr != null)
						anchorField.color = new SerializableColor(colorAttr.color);
					if (offsetAttr != null)
					{
						anchorField.offset = offsetAttr.offset;
						anchorField.padding = offsetAttr.padding;
					}
					if (notRequiredAttr != null)
						anchorField.required = false;
				}
				if (anchorField.anchorType == AnchorType.None) //field does not have a PW attribute
					anchorFieldDictionary.Remove(field.Name);
				else
				{
					//create anchor in this anchorField if there is not existing one
					if (anchorField.anchors.Count == 0)
						anchorField.CreateNewAnchor();

					anchorField.colorSchemeName = ColorTheme.GetAnchorColorSchemeName(field.FieldType);
					anchorField.fieldName = field.Name;
					anchorField.fieldType = (SerializableType)field.FieldType;
					anchorField.fieldValue = field.GetValue(this);
					anchorField.nodeRef = this;
				}
			}

			//remove inhexistants field dictionary entries (for renamed variables):
			var toRemoveKeys = new List< string >();
			foreach (var kp in anchorFieldDictionary)
				if (!actualFields.Contains(kp.Key))
					toRemoveKeys.Add(kp.Key);
			
			foreach (var toRemoveKey in toRemoveKeys)
				anchorFieldDictionary.Remove(toRemoveKey);
		}

		void RemoveAnchorFieldDulicatedKeys()
		{
			List< string > toRemoveKeys = new List< string >();

			//remove duplicate keys in the dictionary (yes it happends ...)
			HashSet< string > duplicateKeys = new HashSet< string >();
			foreach (var kp in anchorFieldDictionary)
			{
				if (duplicateKeys.Contains(kp.Key))
					toRemoveKeys.Add(kp.Key);
				duplicateKeys.Add(kp.Key);
			}

			foreach (var toRemoveKey in toRemoveKeys)
			{
				Debug.Log("removing duplicated dictionary key: " + toRemoveKey);
				anchorFieldDictionary.Remove(toRemoveKey);
			}
		}

		void UpdateAnchorProperties()
		{
			//we check if an anchor have been update (field name changed)
			// and then remove it from the anchor list and take the new field attributes
			// we do the same for every properties
			foreach (var anchorFieldKP in anchorFieldDictionary)
			{
				var af = anchorFieldKP.Value;

				foreach (var anchor in anchorFields)
				{
					//we find the field in inputAnchors corresponding to af
					if (anchor.anchorType == af.anchorType && anchor.fieldName == af.fieldName)
					{
						//we update the values we have in our input/output anchorFileds with the new fresh field values
						anchor.name = af.name;
						anchor.fieldType = af.fieldType;
						anchor.debug = af.debug;
						anchor.offset = af.offset;
						anchor.padding = af.padding;
						anchor.colorSchemeName = af.colorSchemeName;
						anchor.color = af.color;
						anchor.required = af.required;
						anchor.multiple = af.multiple;

						//we tell that the anchor have been linked to an actual field
						anchor.fieldValidated = true;
						af.fieldValidated = true;
					}
				}
			}
			
			//we remove every anchors that is not anymore linked with a field
			inputAnchorFields.RemoveAll(a => !a.fieldValidated);
			outputAnchorFields.RemoveAll(a => !a.fieldValidated);

			//we add new fields to the input/output anchors list
			foreach (var anchorFieldKP in anchorFieldDictionary)
			{
				var af = anchorFieldKP.Value;
				//if the field was not found in both input and output anchor lists
				if (!af.fieldValidated)
				{
					if (af.anchorType == AnchorType.Input)
						inputAnchorFields.Add(af);
					else
						outputAnchorFields.Add(af);
				}
			}
		}

		void LinkRemovedCalllback()
		{
			UpdateWorkStatus();
		}

		void BindEvents()
		{
			//graph events:
			graphRef.OnPostLinkRemoved += LinkRemovedCalllback;
		}

		void UnBindEvents()
		{
			//graph events:
			//null check because this function may be called without the node being initialized
			if (graphRef != null)
			{
				graphRef.OnPostLinkRemoved -= LinkRemovedCalllback;
			}
		}
	}
}