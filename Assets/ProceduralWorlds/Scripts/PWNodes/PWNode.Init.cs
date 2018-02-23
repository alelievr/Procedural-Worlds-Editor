using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using System.Reflection;
using PW.Core;

using PW.Node;

//Initialization and data baking for PWNode
namespace PW
{
	public partial class PWNode
	{
		//anchor 
		[System.Serializable]
		protected class AnchorFieldDictionary : SerializableDictionary< string, PWAnchorField > {}
		[SerializeField]
		protected AnchorFieldDictionary		anchorFieldDictionary = new AnchorFieldDictionary();

		[System.NonSerialized]
		Dictionary< string, FieldInfo >		anchorFieldInfoMap = new Dictionary< string, FieldInfo >();
		
		[System.NonSerialized]
		public List< FieldInfo >			undoableFields = new List< FieldInfo >();
		
		void LoadUndoableFields()
		{
			System.Reflection.FieldInfo[] fInfos = GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

			undoableFields.Clear();
			
			foreach (var fInfo in fInfos)
			{
				var attrs = fInfo.GetCustomAttributes(false);

				bool hasSerializeField = false;
				bool hasNonSerialized = false;

				foreach (var attr in attrs)
				{
					if (attr as PWInputAttribute != null || attr as PWOutputAttribute != null)
						goto skipThisField;
					
					if (attr is System.NonSerializedAttribute)
						hasNonSerialized = true;
					
					if (attr is SerializeField)
						hasSerializeField = true;
				}

				if (fInfo.IsPrivate && !hasSerializeField)
					goto skipThisField;
				
				if (hasNonSerialized)
					goto skipThisField;
				
				if (fInfo.IsNotSerialized)
					goto skipThisField;
				
				undoableFields.Add(fInfo);

				skipThisField:
				continue ;
			}
		}

		void LoadFieldAttributes()
		{
			//get input variables
			System.Reflection.FieldInfo[] fInfos = GetType().GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

			List< string > actualFields = new List< string >();
				
			foreach (var field in fInfos)
			{
				// reading field informations.

				actualFields.Add(field.Name);
				anchorFieldInfoMap[field.Name] = field;

				if (!anchorFieldDictionary.ContainsKey(field.Name))
					anchorFieldDictionary[field.Name] = CreateAnchorField();
				
				PWAnchorField	anchorField = anchorFieldDictionary[field.Name];
				
				//detect multi-anchor by checking for PWArray<T> type
				if (field.FieldType.IsGenericType)
				{
					if (field.FieldType.GetGenericTypeDefinition() == typeof(PWArray<>))
						anchorField.multiple = true;
				}
				
				System.Object[] attrs = field.GetCustomAttributes(true);
				foreach (var attr in attrs)
				{
					PWInputAttribute		inputAttr = attr as PWInputAttribute;
					PWOutputAttribute		outputAttr = attr as PWOutputAttribute;
					PWColorAttribute		colorAttr = attr as PWColorAttribute;
					PWOffsetAttribute		offsetAttr = attr as PWOffsetAttribute;
					PWNotRequiredAttribute	notRequiredAttr = attr as PWNotRequiredAttribute;

					if (inputAttr != null)
					{
						anchorField.anchorType = PWAnchorType.Input;
						if (inputAttr.name != null)
							anchorField.name = inputAttr.name;
					}
					if (outputAttr != null)
					{
						anchorField.anchorType = PWAnchorType.Output;
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
				if (anchorField.anchorType == PWAnchorType.None) //field does not have a PW attribute
					anchorFieldDictionary.Remove(field.Name);
				else
				{
					//create anchor in this anchorField if there is not existing one
					if (anchorField.anchors.Count == 0)
						anchorField.CreateNewAnchor();

					anchorField.colorSchemeName = PWColorTheme.GetAnchorColorSchemeName(field.FieldType);
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
						
			//remove duplicate keys in the dictionary (yes it happends ...)
			HashSet< string > duplicateKeys = new HashSet< string >();
			foreach (var kp in anchorFieldDictionary)
			{
				// if (duplicateKeys.Contains(kp.Key))
					// toRemoveKeys.Add(kp.Key);
				duplicateKeys.Add(kp.Key);
			}
			
			foreach (var toRemoveKey in toRemoveKeys)
				anchorFieldDictionary.Remove(toRemoveKey);
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
					if (af.anchorType == PWAnchorType.Input)
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