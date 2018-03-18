using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
using ProceduralWorlds.Core;
using ProceduralWorlds.Node;

//Utils for BaseNode class
namespace ProceduralWorlds
{
	public partial class BaseNode
	{
		List< ReflectionUtils.GenericField > clonableFields = new List< ReflectionUtils.GenericField >();
		
		void LoadClonableFields()
		{
			clonableFields.Clear();

			Type[] unserializableAttributes = {
				typeof(InputAttribute),
				typeof(OutputAttribute),
				typeof(System.NonSerializedAttribute),
			};

			var fields = GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

			foreach (var field in fields)
			{
				var attrs = field.GetCustomAttributes(false);

				if (attrs.Any(a => unserializableAttributes.Contains(a.GetType())))
					continue ;
				
				if (field.IsPrivate && !attrs.Any(a => a is SerializeField))
					continue ;
				
				clonableFields.Add(ReflectionUtils.CreateGenericField(GetType(), field.Name));
			}
		}

		public IEnumerable< Anchor > inputAnchors
		{
			get
			{
				foreach (var anchorField in inputAnchorFields)
					foreach (var anchor in anchorField.anchors)
						yield return anchor;
			}
		}
		
		public IEnumerable< Anchor > outputAnchors
		{
			get
			{
				foreach (var anchorField in outputAnchorFields)
					foreach (var anchor in anchorField.anchors)
						yield return anchor;
			}
		}
		
		public void Duplicate()
		{
			var newNode = graphRef.CreateNewNode(GetType(), rect.position + new Vector2(50, 50), name);

			//copy internal datas to the new node:
			foreach (var field in clonableFields)
			{
				var value = field.GetValue(this);
				field.SetValue(newNode, value);
			}
		}
	}
}